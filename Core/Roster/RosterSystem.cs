using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaTogether.Core.Config;
using TerrariaTogether.Core.Permissions;

namespace TerrariaTogether.Core.Roster
{
	// smp members for this world, saved with the world
	public class RosterSystem : ModSystem
	{
		private const int SaveVersion = 1;

		private readonly List<RosterMember> members = new();

		public IReadOnlyList<RosterMember> Members => members;

		// uuid of the first player to join this world, added to the admins list once
		public string OwnerUuid { get; private set; }

		// the owner joins before their client can take config updates, so they're made admin once they're in the world
		private string pendingAdminUuid;

		public override void PostUpdateEverything()
		{
			if (pendingAdminUuid == null || Main.netMode != NetmodeID.Server)
				return;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (player.active && PresenceTracker.Instance.GetUuid(player) == pendingAdminUuid)
				{
					SmpPermissions.AddAdmin(pendingAdminUuid, player.name);
					pendingAdminUuid = null;
					return;
				}
			}
		}

		public static RosterSystem Instance => ModContent.GetInstance<RosterSystem>();

		private static CoreServerConfig Config => ModContent.GetInstance<CoreServerConfig>();

		public RosterMember Find(string uuid) => members.Find(m => m.Uuid == uuid);

		public List<RosterMember> FindByName(string name)
		{
			return members.FindAll(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
		}

		public RosterMember Add(string uuid, string name)
		{
			var member = new RosterMember { Uuid = uuid, Name = name, LastSeenUtc = DateTime.UtcNow };
			members.Add(member);
			Mod.Logger.Info($"roster: added {name}");
			return member;
		}

		public void Remove(RosterMember member)
		{
			members.Remove(member);
			Mod.Logger.Info($"roster: removed {member.Name}");
		}

		// called on the server once a joining player's uuid is known
		public void OnJoin(string uuid, string name)
		{
			if (OwnerUuid == null)
			{
				OwnerUuid = uuid;
				Mod.Logger.Info($"roster: {name} is the world owner");
				pendingAdminUuid = uuid;
			}

			RosterMember member = Find(uuid);
			if (member == null)
			{
				if (Config.RosterMode != RosterMode.Auto)
					return;
				member = Add(uuid, name);
			}

			// picks up character renames
			member.Name = name;
			member.LastSeenUtc = DateTime.UtcNow;
		}

		public void OnLeave(string uuid, string name)
		{
			RosterMember member = Find(uuid);
			if (member == null)
				return;

			member.Name = name;
			member.LastSeenUtc = DateTime.UtcNow;
		}

		public bool IsOnline(RosterMember member) => PresenceTracker.Instance.IsOnline(member.Uuid);

		// online members always count, offline ones drop off after the inactive days setting
		public bool IsInactive(RosterMember member)
		{
			int days = Config.InactiveDays;
			return days > 0 && !IsOnline(member) && DateTime.UtcNow - member.LastSeenUtc > TimeSpan.FromDays(days);
		}

		public int OnlineCount => members.Count(m => !m.Excluded && IsOnline(m));

		public int TotalCount => members.Count(m => !m.Excluded && !IsInactive(m));

		public override void SaveWorldData(TagCompound tag)
		{
			// anyone still online was seen just now
			foreach (RosterMember member in members)
			{
				if (IsOnline(member))
					member.LastSeenUtc = DateTime.UtcNow;
			}

			tag["roster"] = new TagCompound
			{
				["version"] = SaveVersion,
				["owner"] = OwnerUuid ?? "",
				["members"] = members.Select(m => new TagCompound
				{
					["uuid"] = m.Uuid,
					["name"] = m.Name,
					["lastSeen"] = m.LastSeenUtc.Ticks,
					["excluded"] = m.Excluded,
				}).ToList(),
			};
		}

		public override void LoadWorldData(TagCompound tag)
		{
			members.Clear();
			OwnerUuid = null;
			if (!tag.TryGet("roster", out TagCompound roster))
				return;

			// version 1 is the only shape so far, migrate here when it changes
			// owner was added later without a version bump, older saves just don't have it
			string owner = roster.GetString("owner");
			OwnerUuid = string.IsNullOrEmpty(owner) ? null : owner;
			foreach (TagCompound t in roster.GetList<TagCompound>("members"))
			{
				members.Add(new RosterMember
				{
					Uuid = t.GetString("uuid"),
					Name = t.GetString("name"),
					LastSeenUtc = new DateTime(t.GetLong("lastSeen"), DateTimeKind.Utc),
					Excluded = t.GetBool("excluded"),
				});
			}
		}

		public override void ClearWorld()
		{
			members.Clear();
			OwnerUuid = null;
			pendingAdminUuid = null;
		}
	}
}
