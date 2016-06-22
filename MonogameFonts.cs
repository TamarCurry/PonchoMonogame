using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PonchoMonogame
{
	internal class MonogameFonts
	{
		private ContentManager _content;
		private Dictionary<string, SpriteFont> _fonts;
		
		// --------------------------------------------------------------
		public MonogameFonts(ContentManager content)
		{
			_content = content;
			_fonts = new Dictionary<string, SpriteFont>();
		}
		
		// --------------------------------------------------------------
		public SpriteFont GetFont(string name, string path)
		{
			SpriteFont font = null;
			if (!_fonts.TryGetValue(name, out font))
			{
				font = _content.Load<SpriteFont>(MonogameFuncs.GetPath(path));
				if (font != null) _fonts.Add(name, font);
			}
			
			return font;
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Returns a texture.
		/// </summary>
		/// <param name="name">Unique name of the texture.</param>
		/// <returns></returns>
		public SpriteFont GetFont(string name)
		{
			SpriteFont font = null;
			_fonts.TryGetValue(name, out font);
			return font;
		}
		
	}
}
