using System;
using System.Collections.Generic;
using Terraria.ModLoader;

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
			// register modules here, e.g. modules.Add(new GatingModule());
		}

		public override void Unload()
		{
			modules.Clear();
		}

		public SmpModule Get(string id)
		{
			return modules.Find(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
		}
	}
}
