using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaTogether.Core.Networking;
using TerrariaTogether.Core.Roster;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating
{
	// works out lock state, saves admin overrides and keeps clients in sync so they can check locally
	public class GatingSystem : ModSystem
	{
		private const int SaveVersion = 1;

		private const byte MsgCounts = 0;
		private const byte MsgRequestSync = 1;
		private const byte MsgOverrides = 2;

		// boss key -> override, unknown keys are kept so removed mods don't lose their overrides
		private readonly Dictionary<string, GateOverride> overrides = new();

		// client only: counts synced from the server
		private int syncedOnline;
		private int syncedTotal;

		// server only: last counts broadcast and last announced state, to spot changes
		private int sentOnline = -1;
		private int sentTotal = -1;
		private bool? announcedLocked;

		// server only: the last boss spawn a player asked for (altar, summon item), so a veto can refund them
		private int requestPlayer = -1;
		private int requestNpcType;
		private uint requestTick;

		public static GatingSystem Instance => ModContent.GetInstance<GatingSystem>();

		private static GatingConfig Config => ModContent.GetInstance<GatingConfig>();

		public LockResult Evaluate(BossEntry boss)
		{
			bool gated = Config.GatedBosses.GetValueOrDefault(boss.Key, true);
			return LockEvaluator.Evaluate(Snapshot(gateOverride: GetOverride(boss.Key), gated: gated, downed: boss.IsDowned()));
		}

		// the plain threshold with no override or kill history, used for status and announcements
		public LockResult EvaluateThreshold()
		{
			return LockEvaluator.Evaluate(Snapshot(GateOverride.None, gated: true, downed: false));
		}

		private LockSnapshot Snapshot(GateOverride gateOverride, bool gated, bool downed)
		{
			// clients use synced counts, the server uses the live roster, debug values beat both
			bool server = Main.netMode == NetmodeID.Server;
			GatingConfig config = Config;
			int online = config.SimulatedOnline >= 0 ? config.SimulatedOnline : server ? RosterSystem.Instance.OnlineCount : syncedOnline;
			int total = config.SimulatedMembers >= 0 ? config.SimulatedMembers : server ? RosterSystem.Instance.TotalCount : syncedTotal;
			return new LockSnapshot(online, total, config.ThresholdPercent, config.MinOnline, gated, downed, config.FirstKillOnly, gateOverride);
		}


		// "Eye of Cthulhu is locked: 1/2 online (need 2)." or the admin-locked version
		public static NetworkText LockedText(BossEntry boss, LockResult result)
		{
			NetworkText name = NetworkText.FromKey(boss.DisplayName.Key);
			if (result.Reason == LockReason.ForcedLock)
				return NetworkText.FromKey("Mods.TerrariaTogether.Gating.LockedByAdmin", name);
			return NetworkText.FromKey("Mods.TerrariaTogether.Gating.Locked", name, result.Online, result.Total, result.Required);
		}

		public GateOverride GetOverride(string bossKey) => overrides.GetValueOrDefault(bossKey);

		// server only: saves the override, tells clients and announces it
		public void SetOverride(BossEntry boss, GateOverride gateOverride)
		{
			if (gateOverride == GateOverride.None)
				overrides.Remove(boss.Key);
			else
				overrides[boss.Key] = gateOverride;

			Mod.Logger.Info($"gating: {boss.Key} override set to {gateOverride}");
			SendOverrides(-1);
		}

		// ponytail: polls once a second instead of hooking every roster/config change, fine for a handful of players
		public override void PostUpdateEverything()
		{
			if (Main.netMode != NetmodeID.Server || !GatingModule.IsEnabled || Main.GameUpdateCount % 60 != 0)
				return;

			int online = RosterSystem.Instance.OnlineCount;
			int total = RosterSystem.Instance.TotalCount;
			if (online != sentOnline || total != sentTotal)
			{
				sentOnline = online;
				sentTotal = total;
				SendCounts(-1);
			}

			// one message for the shared threshold instead of one per boss
			LockResult result = EvaluateThreshold();
			if (announcedLocked != result.Locked)
			{
				// the first check after a world loads just records the state
				if (announcedLocked != null && Config.Announce)
				{
					string key = result.Locked ? "Mods.TerrariaTogether.Gating.Announce.ThresholdRelocked" : "Mods.TerrariaTogether.Gating.Announce.ThresholdUnlocked";
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key, result.Online, result.Total, result.Required),
						result.Locked ? Color.OrangeRed : Color.LightGreen);
				}
				announcedLocked = result.Locked;
			}
		}

		// sends the live counts, -1 means every client
		private static void SendCounts(int toClient)
		{
			ModPacket packet = SmpPackets.Create(GatingModule.ModuleId, MsgCounts);
			packet.Write(RosterSystem.Instance.OnlineCount);
			packet.Write(RosterSystem.Instance.TotalCount);
			packet.Send(toClient);
		}

		private void SendOverrides(int toClient)
		{
			if (Main.netMode != NetmodeID.Server)
				return;

			ModPacket packet = SmpPackets.Create(GatingModule.ModuleId, MsgOverrides);
			packet.Write(overrides.Count);
			foreach ((string key, GateOverride value) in overrides)
			{
				packet.Write(key);
				packet.Write((byte)value);
			}
			packet.Send(toClient);
		}

		public void HandlePacket(byte msgType, BinaryReader reader, int fromWho)
		{
			switch (msgType)
			{
				case MsgCounts when Main.netMode == NetmodeID.MultiplayerClient:
					syncedOnline = reader.ReadInt32();
					syncedTotal = reader.ReadInt32();
					break;
				case MsgOverrides when Main.netMode == NetmodeID.MultiplayerClient:
					overrides.Clear();
					int count = reader.ReadInt32();
					for (int i = 0; i < count; i++)
						overrides[reader.ReadString()] = (GateOverride)reader.ReadByte();
					break;
				case MsgRequestSync when Main.netMode == NetmodeID.Server:
					SendCounts(fromWho);
					SendOverrides(fromWho);
					break;
			}
		}

		// clients ask the server to spawn bosses with this message (summon items, golem's altar),
		// natural spawns never send it, so it tells a player's summon apart from a natural one
		public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			if (messageType != MessageID.SpawnBossUseLicenseStartEvent || Main.netMode != NetmodeID.Server)
				return false;

			long start = reader.BaseStream.Position;
			try
			{
				reader.ReadInt16(); // player index as the client claims it, playerNumber is trusted instead
				requestNpcType = reader.ReadInt16();
				requestPlayer = playerNumber;
				requestTick = Main.GameUpdateCount;
			}
			catch (Exception)
			{
				// malformed message, let terraria deal with it
			}
			finally
			{
				reader.BaseStream.Position = start;
			}
			return false;
		}

		// the player who asked for this spawn in the same tick, or -1; each request is only used once
		public int TakeSpawnRequester(int npcType)
		{
			if (requestPlayer < 0 || requestNpcType != npcType || requestTick != Main.GameUpdateCount)
				return -1;

			int player = requestPlayer;
			requestPlayer = -1;
			return player;
		}

		public void RequestSync()
		{
			SmpPackets.Create(GatingModule.ModuleId, MsgRequestSync).Send();
		}

		// saved even while the module is off, so turning it back on loses nothing
		public override void SaveWorldData(TagCompound tag)
		{
			tag["gating"] = new TagCompound
			{
				["version"] = SaveVersion,
				["overrides"] = overrides.Select(o => new TagCompound
				{
					["key"] = o.Key,
					["value"] = (byte)o.Value,
				}).ToList(),
			};
		}

		public override void LoadWorldData(TagCompound tag)
		{
			overrides.Clear();
			if (!tag.TryGet("gating", out TagCompound gating))
				return;

			// version 1 is the only shape so far, migrate here when it changes
			foreach (TagCompound o in gating.GetList<TagCompound>("overrides"))
				overrides[o.GetString("key")] = (GateOverride)o.GetByte("value");
		}

		public override void ClearWorld()
		{
			overrides.Clear();
			syncedOnline = 0;
			syncedTotal = 0;
			sentOnline = -1;
			sentTotal = -1;
			announcedLocked = null;
			requestPlayer = -1;
		}
	}

	// asks the server for counts and overrides on join, since broadcasts may land before the client is ready
	public class GatingPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient && GatingModule.IsEnabled)
				GatingSystem.Instance.RequestSync();
		}
	}
}
