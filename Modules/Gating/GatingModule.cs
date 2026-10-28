using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Core.Modules;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating
{
	// bosses stay locked until enough members are online
	public class GatingModule : SmpModule
	{
		public const string ModuleId = "gating";

		public override string Id => ModuleId;

		// for globals, so they don't look the module up every call
		public static bool IsEnabled => ModuleManager.IsEnabled(ModuleId);

		private static Mod Mod => ModContent.GetInstance<TerrariaTogether>();

		public override void HandleCommand(CommandCaller caller, string[] args)
		{
			GateCommands.Handle(caller, args);
		}

		public override void HandlePacket(byte msgType, BinaryReader reader, int fromWho)
		{
			GatingSystem.Instance.HandlePacket(msgType, reader, fromWho);
		}

		// Mod.Call("Gating.<name>", ...), see docs/API.md
		public override object Call(string name, object[] args)
		{
			switch (name)
			{
				case "RegisterBoss":
					return RegisterBoss(args);
				case "IsLocked":
					// null when the boss isn't known, false whenever gating isn't enforced
					BossEntry boss = args.Length > 0 && args[0] is string key ? BossRegistry.Instance.Get(key) : null;
					if (boss == null)
						return null;
					return IsEnabled && Main.netMode != NetmodeID.SinglePlayer && GatingSystem.Instance.Evaluate(boss).Locked;
				default:
					return null;
			}
		}

		// args: key, displayName (LocalizedText or string), npcTypes, [summonItems], [downed]
		private static bool RegisterBoss(object[] args)
		{
			if (args.Length < 3 || args[0] is not string key || !key.Contains('/'))
			{
				Mod.Logger.Warn("Gating.RegisterBoss needs a \"ModName/BossName\" key, a display name and npc types");
				return false;
			}

			int[] npcs = ToIntArray(args[2]);
			LocalizedText displayName = args[1] as LocalizedText ?? (args[1] is string text ? Language.GetText(text) : null);
			if (npcs.Length == 0 || displayName == null)
			{
				Mod.Logger.Warn($"Gating.RegisterBoss '{key}': display name or npc types missing");
				return false;
			}

			return BossRegistry.Instance.Register(new BossEntry
			{
				Key = key,
				DisplayName = displayName,
				SpawnNpcs = npcs,
				PartNpcs = Array.Empty<int>(),
				SummonItems = args.Length > 3 ? ToIntArray(args[3]) : Array.Empty<int>(),
				IsDowned = args.Length > 4 && args[4] is Func<bool> downed ? downed : () => false,
			});
		}

		// accepts a single int or any list/array of ints
		private static int[] ToIntArray(object value)
		{
			return value switch
			{
				int single => new[] { single },
				IEnumerable<int> many => many.ToArray(),
				_ => Array.Empty<int>(),
			};
		}
	}
}
