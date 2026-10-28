using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using TerrariaTogether.Core.Roster;

namespace TerrariaTogether.Core.Config
{
	public class CoreServerConfig : SmpServerConfig
	{
		// module id -> on/off, only modules switched at least once are stored, missing ids count as on
		[CustomModConfigItem(typeof(ModuleToggleTableElement))]
		public Dictionary<string, bool> Modules = new();

		// player names or player ids, players can see theirs with /smp whoami
		public List<string> Admins = new();

		[DefaultValue(RosterMode.Auto)]
		public RosterMode RosterMode = RosterMode.Auto;

		// 0 means members never go inactive
		[Range(0, 365)]
		[DefaultValue(14)]
		public int InactiveDays = 14;
	}
}
