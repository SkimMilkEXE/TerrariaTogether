using System;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;

namespace TerrariaTogether.Modules.Gating.Registry
{
	// the only place hardcoded vanilla boss ids are allowed
	public static class VanillaBosses
	{
		public static void Register(BossRegistry registry)
		{
			int[] none = Array.Empty<int>();

			void Add(string name, int[] spawn, int[] parts, int[] items, Func<bool> downed, Action<BossEntry> extra = null)
			{
				var entry = new BossEntry
				{
					Key = "Terraria/" + name,
					DisplayName = Language.GetText("Mods.TerrariaTogether.Gating.Bosses." + name),
					SpawnNpcs = spawn,
					PartNpcs = parts,
					SummonItems = items,
					IsDowned = downed,
				};
				extra?.Invoke(entry);
				registry.Register(entry);
			}

			void AddEvent(string name, int[] items, Func<bool> running, Action stop, Func<bool> downed)
			{
				Add(name, none, none, items, downed, e =>
				{
					e.IsEvent = true;
					e.IsRunning = running;
					e.Stop = stop;
				});
			}

			// pre-hardmode
			Add("KingSlime", new int[] { NPCID.KingSlime }, none,
				new int[] { ItemID.SlimeCrown }, () => NPC.downedSlimeKing);
			Add("EyeOfCthulhu", new int[] { NPCID.EyeofCthulhu }, none,
				new int[] { ItemID.SuspiciousLookingEye }, () => NPC.downedBoss1);
			Add("EaterOfWorlds", new int[] { NPCID.EaterofWorldsHead }, new int[] { NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail },
				new int[] { ItemID.WormFood }, () => NPC.downedBoss2);
			Add("BrainOfCthulhu", new int[] { NPCID.BrainofCthulhu }, new int[] { NPCID.Creeper },
				new int[] { ItemID.BloodySpine }, () => NPC.downedBoss2);
			Add("QueenBee", new int[] { NPCID.QueenBee }, none,
				new int[] { ItemID.Abeemination }, () => NPC.downedQueenBee);
			// summoned by talking to the clothier at night, so only the npc veto catches it
			Add("Skeletron", new int[] { NPCID.SkeletronHead }, new int[] { NPCID.SkeletronHand },
				none, () => NPC.downedBoss3);
			Add("Deerclops", new int[] { NPCID.Deerclops }, none,
				new int[] { ItemID.DeerThing }, () => NPC.downedDeerclops);
			// voodoo doll dies in lava instead of being used, so it's kept out of lava while locked
			Add("WallOfFlesh", new int[] { NPCID.WallofFlesh }, new int[] { NPCID.WallofFleshEye },
				none, () => Main.hardMode, e => e.LavaSummonItems = new int[] { ItemID.GuideVoodooDoll });

			// hardmode
			Add("QueenSlime", new int[] { NPCID.QueenSlimeBoss }, none,
				new int[] { ItemID.QueenSlimeCrystal }, () => NPC.downedQueenSlime);
			// both twins spawn directly from the item
			Add("TheTwins", new int[] { NPCID.Retinazer, NPCID.Spazmatism }, none,
				new int[] { ItemID.MechanicalEye, ItemID.MechdusaSummon }, () => NPC.downedMechBoss2);
			Add("TheDestroyer", new int[] { NPCID.TheDestroyer }, new int[] { NPCID.TheDestroyerBody, NPCID.TheDestroyerTail },
				new int[] { ItemID.MechanicalWorm, ItemID.MechdusaSummon }, () => NPC.downedMechBoss1);
			Add("SkeletronPrime", new int[] { NPCID.SkeletronPrime }, new int[] { NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser },
				new int[] { ItemID.MechanicalSkull, ItemID.MechdusaSummon }, () => NPC.downedMechBoss3);
			// spawned by breaking a bulb
			Add("Plantera", new int[] { NPCID.Plantera }, new int[] { NPCID.PlanterasHook, NPCID.PlanterasTentacle },
				none, () => NPC.downedPlantBoss);
			// power cell goes on the altar (tile interaction, not item use), refunded if vetoed
			Add("Golem", new int[] { NPCID.Golem }, new int[] { NPCID.GolemHead, NPCID.GolemHeadFree, NPCID.GolemFistLeft, NPCID.GolemFistRight },
				none, () => NPC.downedGolemBoss, e => e.RefundItem = ItemID.LihzahrdPowerCell);
			// truffle worm is fishing bait, not item use, refunded if vetoed
			Add("DukeFishron", new int[] { NPCID.DukeFishron }, none,
				none, () => NPC.downedFishron, e => e.RefundItem = ItemID.TruffleWorm);
			// spawned by killing a prismatic lacewing
			Add("EmpressOfLight", new int[] { NPCID.HallowBoss }, none,
				none, () => NPC.downedEmpressOfLight);
			// spawned by killing the dungeon cultists
			Add("LunaticCultist", new int[] { NPCID.CultistBoss }, new int[] { NPCID.CultistBossClone },
				none, () => NPC.downedAncientCultist);
			Add("MoonLord", new int[] { NPCID.MoonLordCore }, new int[] { NPCID.MoonLordHead, NPCID.MoonLordHand, NPCID.MoonLordFreeEye, NPCID.MoonLordLeechBlob },
				new int[] { ItemID.CelestialSigil }, () => NPC.downedMoonlord);

			// events, stopped the moment they start while locked (natural or summoned)
			AddEvent("GoblinArmy", new int[] { ItemID.GoblinBattleStandard },
				() => Main.invasionType == InvasionID.GoblinArmy, StopInvasion, () => NPC.downedGoblins);
			AddEvent("FrostLegion", new int[] { ItemID.SnowGlobe },
				() => Main.invasionType == InvasionID.SnowLegion, StopInvasion, () => NPC.downedFrost);
			AddEvent("Pirates", new int[] { ItemID.PirateMap },
				() => Main.invasionType == InvasionID.PirateInvasion, StopInvasion, () => NPC.downedPirates);
			AddEvent("MartianMadness", none,
				() => Main.invasionType == InvasionID.MartianMadness, StopInvasion, () => NPC.downedMartians);
			AddEvent("PumpkinMoon", new int[] { ItemID.PumpkinMoonMedallion },
				() => Main.pumpkinMoon, StopMoon, () => NPC.downedHalloweenKing);
			AddEvent("FrostMoon", new int[] { ItemID.NaughtyPresent },
				() => Main.snowMoon, StopMoon, () => NPC.downedChristmasIceQueen);
			// eternia crystal goes on its stand (tile interaction), so only the start check catches it
			AddEvent("OldOnesArmy", none,
				() => DD2Event.Ongoing, () => DD2Event.StopInvasion(), () => DD2Event.DownedInvasionT1);
		}

		private static void StopInvasion()
		{
			Main.invasionType = 0;
			Main.invasionSize = 0;
			Main.invasionDelay = 0;
			NetMessage.SendData(MessageID.WorldData);
		}

		private static void StopMoon()
		{
			Main.stopMoonEvent();
			NetMessage.SendData(MessageID.WorldData);
		}
	}
}
