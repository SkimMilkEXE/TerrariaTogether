using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Modules.Gating.Globals;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating
{
	// server only: stops locked events as they start, and applies the mid-fight policy to running boss fights
	public class FightWatchSystem : ModSystem
	{
		private const string Key = "Mods.TerrariaTogether.Gating.MidFight.";

		// event running state last tick, to spot the moment one starts
		private bool[] eventWasRunning;
		// the first tick after a world loads only records state, so saved mid-event worlds aren't cancelled
		private bool eventsPrimed;

		// locked bosses mid-fight -> tick they leave at, under the despawn policy
		private readonly Dictionary<BossEntry, uint> leaving = new();
		private readonly HashSet<BossEntry> fighting = new();

		public override void PostUpdateEverything()
		{
			if (Main.netMode != NetmodeID.Server || !GatingModule.IsEnabled)
				return;

			CheckEvents();
			if (Main.GameUpdateCount % 60 == 0)
				CheckFights();
		}

		private void CheckEvents()
		{
			IReadOnlyList<BossEntry> events = BossRegistry.Instance.Events;
			if (eventWasRunning == null || eventWasRunning.Length != events.Count)
			{
				eventWasRunning = new bool[events.Count];
				eventsPrimed = false;
			}

			for (int i = 0; i < events.Count; i++)
			{
				BossEntry gameEvent = events[i];
				bool running = gameEvent.IsRunning();

				// only a fresh start is stopped, an event already under way always finishes
				if (running && !eventWasRunning[i] && eventsPrimed)
				{
					LockResult result = GatingSystem.Instance.Evaluate(gameEvent);
					if (result.Locked)
					{
						gameEvent.Stop();
						running = false;
						Mod.Logger.Info($"gating: stopped {gameEvent.Key} ({result.Online}/{result.Total}, need {result.Required})");
						ChatHelper.BroadcastChatMessage(GatingSystem.LockedText(gameEvent, result), Color.OrangeRed);
					}
				}
				eventWasRunning[i] = running;
			}
			eventsPrimed = true;
		}

		private void CheckFights()
		{
			fighting.Clear();
			foreach (NPC npc in Main.ActiveNPCs)
			{
				BossEntry boss = BossRegistry.Instance.Resolve(npc);
				if (boss != null && !GateGlobalNPC.IsVetoed(npc))
					fighting.Add(boss);
			}

			// forget bosses whose fight ended
			if (leaving.Count > 0)
			{
				foreach (BossEntry boss in leaving.Keys.Where(b => !fighting.Contains(b)).ToList())
					leaving.Remove(boss);
			}

			GatingConfig config = ModContent.GetInstance<GatingConfig>();
			if (config.MidFight != MidFightPolicy.DespawnAfterWarning)
			{
				leaving.Clear();
				return;
			}

			foreach (BossEntry boss in fighting)
			{
				LockResult result = GatingSystem.Instance.Evaluate(boss);
				NetworkText name = NetworkText.FromKey(boss.DisplayName.Key);
				bool warned = leaving.TryGetValue(boss, out uint leaveTick);

				if (!result.Locked)
				{
					if (warned)
					{
						leaving.Remove(boss);
						ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Key + "Stays", name), Color.LightGreen);
					}
					continue;
				}

				if (!warned)
				{
					leaving[boss] = Main.GameUpdateCount + (uint)config.MidFightWarningSeconds * 60;
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Key + "Warning", name,
						result.Online, result.Total, result.Required, config.MidFightWarningSeconds), Color.OrangeRed);
				}
				else if (Main.GameUpdateCount >= leaveTick)
				{
					Despawn(boss);
					leaving.Remove(boss);
					Mod.Logger.Info($"gating: {boss.Key} left mid-fight ({result.Online}/{result.Total}, need {result.Required})");
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Key + "Left", name), Color.OrangeRed);
				}
			}
		}

		// removes every npc of the fight, parts and minions registered to it included
		private static void Despawn(BossEntry boss)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (BossRegistry.Instance.Resolve(npc) != boss)
					continue;
				npc.active = false;
				NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			}
		}

		public override void ClearWorld()
		{
			eventWasRunning = null;
			eventsPrimed = false;
			leaving.Clear();
		}
	}
}
