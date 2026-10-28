using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Config;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating
{
	// one checkbox per registered boss in the Gated bosses table, grouped under a heading per mod
	public class BossToggleTableElement : ToggleTableElement
	{
		protected override IEnumerable<(string Key, string Label)> Rows()
		{
			// GroupBy keeps first-seen order, so vanilla comes first and bosses stay in progression order
			// each mod's events get their own group after its bosses
			var groups = BossRegistry.Instance.All
				.GroupBy(b => (Mod: b.Key[..b.Key.IndexOf('/')], b.IsEvent))
				.OrderBy(g => g.Key.IsEvent ? 1 : 0)
				.ThenBy(g => g.Key.Mod == "Terraria" ? 0 : 1);
			foreach (var group in groups)
			{
				string mod = ModDisplayName(group.Key.Mod);
				yield return (null, group.Key.IsEvent ? Language.GetTextValue("Mods.TerrariaTogether.Gating.EventsHeading", mod) : mod);
				foreach (BossEntry boss in group)
					yield return (boss.Key, boss.DisplayName.Value);
			}
		}

		private static string ModDisplayName(string modName)
		{
			return ModLoader.TryGetMod(modName, out Mod mod) ? mod.DisplayName : modName;
		}
	}
}
