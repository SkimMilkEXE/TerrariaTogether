using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Permissions;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating
{
	// /smp gating status|lock|unlock|clear <boss>
	public static class GateCommands
	{
		private const string Key = "Mods.TerrariaTogether.Gating.Commands.";

		public static void Handle(CommandCaller caller, string[] args)
		{
			string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "status";
			if (sub == "status")
			{
				Status(caller);
				return;
			}

			GateOverride gateOverride;
			switch (sub)
			{
				case "lock":
					gateOverride = GateOverride.Locked;
					break;
				case "unlock":
					gateOverride = GateOverride.Unlocked;
					break;
				case "clear":
					gateOverride = GateOverride.None;
					break;
				default:
					caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
					return;
			}

			if (!SmpPermissions.RequireAdmin(caller))
				return;

			// boss names can have spaces
			string name = string.Join(" ", args[1..]);
			if (name.Length == 0)
			{
				caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
				return;
			}

			BossEntry boss = FindBoss(caller, name);
			if (boss == null)
				return;

			GatingSystem.Instance.SetOverride(boss, gateOverride);
			string messageKey = "Mods.TerrariaTogether.Gating.Announce.Override." + gateOverride;
			NetworkText message = NetworkText.FromKey(messageKey, NetworkText.FromKey(boss.DisplayName.Key));

			// with announcements on everyone sees it, including the admin
			if (ModContent.GetInstance<GatingConfig>().Announce)
				ChatHelper.BroadcastChatMessage(message, Color.LightGreen);
			if (!ModContent.GetInstance<GatingConfig>().Announce || caller.CommandType == CommandType.Console)
				caller.Reply(message.ToString());
		}

		private static void Status(CommandCaller caller)
		{
			GatingConfig config = ModContent.GetInstance<GatingConfig>();
			LockResult threshold = GatingSystem.Instance.EvaluateThreshold();
			string firstKill = Language.GetTextValue("Mods.TerrariaTogether.Commands." + (config.FirstKillOnly ? "On" : "Off"));
			caller.Reply(Language.GetTextValue(Key + "StatusHeader", threshold.Online, threshold.Total, threshold.Required, firstKill));

			foreach (BossEntry boss in BossRegistry.Instance.All)
			{
				LockResult result = GatingSystem.Instance.Evaluate(boss);
				string line = result.Reason == LockReason.Threshold
					? (result.Locked ? "StatusLocked" : "StatusUnlocked")
					: "Status" + result.Reason;
				caller.Reply(Language.GetTextValue(Key + line, boss.DisplayName.Value), result.Locked ? Color.OrangeRed : Color.LightGreen);
			}
		}

		// accepts the full key, the part after the slash, or the display name, ignoring case and spaces
		private static BossEntry FindBoss(CommandCaller caller, string name)
		{
			string wanted = Normalize(name);
			List<BossEntry> matches = BossRegistry.Instance.All.Where(b =>
				Normalize(b.Key) == wanted ||
				Normalize(b.Key[(b.Key.IndexOf('/') + 1)..]) == wanted ||
				Normalize(b.DisplayName.Value) == wanted).ToList();

			if (matches.Count == 1)
				return matches[0];

			string key = matches.Count == 0 ? "BossNotFound" : "BossAmbiguous";
			caller.Reply(Language.GetTextValue(Key + key, name), Color.OrangeRed);
			return null;
		}

		private static string Normalize(string s) => s.Replace(" ", "").ToLowerInvariant();
	}
}
