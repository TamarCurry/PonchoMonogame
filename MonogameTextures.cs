using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonchoMonogame
{
	internal class MonogameTextures
	{
		private ContentManager _content;
		private Dictionary<string, Texture2D> _textures;
		
		// --------------------------------------------------------------
		public MonogameTextures(ContentManager content)
		{
			_content = content;
			_textures = new Dictionary<string, Texture2D>();
		}

		// --------------------------------------------------------------
		/// <summary>
		/// Returns a texture.
		/// </summary>
		/// <param name="path">Path of the texture to be loaded.</param>
		/// <param name="name">Unique name of the texture.</param>
		/// <returns></returns>
		public Texture2D GetTexture(string path, string name)
		{
			Texture2D t = null;
			if(!_textures.TryGetValue(name, out t))
			{
				t = _content.Load<Texture2D>(MonogameFuncs.GetPath(path));
				if(t != null) _textures.Add(name, t);
			}
			
			return t;
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Returns a texture.
		/// </summary>
		/// <param name="name">Unique name of the texture.</param>
		/// <returns></returns>
		public Texture2D GetTexture(string name)
		{
			Texture2D t = null;
			_textures.TryGetValue(name, out t);
			return t;
		}
		
	}
}
