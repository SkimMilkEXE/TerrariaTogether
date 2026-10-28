using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using TerrariaTogether.Core.Config;

namespace TerrariaTogether.Modules.Gating
{
	public enum UnknownBossPolicy
	{
		// shared threshold, like any registered boss
		Gate,
		// spawns freely
		Ignore
	}

	public class GatingConfig : SmpServerConfig
	{
		// percent of counted members that must be online
		[Range(0, 100)]
		[DefaultValue(75)]
		public int ThresholdPercent = 75;

		// never unlock with fewer than this many online
		[Range(0, 255)]
		[DefaultValue(2)]
		public int MinOnline = 2;

		// beaten bosses stay unlocked so people can farm them
		[DefaultValue(true)]
		public bool FirstKillOnly = true;

		// server chat message when bosses lock or unlock
		[DefaultValue(true)]
		public bool Announce = true;

		// modded npcs flagged as bosses that nothing registered (no boss checklist entry, no Mod.Call)
		[DefaultValue(UnknownBossPolicy.Gate)]
		public UnknownBossPolicy UnknownBosses = UnknownBossPolicy.Gate;

		// boss key -> gated, only bosses switched at least once are stored, missing keys count as gated
		[Header("Bosses")]
		[CustomModConfigItem(typeof(BossToggleTableElement))]
		public Dictionary<string, bool> GatedBosses = new();

		// -1 means off, otherwise replaces the real count so thresholds can be tested solo
		[Header("Debug")]
		[Range(-1, 255)]
		[DefaultValue(-1)]
		public int SimulatedOnline = -1;

		[Range(-1, 255)]
		[DefaultValue(-1)]
		public int SimulatedMembers = -1;
	}
}
