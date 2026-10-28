using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;

namespace TerrariaTogether.Modules.Stats
{
	// playtime, deaths and kills per player, saved with the world
	public class StatsModule : SmpModule
	{
		public const string ModuleId = "stats";

		public override string Id => ModuleId;

		// for globals, so they don't look the module up every call
		public static bool IsEnabled => ModuleManager.IsEnabled(ModuleId);

		public override void HandleCommand(CommandCaller caller, string[] args)
		{
			StatsCommands.Handle(caller, args);
		}
	}
}
