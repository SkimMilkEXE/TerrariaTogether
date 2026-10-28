using System.ComponentModel;
using TerrariaTogether.Core.Config;

namespace TerrariaTogether.Modules.Discord
{
	// what gets posted, the webhook url itself lives in a file on the server so it's never synced to players
	public class DiscordConfig : SmpServerConfig
	{
		[DefaultValue(true)]
		public bool PostJoinLeave = true;

		[DefaultValue(true)]
		public bool PostChat = true;

		[DefaultValue(true)]
		public bool PostDeaths = true;

		[DefaultValue(true)]
		public bool PostFirstBossKills = true;
	}
}
