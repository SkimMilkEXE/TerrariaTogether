using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using TerrariaTogether.Core.Config;
using TerrariaTogether.Modules.Gating;
using TerrariaTogether.Modules.Stats;

namespace TerrariaTogether.Core.Modules
{
	// holds every smp module, looked up by id
	public class ModuleManager : ModSystem
	{
		private readonly List<SmpModule> modules = new();

		public IReadOnlyList<SmpModule> Modules => modules;

		public static ModuleManager Instance => ModContent.GetInstance<ModuleManager>();

		public override void Load()
		{
			modules.Add(new GatingModule());
			modules.Add(new StatsModule());
		}

		public override void Unload()
		{
			modules.Clear();
		}

		// modules missing from the table count as on
		public static bool IsEnabled(string id)
		{
			return ModContent.GetInstance<CoreServerConfig>().Modules.GetValueOrDefault(id, true);
		}

		public SmpModule Get(string id)
		{
			return modules.Find(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
		}
	}
}
