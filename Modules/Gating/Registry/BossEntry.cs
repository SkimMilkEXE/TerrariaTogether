using System;
using Terraria.Localization;

namespace TerrariaTogether.Modules.Gating.Registry
{
	public class BossEntry
	{
		// stable "ModName/BossName" key, used in saves, commands and the api
		public string Key;

		public LocalizedText DisplayName;

		// npcs that start the fight, these are the ones vetoed
		public int[] SpawnNpcs;

		// every other npc belonging to the fight (segments, hands, eyes...)
		public int[] PartNpcs;

		public int[] SummonItems;

		public Func<bool> IsDowned;
	}
}
