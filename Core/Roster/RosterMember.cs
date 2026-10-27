using System;

namespace TerrariaTogether.Core.Roster
{
	public class RosterMember
	{
		// client uuid, the stable key for a member
		public string Uuid;

		// last known character name, only a label since names aren't unique
		public string Name;

		public DateTime LastSeenUtc;

		// excluded members never count toward online or total
		public bool Excluded;
	}

	public enum RosterMode
	{
		// anyone who joins becomes a member
		Auto,
		// only admins add members
		Manual
	}
}
