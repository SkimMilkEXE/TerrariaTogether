using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;

namespace TerrariaTogether.Modules.Rules
{
	// welcome message and server rules on join, optionally accepted before building
	public class RulesModule : SmpModule
	{
		public const string ModuleId = "rules";
		private const string Key = "Mods.TerrariaTogether.Rules.";

		public override string Id => ModuleId;

		// for globals, so they don't look the module up every call
		public static bool IsEnabled => ModuleManager.IsEnabled(ModuleId);

		public override void HandlePacket(byte msgType, BinaryReader reader, int fromWho)
		{
			RulesSystem.Instance.HandlePacket(msgType, reader, fromWho);
		}

		// /smp rules [accept]
		public override void HandleCommand(CommandCaller caller, string[] args)
		{
			string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "";
			if (sub == "accept")
			{
				if (caller.CommandType == CommandType.Console)
				{
					caller.Reply(Language.GetTextValue(Key + "ConsoleAccept"), Color.OrangeRed);
					return;
				}
				RulesSystem.Instance.Accept(caller.Player);
				caller.Reply(Language.GetTextValue(Key + "Accepted"), Color.LightGreen);
				return;
			}
			if (sub != "")
			{
				caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
				return;
			}

			List<string> lines = RulesSystem.Instance.RuleLines();
			if (lines.Count == 0)
			{
				caller.Reply(Language.GetTextValue(Key + "NoRules"));
				return;
			}
			foreach (string line in lines)
				caller.Reply(line);
			if (caller.CommandType != CommandType.Console && !RulesSystem.Instance.HasAccepted(caller.Player))
				caller.Reply(Language.GetTextValue(Key + "AcceptHint"), Color.Gold);
		}
	}
}
