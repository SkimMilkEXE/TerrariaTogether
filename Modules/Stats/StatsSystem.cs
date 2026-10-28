using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaTogether.Core.Roster;

namespace TerrariaTogether.Modules.Stats
{
	public class PlayerStats
	{
		public string Uuid;
		public string Name;
		public long PlaytimeSeconds;
		public int Deaths;
		public int BossKills;
		public int PvpKills;
	}

	// stats per client uuid, counted on the server (or locally in single player)
	public class StatsSystem : ModSystem
	{
		private const int SaveVersion = 1;

		private readonly Dictionary<string, PlayerStats> stats = new();
		private int ticksSinceSecond;

		public static StatsSystem Instance => ModContent.GetInstance<StatsSystem>();

		public IEnumerable<PlayerStats> All => stats.Values;

		// null until the player's uuid has arrived, creates the record on first use
		public PlayerStats Get(Player player)
		{
			string uuid = PresenceTracker.Instance.GetUuid(player);
			if (uuid == null)
				return null;

			if (!stats.TryGetValue(uuid, out PlayerStats playerStats))
			{
				playerStats = new PlayerStats { Uuid = uuid };
				stats[uuid] = playerStats;
			}
			// picks up character renames
			playerStats.Name = player.name;
			return playerStats;
		}

		public List<PlayerStats> FindByName(string name)
		{
			return stats.Values.Where(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
		}

		// adds a second of playtime to everyone in the world once per second
		public override void PostUpdateEverything()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || !StatsModule.IsEnabled)
				return;
			if (++ticksSinceSecond < 60)
				return;
			ticksSinceSecond = 0;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (Main.player[i].active && Get(Main.player[i]) is PlayerStats playerStats)
					playerStats.PlaytimeSeconds++;
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["stats"] = new TagCompound
			{
				["version"] = SaveVersion,
				["players"] = stats.Values.Select(s => new TagCompound
				{
					["uuid"] = s.Uuid,
					["name"] = s.Name,
					["playtime"] = s.PlaytimeSeconds,
					["deaths"] = s.Deaths,
					["bossKills"] = s.BossKills,
					["pvpKills"] = s.PvpKills,
				}).ToList(),
			};
		}

		public override void LoadWorldData(TagCompound tag)
		{
			stats.Clear();
			if (!tag.TryGet("stats", out TagCompound saved))
				return;

			// version 1 is the only shape so far, migrate here when it changes
			foreach (TagCompound t in saved.GetList<TagCompound>("players"))
			{
				var playerStats = new PlayerStats
				{
					Uuid = t.GetString("uuid"),
					Name = t.GetString("name"),
					PlaytimeSeconds = t.GetLong("playtime"),
					Deaths = t.GetInt("deaths"),
					BossKills = t.GetInt("bossKills"),
					PvpKills = t.GetInt("pvpKills"),
				};
				stats[playerStats.Uuid] = playerStats;
			}
		}

		public override void ClearWorld()
		{
			stats.Clear();
			ticksSinceSecond = 0;
		}
	}
}
