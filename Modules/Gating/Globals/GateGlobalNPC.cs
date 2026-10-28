using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating.Globals
{
	// catch-all: any locked boss that spawns anyway is sent away on its first tick, never remove this
	public class GateGlobalNPC : GlobalNPC
	{
		private bool vetoed;

		public override bool InstancePerEntity => true;

		// only boss npcs get an instance, so normal enemies don't pay for this
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return lateInstantiation && (BossRegistry.Instance.ByNpc(entity.type) != null || BossRegistry.IsUnknownBoss(entity));
		}

		// killing in OnSpawn is unreliable, so flag here and despawn in PreAI
		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (Main.netMode != NetmodeID.Server || !GatingModule.IsEnabled)
				return;

			BossEntry boss = BossRegistry.Instance.Resolve(npc);
			if (boss == null || FightRunning(boss, npc))
				return;

			LockResult result = GatingSystem.Instance.Evaluate(boss);
			if (!result.Locked)
				return;

			vetoed = true;
			Mod.Logger.Info($"gating: vetoed {boss.Key} spawn ({result.Online}/{result.Total}, need {result.Required})");
			ChatHelper.BroadcastChatMessage(GatingSystem.LockedText(boss, result), Color.OrangeRed);
		}

		// minions and segments spawned mid-fight are left alone, a locked boss only stops new fights
		// npcs already being vetoed don't count, so both twins are stopped when they spawn together
		private static bool FightRunning(BossEntry boss, NPC self)
		{
			foreach (NPC other in Main.ActiveNPCs)
			{
				if (other.whoAmI == self.whoAmI || BossRegistry.Instance.Resolve(other) != boss)
					continue;
				if (!other.TryGetGlobalNPC(out GateGlobalNPC gate) || !gate.vetoed)
					return true;
			}
			return false;
		}

		public override bool PreAI(NPC npc)
		{
			if (!vetoed)
				return true;

			// parts are spawned by the boss's own ai, so stopping it here means they never appear
			npc.active = false;
			NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			return false;
		}
	}
}
