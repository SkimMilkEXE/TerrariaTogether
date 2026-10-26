using System;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;
using TerrariaTogether.Core.Permissions;

namespace TerrariaTogether.Core.Commands
{
	// /smp <subcommand or module> <args...>, runs on the server in multiplayer
	public class SmpCommand : ModCommand
	{
		private const string Key = "Mods.TerrariaTogether.Commands.";

		public override string Command => "smp";
		public override CommandType Type => CommandType.World | CommandType.Console;
		public override string Usage => "/smp help";
		public override string Description => Language.GetTextValue(Key + "Description");

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "help";

			switch (sub)
			{
				case "help":
					caller.Reply(Language.GetTextValue(Key + "Help"));
					return;
				case "modules":
					ListModules(caller);
					return;
				case "whoami":
					WhoAmI(caller);
					return;
			}

			// anything else is routed to the module with that id
			SmpModule module = ModuleManager.Instance.Get(sub);
			if (module == null)
			{
				caller.Reply(Language.GetTextValue(Key + "Unknown", sub), Color.OrangeRed);
				return;
			}
			if (!module.Enabled)
			{
				caller.Reply(Language.GetTextValue(Key + "ModuleDisabled", module.Id), Color.OrangeRed);
				return;
			}
			module.HandleCommand(caller, args[1..]);
		}

		private static void ListModules(CommandCaller caller)
		{
			var modules = ModuleManager.Instance.Modules;
			if (modules.Count == 0)
			{
				caller.Reply(Language.GetTextValue(Key + "NoModules"));
				return;
			}

			caller.Reply(Language.GetTextValue(Key + "ModulesHeader"));
			foreach (SmpModule module in modules)
			{
				string state = Language.GetTextValue(Key + (module.Enabled ? "On" : "Off"));
				caller.Reply(Language.GetTextValue(Key + "ModuleLine", module.Id, state), module.Enabled ? Color.LightGreen : Color.Gray);
			}
		}

		// shows the caller how the server sees them, handy when setting up the admin list
		private static void WhoAmI(CommandCaller caller)
		{
			if (caller.CommandType == CommandType.Console)
			{
				caller.Reply(Language.GetTextValue(Key + "WhoAmI.Console"));
				return;
			}

			string key = SmpPermissions.IsAdmin(caller) ? "WhoAmI.Admin" : "WhoAmI.Player";
			caller.Reply(Language.GetTextValue(Key + key, caller.Player.name));
		}
	}
}
