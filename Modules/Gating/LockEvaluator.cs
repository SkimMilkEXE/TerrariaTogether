using System;

namespace TerrariaTogether.Modules.Gating
{
	// admin override set with /smp gating lock|unlock
	public enum GateOverride
	{
		None,
		Unlocked,
		Locked
	}

	public enum LockReason
	{
		Threshold,
		Downed,
		NotGated,
		ForcedUnlock,
		ForcedLock
	}

	// Gated is false when the boss is switched off in the config's boss table
	public readonly record struct LockSnapshot(
		int Online, int Total, int ThresholdPercent, int MinOnline,
		bool Gated, bool Downed, bool FirstKillOnly, GateOverride Override);

	public readonly record struct LockResult(bool Locked, LockReason Reason, int Online, int Total, int Required);

	// pure: no Main.* access, everything comes in through the snapshot
	public static class LockEvaluator
	{
		public static LockResult Evaluate(LockSnapshot s)
		{
			// integer ceil of total * percent / 100
			int byPercent = (s.Total * s.ThresholdPercent + 99) / 100;
			int required = Math.Max(s.MinOnline, byPercent);

			// command overrides beat everything, then the config boss table, then first-kill-only, then the threshold
			if (s.Override == GateOverride.Locked)
				return new LockResult(true, LockReason.ForcedLock, s.Online, s.Total, required);
			if (s.Override == GateOverride.Unlocked)
				return new LockResult(false, LockReason.ForcedUnlock, s.Online, s.Total, required);
			if (!s.Gated)
				return new LockResult(false, LockReason.NotGated, s.Online, s.Total, required);
			if (s.FirstKillOnly && s.Downed)
				return new LockResult(false, LockReason.Downed, s.Online, s.Total, required);

			return new LockResult(s.Online < required, LockReason.Threshold, s.Online, s.Total, required);
		}
	}
}
