using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using TerrariaTogether.Core.Permissions;

namespace TerrariaTogether.Core.Config
{
	// base for every server config, only admins can change them from in game
	public abstract class SmpServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
		{
			if (SmpPermissions.IsAdmin(Main.player[whoAmI]))
				return true;

			message = NetworkText.FromKey("Mods.TerrariaTogether.Commands.NotAdmin");
			return false;
		}
	}
}
