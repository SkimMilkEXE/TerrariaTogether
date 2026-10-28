using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaTogether.Modules.Gating.Registry;

namespace TerrariaTogether.Modules.Gating.Globals
{
	// blocks summon items while their boss is locked, so the item isn't used up
	public class GateGlobalItem : GlobalItem
	{
		// stops the message repeating every frame while the mouse is held
		// static because globals are shared, tmodloader refuses instance fields without InstancePerEntity
		private static uint lastMessageTick;

		public override bool CanUseItem(Item item, Player player)
		{
			if (!TryFindLocked(item, out BossEntry boss, out LockResult result) || !result.Locked)
				return true;

			if (player.whoAmI == Main.myPlayer && Main.GameUpdateCount - lastMessageTick > 60)
			{
				lastMessageTick = Main.GameUpdateCount;
				Main.NewText(GatingSystem.LockedText(boss, result).ToString(), Color.OrangeRed);
			}
			return false;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!TryFindLocked(item, out _, out LockResult result))
				return;

			string text = !result.Locked
				? Language.GetTextValue("Mods.TerrariaTogether.Gating.Tooltip.Unlocked")
				: result.Reason == LockReason.ForcedLock
					? Language.GetTextValue("Mods.TerrariaTogether.Gating.Tooltip.LockedByAdmin")
					: Language.GetTextValue("Mods.TerrariaTogether.Gating.Tooltip.Locked", result.Online, result.Total, result.Required);

			tooltips.Add(new TooltipLine(Mod, "GateState", text) { OverrideColor = result.Locked ? Color.OrangeRed : Color.LightGreen });
		}

		// finds the first locked boss this item summons, or the last unlocked one if none are locked
		private static bool TryFindLocked(Item item, out BossEntry boss, out LockResult result)
		{
			boss = null;
			result = default;
			if (!GatingModule.IsEnabled || Main.netMode == NetmodeID.SinglePlayer)
				return false;

			List<BossEntry> bosses = BossRegistry.Instance.BySummonItem(item.type);
			if (bosses == null)
				return false;

			foreach (BossEntry entry in bosses)
			{
				boss = entry;
				result = GatingSystem.Instance.Evaluate(entry);
				if (result.Locked)
					break;
			}
			return true;
		}
	}
}
