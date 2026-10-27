using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using TerrariaTogether.Core.Permissions;
using TerrariaTogether.Core.Roster;

namespace TerrariaTogether.Core.Config
{
	public class CoreServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// module on/off toggles go here as each module is added

		// player names or client uuids, players can see theirs with /smp whoami
		public List<string> Admins = new();

		[DefaultValue(RosterMode.Auto)]
		public RosterMode RosterMode = RosterMode.Auto;

		// 0 means members never go inactive
		[Range(0, 365)]
		[DefaultValue(14)]
		public int InactiveDays = 14;

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
