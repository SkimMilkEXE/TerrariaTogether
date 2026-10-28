using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terraria.ModLoader;
using TerrariaTogether.Core.Config;

namespace TerrariaTogether.Modules.Rules
{
	public class RulesConfig : SmpServerConfig
	{
		[DefaultValue(true)]
		public bool ShowWelcome = true;

		// {name} and {world} are filled in when it's sent
		[DefaultValue("Welcome to {world}, {name}!")]
		public string WelcomeMessage = "Welcome to {world}, {name}!";

		[DefaultValue(true)]
		public bool ShowRules = true;

		// one rule per entry, numbered when shown
		public List<string> Rules = new()
		{
			"Be kind to each other.",
			"No griefing or stealing.",
			"Ask before building close to someone else.",
		};

		// players can't build until they type /smp rules accept
		[DefaultValue(false)]
		public bool RequireAccept = false;

		// methods, not properties, so they don't show up in the config ui
		public List<string> ActiveRules()
		{
			return ShowRules ? Rules.Where(r => !string.IsNullOrWhiteSpace(r)).ToList() : new List<string>();
		}

		public bool MustAccept() => RequireAccept && ActiveRules().Count > 0;

		public override void OnChanged()
		{
			ModContent.GetInstance<RulesSystem>()?.OnConfigChanged();
		}
	}
}
