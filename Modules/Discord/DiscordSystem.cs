using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Chat.Commands;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaTogether.Core.Roster;

namespace TerrariaTogether.Modules.Discord
{
	// listens for things worth posting, server only
	public class DiscordSystem : ModSystem
	{
		private const int SaveVersion = 1;
		private const string Key = "Mods.TerrariaTogether.Discord.";

		// embed bar colors, 0xRRGGBB
		private const int JoinColor = 0x57F287;
		private const int LeaveColor = 0x99AAB5;
		private const int ChatColor = 0x5865F2;
		private const int DeathColor = 0xED4245;
		private const int BossColor = 0xFEE75C;

		private DiscordWebhook webhook;

		// bosses this world has beaten while the module was on, so only the first kill is posted
		private readonly HashSet<string> defeated = new();

		// boss names and helpers of the fight in progress, bosses made of several npcs (the twins) post once
		private readonly SortedSet<string> fightBosses = new();
		private readonly SortedSet<string> fightHelpers = new();

		public static DiscordSystem Instance => ModContent.GetInstance<DiscordSystem>();

		private static DiscordConfig Config => ModContent.GetInstance<DiscordConfig>();

		private static bool Active => Main.netMode == NetmodeID.Server && DiscordModule.IsEnabled;

		public override void Load()
		{
			webhook = new DiscordWebhook(Mod.Logger);
			On_SayChatCommand.ProcessIncomingMessage += OnChat;
		}

		public override void Unload()
		{
			webhook?.Dispose();
			webhook = null;
		}

		public override void PostSetupContent()
		{
			PresenceTracker.Instance.PlayerJoined += player => PostIf(Config.PostJoinLeave, JoinColor, "Joined", player.name);
			PresenceTracker.Instance.PlayerLeft += player => PostIf(Config.PostJoinLeave, LeaveColor, "Left", player.name);
		}

		// re-read every world load, so a new url only needs a server restart, not a mod rebuild
		public override void OnWorldLoad()
		{
			if (Main.netMode == NetmodeID.Server)
				webhook.LoadUrl();
		}

		// public chat only, party chat and commands go through other paths
		private void OnChat(On_SayChatCommand.orig_ProcessIncomingMessage orig, SayChatCommand self, string text, byte clientId)
		{
			orig(self, text, clientId);
			if (Config.PostChat && !text.StartsWith('/') && clientId < Main.maxPlayers)
				PostIf(true, ChatColor, "Chat", Main.player[clientId].name, text);
		}

		public void OnDeath(Player player, PlayerDeathReason reason)
		{
			if (Config.PostDeaths)
				Post(Language.GetTextValue(Key + "Death", DiscordWebhook.Escape(reason.GetDeathText(player.name).ToString())), DeathColor);
		}

		public void OnBossKilled(NPC npc)
		{
			if (!Active || !Config.PostFirstBossKills)
				return;

			fightBosses.Add(npc.TypeName);
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (npc.playerInteraction[i] && Main.player[i].active)
					fightHelpers.Add(Main.player[i].name);
			}

			// the fight is over once no other boss npc is alive
			// ponytail: an unrelated boss alive at the same time delays the post until it dies too
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC other = Main.npc[i];
				if (other.active && other.boss && i != npc.whoAmI && other.realLife != npc.whoAmI && (npc.realLife < 0 || other.realLife != npc.realLife))
					return;
			}

			string bosses = string.Join(" & ", fightBosses);
			if (defeated.Add(bosses))
			{
				string helpers = string.Join(", ", fightHelpers.Select(DiscordWebhook.Escape));
				Post(Language.GetTextValue(Key + "FirstKill", DiscordWebhook.Escape(bosses), helpers), BossColor);
			}
			fightBosses.Clear();
			fightHelpers.Clear();
		}

		private void PostIf(bool setting, int color, string key, params object[] args)
		{
			if (setting)
				Post(Language.GetTextValue(Key + key, args.Select(a => DiscordWebhook.Escape(a.ToString())).ToArray()), color);
		}

		private void Post(string text, int color)
		{
			if (Active)
				webhook.Post(text, color);
		}

		// saved even while the module is off, so turning it back on loses nothing
		public override void SaveWorldData(TagCompound tag)
		{
			tag["discord"] = new TagCompound
			{
				["version"] = SaveVersion,
				["defeated"] = defeated.ToList(),
			};
		}

		public override void LoadWorldData(TagCompound tag)
		{
			defeated.Clear();
			if (!tag.TryGet("discord", out TagCompound saved))
				return;

			// version 1 is the only shape so far, migrate here when it changes
			defeated.UnionWith(saved.GetList<string>("defeated"));
		}

		public override void ClearWorld()
		{
			defeated.Clear();
			fightBosses.Clear();
			fightHelpers.Clear();
		}
	}

	// Kill also runs on the server when a client reports its death
	public class DiscordPlayer : ModPlayer
	{
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (Main.netMode == NetmodeID.Server && DiscordModule.IsEnabled)
				DiscordSystem.Instance.OnDeath(Player, damageSource);
		}
	}

	public class DiscordGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.boss)
				DiscordSystem.Instance.OnBossKilled(npc);
		}
	}
}
