using TerrariaTogether.Core.Modules;

namespace TerrariaTogether.Modules.Discord
{
	// posts joins, leaves, chat, deaths and first boss kills to a discord channel through a webhook
	public class DiscordModule : SmpModule
	{
		public const string ModuleId = "discord";

		public override string Id => ModuleId;

		// for globals, so they don't look the module up every call
		public static bool IsEnabled => ModuleManager.IsEnabled(ModuleId);
	}
}
