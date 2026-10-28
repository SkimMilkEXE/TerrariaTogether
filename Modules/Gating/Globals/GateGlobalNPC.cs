using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating.Globals
{
	// catch-all: any locked boss that spawns anyway is sent away on its first tick, never remove this
	public class GateGlobalNPC : GlobalNPC
	{
		private bool vetoed;

		public override bool InstancePerEntity => true;

		// only boss spawn npcs get an instance, so normal enemies don't pay for this
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return lateInstantiation && BossRegistry.Instance.BySpawnNpc(entity.type) != null;
		}

		// killing in OnSpawn is unreliable, so flag here and despawn in PreAI
		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (Main.netMode != NetmodeID.Server || !GatingModule.IsEnabled)
				return;

			BossEntry boss = BossRegistry.Instance.BySpawnNpc(npc.type);
			if (boss == null)
				return;

			LockResult result = GatingSystem.Instance.Evaluate(boss);
			if (!result.Locked)
				return;

			vetoed = true;
			Mod.Logger.Info($"gating: vetoed {boss.Key} spawn ({result.Online}/{result.Total}, need {result.Required})");
			ChatHelper.BroadcastChatMessage(GatingSystem.LockedText(boss, result), Color.OrangeRed);
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
