using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using TerrariaTogether.Core.Config;
using TerrariaTogether.Core.Roster;

namespace TerrariaTogether.Core.Permissions
{
	// named SmpPermissions so it doesn't clash with the namespace
	public static class SmpPermissions
	{
		// the server console is always admin
		public static bool IsAdmin(CommandCaller caller)
		{
			return caller.CommandType == CommandType.Console || IsAdmin(caller.Player);
		}

		// only meaningful on the server or in single player, clients don't know other players' uuids
		public static bool IsAdmin(Player player)
		{
			if (player == null)
				return false;

			// single player owns their own world
			if (Main.netMode == NetmodeID.SinglePlayer)
				return true;

			string uuid = PresenceTracker.Instance.GetUuid(player);
			foreach (string entry in ModContent.GetInstance<CoreServerConfig>().Admins)
			{
				string trimmed = entry?.Trim();
				if (string.IsNullOrEmpty(trimmed))
					continue;
				if (trimmed == player.name)
					return true;
				// accepts the full uuid or the short id from /smp whoami
				if (uuid != null && trimmed.Length >= PresenceTracker.ShortIdLength && uuid.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

		// server only: adds the player's short id to the admins list in Server Settings and saves it
		public static void AddAdmin(string uuid, string name)
		{
			var config = ModContent.GetInstance<CoreServerConfig>();
			string id = PresenceTracker.ShortId(uuid);
			if (config.Admins.Exists(e => e != null && (e.Trim().Equals(id, StringComparison.OrdinalIgnoreCase) || e.Trim().Equals(uuid, StringComparison.OrdinalIgnoreCase))))
				return;

			// changes go through a copy, tmodloader applies and syncs it on save
			var pending = (CoreServerConfig)ConfigManager.GeneratePopulatedClone(config);
			pending.Admins.Add(id);
			config.SaveChanges(pending, broadcast: false);
			ModContent.GetInstance<TerrariaTogether>().Logger.Info($"permissions: added {name} ({id}) to admins");
		}

		// replies with a refusal and returns false when the caller isn't an admin
		public static bool RequireAdmin(CommandCaller caller)
		{
			if (IsAdmin(caller))
				return true;

			caller.Reply(Language.GetTextValue("Mods.TerrariaTogether.Commands.NotAdmin"), Color.OrangeRed);
			return false;
		}
	}
}
