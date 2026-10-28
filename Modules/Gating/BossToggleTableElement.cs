using System.Collections.Generic;
using System.Linq;
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
			foreach (var group in BossRegistry.Instance.All.GroupBy(b => b.Key[..b.Key.IndexOf('/')]))
			{
				yield return (null, ModDisplayName(group.Key));
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
