using System.Collections.Generic;
using Terraria.ModLoader;

namespace TerrariaTogether.Modules.Gating.Registry
{
	// every gateable boss, looked up by key, spawn npc or summon item
	public class BossRegistry : ModSystem
	{
		private readonly Dictionary<string, BossEntry> byKey = new();
		private readonly Dictionary<int, BossEntry> bySpawnNpc = new();
		private readonly Dictionary<int, List<BossEntry>> bySummonItem = new();

		public static BossRegistry Instance => ModContent.GetInstance<BossRegistry>();

		public IEnumerable<BossEntry> All => byKey.Values;

		public override void Load()
		{
			VanillaBosses.Register(this);
		}

		public override void Unload()
		{
			byKey.Clear();
			bySpawnNpc.Clear();
			bySummonItem.Clear();
		}

		public void Register(BossEntry entry)
		{
			if (byKey.ContainsKey(entry.Key))
			{
				Mod.Logger.Warn($"boss '{entry.Key}' is already registered, ignoring");
				return;
			}

			byKey[entry.Key] = entry;
			foreach (int npc in entry.SpawnNpcs)
				bySpawnNpc[npc] = entry;

			// one item can summon several bosses (mechdusa summons all three mechs)
			foreach (int item in entry.SummonItems)
			{
				if (!bySummonItem.TryGetValue(item, out List<BossEntry> list))
					bySummonItem[item] = list = new List<BossEntry>();
				list.Add(entry);
			}
		}

		public BossEntry Get(string key) => byKey.GetValueOrDefault(key);

		public BossEntry BySpawnNpc(int npcType) => bySpawnNpc.GetValueOrDefault(npcType);

		public List<BossEntry> BySummonItem(int itemType) => bySummonItem.GetValueOrDefault(itemType);
	}
}
