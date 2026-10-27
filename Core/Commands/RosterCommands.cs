using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Permissions;
using TerrariaTogether.Core.Roster;

namespace TerrariaTogether.Core.Commands
{
	// /smp roster list|add|remove|exclude <name>
	public static class RosterCommands
	{
		private const string Key = "Mods.TerrariaTogether.Commands.Roster.";

		public static void Handle(CommandCaller caller, string[] args)
		{
			string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "list";
			if (sub == "list")
			{
				List(caller);
				return;
			}

			if (sub != "add" && sub != "remove" && sub != "exclude")
			{
				caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
				return;
			}
			if (!SmpPermissions.RequireAdmin(caller))
				return;

			// names can have spaces
			string name = string.Join(" ", args[1..]);
			if (name.Length == 0)
			{
				caller.Reply(Language.GetTextValue(Key + "Usage"), Color.OrangeRed);
				return;
			}

			if (sub == "add")
			{
				Add(caller, name);
				return;
			}

			RosterMember member = FindOne(caller, name);
			if (member == null)
				return;

			if (sub == "remove")
			{
				RosterSystem.Instance.Remove(member);
				caller.Reply(Language.GetTextValue(Key + "Removed", member.Name));
			}
			else
			{
				member.Excluded = !member.Excluded;
				caller.Reply(Language.GetTextValue(Key + (member.Excluded ? "Excluded" : "Included"), member.Name));
			}
		}

		private static void List(CommandCaller caller)
		{
			RosterSystem roster = RosterSystem.Instance;
			if (roster.Members.Count == 0)
			{
				caller.Reply(Language.GetTextValue(Key + "Empty"));
				return;
			}

			caller.Reply(Language.GetTextValue(Key + "Header", roster.OnlineCount, roster.TotalCount));
			foreach (RosterMember member in roster.Members)
			{
				int days = (int)(DateTime.UtcNow - member.LastSeenUtc).TotalDays;
				if (member.Excluded)
					caller.Reply(Language.GetTextValue(Key + "LineExcluded", member.Name), Color.Gray);
				else if (roster.IsOnline(member))
					caller.Reply(Language.GetTextValue(Key + "LineOnline", member.Name), Color.LightGreen);
				else if (roster.IsInactive(member))
					caller.Reply(Language.GetTextValue(Key + "LineInactive", member.Name, days), Color.Gray);
				else
					caller.Reply(Language.GetTextValue(Key + "LineOffline", member.Name, days));
			}
		}

		// adding needs the player online, since the server only learns uuids when someone joins
		private static void Add(CommandCaller caller, string name)
		{
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (!player.active || !string.Equals(player.name, name, StringComparison.OrdinalIgnoreCase))
					continue;

				string uuid = PresenceTracker.Instance.GetUuid(player);
				if (uuid == null)
				{
					caller.Reply(Language.GetTextValue(Key + "NoUuid", player.name), Color.OrangeRed);
					return;
				}
				if (RosterSystem.Instance.Find(uuid) != null)
				{
					caller.Reply(Language.GetTextValue(Key + "AlreadyMember", player.name));
					return;
				}

				RosterSystem.Instance.Add(uuid, player.name);
				caller.Reply(Language.GetTextValue(Key + "Added", player.name));
				return;
			}
			caller.Reply(Language.GetTextValue(Key + "NotOnline", name), Color.OrangeRed);
		}

		// names aren't unique, so refuse to guess when several members match
		private static RosterMember FindOne(CommandCaller caller, string name)
		{
			List<RosterMember> matches = RosterSystem.Instance.FindByName(name);
			if (matches.Count == 1)
				return matches[0];

			string key = matches.Count == 0 ? "NotFound" : "Ambiguous";
			caller.Reply(Language.GetTextValue(Key + key, name), Color.OrangeRed);
			return null;
		}
	}
}
