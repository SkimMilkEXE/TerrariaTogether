using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using TerrariaTogether.Core.Permissions;

namespace TerrariaTogether.Core.Config
{
	public class CoreServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// module on/off toggles go here as each module is added

		// player names, uuids come in phase 2
		public List<string> Admins = new();

		// only admins can change server settings from in game
		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
		{
			if (SmpPermissions.IsAdmin(Main.player[whoAmI]))
				return true;

			message = NetworkText.FromKey("Mods.TerrariaTogether.Commands.NotAdmin");
			return false;
		}
	}
}
