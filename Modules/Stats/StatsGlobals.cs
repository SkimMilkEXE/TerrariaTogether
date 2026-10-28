using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaTogether.Modules.Stats
{
	// counts deaths and pvp kills, Kill also runs on the server when a client reports its death
	public class StatsPlayer : ModPlayer
	{
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || !StatsModule.IsEnabled)
				return;

			if (StatsSystem.Instance.Get(Player) is PlayerStats victim)
				victim.Deaths++;

			int killer = damageSource.SourcePlayerIndex;
			if (pvp && killer >= 0 && killer < Main.maxPlayers && killer != Player.whoAmI && Main.player[killer].active
				&& StatsSystem.Instance.Get(Main.player[killer]) is PlayerStats killerStats)
				killerStats.PvpKills++;
		}
	}

	// counts a boss kill for everyone who hit the boss, same rule vanilla uses for loot
	public class StatsGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || !StatsModule.IsEnabled || !npc.boss)
				return;

			// bosses made of several npcs (the twins) only count when the last one dies
			// ponytail: also skips a kill while an unrelated boss is alive, rare enough to ignore
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC other = Main.npc[i];
				if (other.active && other.boss && i != npc.whoAmI && !SameBody(npc, other))
					return;
			}

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (npc.playerInteraction[i] && Main.player[i].active && StatsSystem.Instance.Get(Main.player[i]) is PlayerStats playerStats)
					playerStats.BossKills++;
			}
		}

		// segments of worm bosses share one health pool through realLife
		private static bool SameBody(NPC npc, NPC other)
		{
			return other.realLife == npc.whoAmI || (npc.realLife >= 0 && (other.realLife == npc.realLife || other.whoAmI == npc.realLife));
		}
	}
}
