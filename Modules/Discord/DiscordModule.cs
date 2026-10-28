using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;
using TerrariaTogether.Core.Permissions;

namespace TerrariaTogether.Modules.Discord
{
	// posts joins, leaves, chat, deaths and first boss kills to a discord channel through a webhook
	public class DiscordModule : SmpModule
	{
		public const string ModuleId = "discord";
		private const string Key = "Mods.TerrariaTogether.Discord.Commands.";

		public override string Id => ModuleId;

		// for globals, so they don't look the module up every call
		public static bool IsEnabled => ModuleManager.IsEnabled(ModuleId);

		// /smp discord [status | url <url> | url clear], admins only
		public override void HandleCommand(CommandCaller caller, string[] args)
		{
			if (!SmpPermissions.RequireAdmin(caller))
				return;

			DiscordWebhook webhook = DiscordSystem.Instance.Webhook;
			string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "status";
			if (sub == "status")
			{
				// never shows the url itself
				caller.Reply(Language.GetTextValue(Key + (webhook.HasUrl ? "StatusSet" : "StatusNotSet")));
				return;
			}
			if (sub != "url" || args.Length < 2)
			{
				caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
				return;
			}

			if (args[1].ToLowerInvariant() == "clear")
			{
				webhook.ClearUrl();
				caller.Reply(Language.GetTextValue(Key + "Cleared"));
			}
			else if (webhook.SetUrl(args[1]))
				caller.Reply(Language.GetTextValue(Key + "Set"), Color.LightGreen);
			else
				caller.Reply(Language.GetTextValue(Key + "Invalid"), Color.OrangeRed);
		}
	}
}
