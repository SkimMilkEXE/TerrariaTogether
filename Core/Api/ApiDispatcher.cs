using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;

namespace TerrariaTogether.Core.Api
{
	// routes Mod.Call, see docs/API.md for the public contract
	public static class ApiDispatcher
	{
		// bump when a call is renamed or removed, not when one is added
		public const int ApiVersion = 1;

		public static object Call(Mod mod, object[] args)
		{
			if (args == null || args.Length == 0 || args[0] is not string name)
			{
				mod.Logger.Warn("Mod.Call needs a call name as the first argument");
				return null;
			}

			switch (name)
			{
				case "GetApiVersion":
					return ApiVersion;
				case "IsModuleEnabled":
					return args.Length > 1 && args[1] is string id && ModuleManager.Instance.Get(id)?.Enabled == true;
			}

			// "Module.Call" names go to that module, even when it's disabled so registrations aren't lost
			int dot = name.IndexOf('.');
			if (dot > 0)
			{
				SmpModule module = ModuleManager.Instance.Get(name[..dot]);
				if (module != null)
					return module.Call(name[(dot + 1)..], args[1..]);
			}

			mod.Logger.Warn($"unknown Mod.Call '{name}'");
			return null;
		}
	}
}
