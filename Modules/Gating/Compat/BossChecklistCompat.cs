using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating.Compat
{
	// registers every modded boss boss checklist knows about, so most content mods need zero code
	// weak reference only: does nothing when boss checklist isn't installed
	public class BossChecklistCompat : ModSystem
	{
		private const string ApiVersion = "1.6";

		// boss checklist takes registrations until AddRecipes, so its list is complete here
		public override void PostAddRecipes()
		{
			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
				return;

			// other mods' data, so a bad entry must never stop the game loading
			try
			{
				object response = bossChecklist.Call("GetBossInfoDictionary", Mod, ApiVersion);
				if (response is not Dictionary<string, Dictionary<string, object>> entries)
				{
					Mod.Logger.Warn("gating: boss checklist returned an unexpected boss list, nothing imported");
					return;
				}

				int imported = entries.Count(e => TryImport(e.Key, e.Value));
				Mod.Logger.Info($"gating: imported {imported} bosses from boss checklist");
			}
			catch (Exception e)
			{
				Mod.Logger.Warn("gating: boss checklist import failed", e);
			}
		}

		private bool TryImport(string checklistKey, Dictionary<string, object> info)
		{
			// mini-bosses and events aren't gated yet
			if (!info.TryGetValue("isBoss", out object isBoss) || !Convert.ToBoolean(isBoss))
				return false;

			// vanilla bosses already come from VanillaBosses
			string modSource = info.GetValueOrDefault("modSource") as string;
			if (string.IsNullOrEmpty(modSource) || modSource == "Terraria")
				return false;

			if (info.GetValueOrDefault("npcIDs") is not List<int> npcs || npcs.Count == 0)
				return false;

			// a Mod.Call registration for the same boss wins over the import
			if (npcs.Any(n => BossRegistry.Instance.ByNpc(n) != null))
				return false;

			// checklist keys look like "ModName InternalName", ours like "ModName/InternalName"
			string name = checklistKey.StartsWith(modSource + " ") ? checklistKey[(modSource.Length + 1)..] : checklistKey;

			return BossRegistry.Instance.Register(new BossEntry
			{
				Key = modSource + "/" + name,
				DisplayName = info.GetValueOrDefault("displayName") as LocalizedText ?? Language.GetText(name),
				SpawnNpcs = npcs.ToArray(),
				PartNpcs = Array.Empty<int>(),
				SummonItems = (info.GetValueOrDefault("spawnItems") as List<int>)?.ToArray() ?? Array.Empty<int>(),
				IsDowned = info.GetValueOrDefault("downed") as Func<bool> ?? (() => false),
			});
		}
	}
}
