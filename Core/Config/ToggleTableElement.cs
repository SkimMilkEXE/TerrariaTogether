using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using TerrariaTogether.Core.Modules;

namespace TerrariaTogether.Core.Config
{
	// draws a Dictionary<string, bool> as a fixed list of named checkboxes instead of editable key/value rows
	// rows come from code (registry, module list), so entries missing from the dictionary show as on
	public abstract class ToggleTableElement : ConfigElement<Dictionary<string, bool>>
	{
		// a row with a null key is drawn as a group heading instead of a toggle
		protected abstract IEnumerable<(string Key, string Label)> Rows();

		public override void OnBind()
		{
			base.OnBind();

			Dictionary<string, bool> table = Value;
			if (table == null)
			{
				table = new Dictionary<string, bool>();
				Value = table;
			}

			// one ToggleRowElement per row, laid out by tmodloader's normal wrapper
			var wrapper = new PropertyFieldWrapper(typeof(ToggleRow).GetProperty(nameof(ToggleRow.Enabled)));
			int top = 30; // leaves room for this element's own label
			int order = 0;
			foreach ((string key, string label) in Rows())
			{
				if (key == null)
				{
					var heading = new UIText(label, 0.9f);
					heading.Top.Set(top + 6, 0f);
					heading.Left.Set(12, 0f);
					Append(heading);
					top += 30;
					continue;
				}
				ConfigManager.WrapIt(this, ref top, wrapper, new ToggleRow(table, key, label), order++);
			}

			// ui elements are capped at screen height by default, long lists (calamity + vanilla) go past that
			MaxHeight.Set(int.MaxValue, 0f);
			Height.Set(top, 0f);
		}
	}

	// one checkbox in a ToggleTableElement, reads and writes a single dictionary entry
	public class ToggleRow
	{
		private readonly Dictionary<string, bool> table;
		private readonly string key;

		public string Label { get; }

		public ToggleRow(Dictionary<string, bool> table, string key, string label)
		{
			this.table = table;
			this.key = key;
			Label = label;
		}

		[CustomModConfigItem(typeof(ToggleRowElement))]
		public bool Enabled
		{
			get => table.GetValueOrDefault(key, true);
			set => table[key] = value;
		}
	}

	// tmodloader's on/off toggle, but labelled with the row's name instead of a localization key
	public class ToggleRowElement : ConfigElement<bool>
	{
		private Asset<Texture2D> toggleTexture;

		public override void OnBind()
		{
			base.OnBind();
			toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");
			TextDisplayFunction = () => ((ToggleRow)Item).Label;
			OnLeftClick += (_, _) => Value = !Value;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			// the texture holds the off and on frames side by side
			CalculatedStyle dimensions = GetDimensions();
			int frameWidth = (toggleTexture.Width() - 2) / 2;
			var source = new Rectangle(Value ? frameWidth + 2 : 0, 0, frameWidth, toggleTexture.Height());
			var position = new Vector2(dimensions.X + dimensions.Width - frameWidth - 10f, dimensions.Y + 8f);
			spriteBatch.Draw(toggleTexture.Value, position, source, Color.White);

			// "On" / "Off" just left of the toggle
			string text = Language.GetTextValue("Mods.TerrariaTogether.Configs.Toggle." + (Value ? "On" : "Off"));
			float textWidth = FontAssets.MouseText.Value.MeasureString(text).X * 0.8f;
			var textPosition = new Vector2(position.X - textWidth - 8f, dimensions.Y + 8f);
			Utils.DrawBorderString(spriteBatch, text, textPosition, Color.White, 0.8f);
		}
	}

	// one row per module in the Modules table
	public class ModuleToggleTableElement : ToggleTableElement
	{
		protected override IEnumerable<(string Key, string Label)> Rows()
		{
			return ModuleManager.Instance.Modules.Select(m => (m.Id, Language.GetTextValue("Mods.TerrariaTogether.ModuleNames." + m.Id)));
		}
	}
}
