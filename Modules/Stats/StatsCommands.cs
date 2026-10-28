using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaTogether.Modules.Stats
{
	// /smp stats [player] and /smp stats top <stat>, open to everyone
	public static class StatsCommands
	{
		private const string Key = "Mods.TerrariaTogether.Stats.Commands.";
		private const int TopCount = 5;

		public static void Handle(CommandCaller caller, string[] args)
		{
			if (args.Length > 0 && args[0].Equals("top", StringComparison.OrdinalIgnoreCase))
			{
				Top(caller, args.Length > 1 ? args[1].ToLowerInvariant() : "playtime");
				return;
			}

			PlayerStats stats;
			if (args.Length == 0)
			{
				// the console has no stats of its own
				if (caller.CommandType == CommandType.Console)
				{
					caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
					return;
				}
				stats = StatsSystem.Instance.Get(caller.Player);
				if (stats == null)
				{
					caller.Reply(Language.GetTextValue(Key + "NoUuid"), Color.OrangeRed);
					return;
				}
			}
			else
			{
				stats = FindOne(caller, string.Join(" ", args));
				if (stats == null)
					return;
			}

			caller.Reply(Language.GetTextValue(Key + "Header", stats.Name));
			caller.Reply(Language.GetTextValue(Key + "Line", FormatPlaytime(stats.PlaytimeSeconds), stats.Deaths, stats.BossKills, stats.PvpKills));
		}

		private static void Top(CommandCaller caller, string board)
		{
			Func<PlayerStats, long> value = board switch
			{
				"playtime" => s => s.PlaytimeSeconds,
				"deaths" => s => s.Deaths,
				"bosses" => s => s.BossKills,
				"pvp" => s => s.PvpKills,
				_ => null,
			};
			if (value == null)
			{
				caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
				return;
			}

			List<PlayerStats> top = StatsSystem.Instance.All.Where(s => value(s) > 0).OrderByDescending(value).Take(TopCount).ToList();
			caller.Reply(Language.GetTextValue(Key + "TopHeader", Language.GetTextValue(Key + "Boards." + board)));
			if (top.Count == 0)
			{
				caller.Reply(Language.GetTextValue(Key + "TopEmpty"));
				return;
			}
			for (int i = 0; i < top.Count; i++)
			{
				string shown = board == "playtime" ? FormatPlaytime(top[i].PlaytimeSeconds) : value(top[i]).ToString();
				caller.Reply(Language.GetTextValue(Key + "TopLine", i + 1, top[i].Name, shown));
			}
		}

		private static string FormatPlaytime(long seconds)
		{
			TimeSpan time = TimeSpan.FromSeconds(seconds);
			return Language.GetTextValue("Mods.TerrariaTogether.Stats.Playtime", (int)time.TotalHours, time.Minutes);
		}

		// names aren't unique, so refuse to guess when several players match
		private static PlayerStats FindOne(CommandCaller caller, string name)
		{
			List<PlayerStats> matches = StatsSystem.Instance.FindByName(name);
			if (matches.Count == 1)
				return matches[0];

			string key = matches.Count == 0 ? "NotFound" : "Ambiguous";
			caller.Reply(Language.GetTextValue(Key + key, name), Color.OrangeRed);
			return null;
		}
	}
}
