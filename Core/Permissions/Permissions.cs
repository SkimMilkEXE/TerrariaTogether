using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Config;

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

			// ponytail: names only, phase 2 adds client uuids once the server captures them
			foreach (string entry in ModContent.GetInstance<CoreServerConfig>().Admins)
			{
				if (entry?.Trim() == player.name)
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
