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

namespace TerrariaTogether.Modules.Rules
{
	// welcome message and rules on join, and who has accepted which version of the rules
	public class RulesSystem : ModSystem
	{
		private const int SaveVersion = 1;
		private const string Key = "Mods.TerrariaTogether.Rules.";

		private const byte MsgEntered = 0;
		private const byte MsgAcceptState = 1;

		// the rules as last seen, the version goes up whenever they change so everyone sees and accepts them again
		private string rulesText = "";
		private int rulesVersion;

		// uuid -> rules version the player last saw / accepted
		private readonly Dictionary<string, int> seen = new();
		private readonly Dictionary<string, int> accepted = new();

		// client only: whether the server says we accepted the current rules
		private bool localAccepted;

		public static RulesSystem Instance => ModContent.GetInstance<RulesSystem>();

		private static RulesConfig Config => ModContent.GetInstance<RulesConfig>();

		// client only: building is blocked until the rules are accepted
		public bool LocalBlocked => RulesModule.IsEnabled && Config.MustAccept() && !localAccepted;

		// server: whether this player may build
		public bool HasAccepted(Player player)
		{
			if (!RulesModule.IsEnabled || !Config.RequireAccept)
				return true;
			string uuid = PresenceTracker.Instance.GetUuid(player);
			return (uuid != null && accepted.GetValueOrDefault(uuid, -1) == rulesVersion) || !Config.MustAccept();
		}

		// bumps the version if an admin changed the rules, returns true when it did
		private bool UpdateRulesVersion()
		{
			string text = string.Join("\n", Config.ActiveRules());
			if (text == rulesText)
				return false;
			rulesText = text;
			rulesVersion++;
			return true;
		}

		public override void OnWorldLoad()
		{
			UpdateRulesVersion();
		}

		// server (or single player): the player finished joining
		public void OnEntered(int who)
		{
			Player player = Main.player[who];
			RulesConfig config = Config;
			if (config.ShowWelcome && !string.IsNullOrWhiteSpace(config.WelcomeMessage))
				Send(who, config.WelcomeMessage.Replace("{name}", player.name).Replace("{world}", Main.worldName), Color.Gold);

			string uuid = PresenceTracker.Instance.GetUuid(player);
			if (uuid == null)
				return;

			// rules show on first join, after they change, and on every join until accepted
			bool needsAccept = Main.netMode == NetmodeID.Server && !HasAccepted(player);
			if (config.ActiveRules().Count > 0 && (needsAccept || seen.GetValueOrDefault(uuid, -1) != rulesVersion))
				ShowRules(who, needsAccept);
			SendAcceptState(who);
		}

		private void ShowRules(int who, bool needsAccept)
		{
			foreach (string line in RuleLines())
				Send(who, line, Color.White);
			if (needsAccept)
				Send(who, Language.GetTextValue(Key + "AcceptHint"), Color.Gold);

			string uuid = PresenceTracker.Instance.GetUuid(Main.player[who]);
			if (uuid != null)
				seen[uuid] = rulesVersion;
		}

		// header plus numbered rules, empty when there are none
		public List<string> RuleLines()
		{
			List<string> rules = Config.ActiveRules();
			if (rules.Count == 0)
				return rules;

			var lines = new List<string> { Language.GetTextValue(Key + "Header") };
			lines.AddRange(rules.Select((rule, i) => Language.GetTextValue(Key + "Line", i + 1, rule)));
			return lines;
		}

		public void Accept(Player player)
		{
			string uuid = PresenceTracker.Instance.GetUuid(player);
			if (uuid == null)
				return;
			accepted[uuid] = rulesVersion;
			seen[uuid] = rulesVersion;
			Mod.Logger.Info($"rules: {player.name} accepted version {rulesVersion}");
			SendAcceptState(player.whoAmI);
		}

		// server: changed rules are shown to everyone online, and clients learn whether they still count as accepted
		public void OnConfigChanged()
		{
			if (Main.netMode != NetmodeID.Server || !UpdateRulesVersion() || !RulesModule.IsEnabled)
				return;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (!player.active || Config.ActiveRules().Count == 0)
					continue;
				Send(i, Language.GetTextValue(Key + "Changed"), Color.Gold);
				ShowRules(i, !HasAccepted(player));
				SendAcceptState(i);
			}
		}

		private void SendAcceptState(int who)
		{
			if (Main.netMode != NetmodeID.Server)
				return;
			string uuid = PresenceTracker.Instance.GetUuid(Main.player[who]);
			ModPacket packet = SmpPackets.Create(RulesModule.ModuleId, MsgAcceptState);
			packet.Write(uuid != null && accepted.GetValueOrDefault(uuid, -1) == rulesVersion);
			packet.Send(who);
		}

		// chat to one player, works on the server and in single player
		private static void Send(int who, string text, Color color)
		{
			if (Main.netMode == NetmodeID.Server)
				ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(text), color, who);
			else
				Main.NewText(text, color);
		}

		public void SendEntered()
		{
			localAccepted = false;
			SmpPackets.Create(RulesModule.ModuleId, MsgEntered).Send();
		}

		public void HandlePacket(byte msgType, BinaryReader reader, int fromWho)
		{
			switch (msgType)
			{
				case MsgEntered when Main.netMode == NetmodeID.Server:
					OnEntered(fromWho);
					break;
				case MsgAcceptState when Main.netMode == NetmodeID.MultiplayerClient:
					localAccepted = reader.ReadBoolean();
					break;
			}
		}

		// the real block: drops tile edits from players who haven't accepted, in case a client skips its own check
		// ponytail: covers tile/wall place and break, not paint, wires or liquids
		public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			if (messageType != MessageID.TileManipulation && messageType != MessageID.PlaceObject)
				return false;
			if (Main.netMode != NetmodeID.Server || playerNumber < 0 || playerNumber >= Main.maxPlayers)
				return false;
			return !HasAccepted(Main.player[playerNumber]);
		}

		// saved even while the module is off, so turning it back on loses nothing
		public override void SaveWorldData(TagCompound tag)
		{
			tag["rules"] = new TagCompound
			{
				["version"] = SaveVersion,
				["rulesText"] = rulesText,
				["rulesVersion"] = rulesVersion,
				["seen"] = ToTags(seen),
				["accepted"] = ToTags(accepted),
			};
		}

		public override void LoadWorldData(TagCompound tag)
		{
			ClearWorld();
			if (!tag.TryGet("rules", out TagCompound saved))
				return;

			// version 1 is the only shape so far, migrate here when it changes
			rulesText = saved.GetString("rulesText");
			rulesVersion = saved.GetInt("rulesVersion");
			FromTags(saved.GetList<TagCompound>("seen"), seen);
			FromTags(saved.GetList<TagCompound>("accepted"), accepted);
		}

		private static List<TagCompound> ToTags(Dictionary<string, int> versions)
		{
			return versions.Select(v => new TagCompound { ["uuid"] = v.Key, ["v"] = v.Value }).ToList();
		}

		private static void FromTags(IList<TagCompound> tags, Dictionary<string, int> versions)
		{
			foreach (TagCompound t in tags)
				versions[t.GetString("uuid")] = t.GetInt("v");
		}

		public override void ClearWorld()
		{
			rulesText = "";
			rulesVersion = 0;
			seen.Clear();
			accepted.Clear();
			localAccepted = false;
		}
	}

	// tells the server we've joined once the world is loaded, so the welcome doesn't land on a loading screen
	public class RulesPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				RulesSystem.Instance.SendEntered();
			else if (Main.netMode == NetmodeID.SinglePlayer && RulesModule.IsEnabled)
				RulesSystem.Instance.OnEntered(Main.myPlayer);
		}
	}

	// client side check so blocks aren't placed then snapped back, the server check is in RulesSystem
	public class RulesGlobalItem : GlobalItem
	{
		// static because globals are shared, tmodloader refuses instance fields without InstancePerEntity
		private static uint lastMessageTick;

		public override bool CanUseItem(Item item, Player player)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient || player.whoAmI != Main.myPlayer || !IsBuildingItem(item) || !RulesSystem.Instance.LocalBlocked)
				return true;

			if (Main.GameUpdateCount - lastMessageTick > 60)
			{
				lastMessageTick = Main.GameUpdateCount;
				Main.NewText(Language.GetTextValue("Mods.TerrariaTogether.Rules.BuildBlocked"), Color.OrangeRed);
			}
			return false;
		}

		// blocks, walls, furniture, pickaxes, axes and hammers
		private static bool IsBuildingItem(Item item)
		{
			return item.createTile >= 0 || item.createWall > 0 || item.pick > 0 || item.axe > 0 || item.hammer > 0;
		}
	}
}
