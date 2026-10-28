using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaTogether.Modules.Gating.Registry
{
	// every gateable boss, looked up by key, npc type or summon item
	public class BossRegistry : ModSystem
	{
		private readonly Dictionary<string, BossEntry> byKey = new();
		private readonly Dictionary<int, BossEntry> byNpc = new();
		private readonly Dictionary<int, List<BossEntry>> bySummonItem = new();

		// unregistered modded bosses caught by the fallback, never listed in All
		private readonly Dictionary<int, BossEntry> unknownByNpc = new();

		public static BossRegistry Instance => ModContent.GetInstance<BossRegistry>();

		public IEnumerable<BossEntry> All => byKey.Values;

		public override void Load()
		{
			VanillaBosses.Register(this);
		}

		public override void Unload()
		{
			byKey.Clear();
			byNpc.Clear();
			bySummonItem.Clear();
			unknownByNpc.Clear();
		}

		// returns false if the key is taken, the first registration wins
		public bool Register(BossEntry entry)
		{
			if (byKey.ContainsKey(entry.Key))
			{
				Mod.Logger.Warn($"boss '{entry.Key}' is already registered, ignoring");
				return false;
			}

			byKey[entry.Key] = entry;
			foreach (int npc in entry.SpawnNpcs)
				byNpc.TryAdd(npc, entry);
			foreach (int npc in entry.PartNpcs)
				byNpc.TryAdd(npc, entry);

			// one item can summon several bosses (mechdusa summons all three mechs)
			foreach (int item in entry.SummonItems)
			{
				if (!bySummonItem.TryGetValue(item, out List<BossEntry> list))
					bySummonItem[item] = list = new List<BossEntry>();
				list.Add(entry);
			}
			return true;
		}

		public BossEntry Get(string key) => byKey.GetValueOrDefault(key);

		public BossEntry ByNpc(int npcType) => byNpc.GetValueOrDefault(npcType);

		public List<BossEntry> BySummonItem(int itemType) => bySummonItem.GetValueOrDefault(itemType);

		// registered boss for this npc, or a fallback entry for unregistered modded bosses when the policy says so
		public BossEntry Resolve(NPC npc)
		{
			BossEntry entry = ByNpc(npc.type);
			if (entry != null)
				return entry;
			if (!IsUnknownBoss(npc) || ModContent.GetInstance<GatingConfig>().UnknownBosses != UnknownBossPolicy.Gate)
				return null;

			if (!unknownByNpc.TryGetValue(npc.type, out entry))
			{
				ModNPC modNpc = npc.ModNPC;
				entry = new BossEntry
				{
					Key = modNpc.Mod.Name + "/" + modNpc.Name,
					DisplayName = modNpc.DisplayName,
					SpawnNpcs = new[] { npc.type },
					PartNpcs = Array.Empty<int>(),
					SummonItems = Array.Empty<int>(),
					IsDowned = () => false, // no kill history for unknown bosses
				};
				unknownByNpc[npc.type] = entry;
				Mod.Logger.Info($"gating: unregistered boss {entry.Key} will use the unknown-boss policy");
			}
			return entry;
		}

		// vanilla event bosses (pumpking etc.) are left for the events work in phase 6
		public static bool IsUnknownBoss(NPC npc) => npc.boss && npc.ModNPC != null;
	}
}
