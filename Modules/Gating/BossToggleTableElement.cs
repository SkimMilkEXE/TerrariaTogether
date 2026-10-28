using System.Collections.Generic;
using System.Linq;
using TerrariaTogether.Core.Config;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating
{
	// one checkbox per registered boss in the Gated bosses table, modded bosses included
	public class BossToggleTableElement : ToggleTableElement
	{
		protected override IEnumerable<(string Key, string Label)> Rows()
		{
			return BossRegistry.Instance.All.Select(b => (b.Key, b.DisplayName.Value));
		}
	}
}
