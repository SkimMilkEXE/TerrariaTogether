using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
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
