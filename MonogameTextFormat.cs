using Microsoft.Xna.Framework.Graphics;
using Poncho.Text;

namespace PonchoMonogame
{
	internal class MonogameTextFormat : TextFormat
	{
		public SpriteFont spriteFont { get; private set; }

		public MonogameTextFormat(string font, ushort size, SpriteFont spriteFont) : base(font, size)
		{
			this.spriteFont = spriteFont;
		}
	}
}
