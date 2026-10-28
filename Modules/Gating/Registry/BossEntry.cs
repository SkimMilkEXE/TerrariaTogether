using System;
using Terraria.Localization;

namespace TerrariaTogether.Modules.Gating.Registry
{
	public class BossEntry
	{
		// stable "ModName/BossName" key, used in saves, commands and the api
		public string Key;

		public LocalizedText DisplayName;

		// npcs that start the fight
		public int[] SpawnNpcs;

		// every other npc belonging to the fight (segments, hands, eyes...)
		// any of these is vetoed while locked unless a fight with this boss is already running
		public int[] PartNpcs;

		public int[] SummonItems;

		// items that summon the boss by falling into lava (guide voodoo doll)
		public int[] LavaSummonItems = Array.Empty<int>();

		// given back when the catch-all vetoes a player's summon, 0 means the first summon item
		public int RefundItem;

		public Func<bool> IsDowned;

		// events (invasions, moons) have no npcs to veto, they're stopped as they start instead
		public bool IsEvent;
		public Func<bool> IsRunning;
		public Action Stop;

		public int RefundItemType => RefundItem != 0 ? RefundItem : SummonItems.Length > 0 ? SummonItems[0] : 0;
	}
}
