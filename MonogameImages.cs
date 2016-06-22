using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Poncho.Display;
using Poncho.Geom;
using Poncho.Interfaces;

namespace PonchoMonogame
{
	internal class MonogameImages : IAppImages
	{
		private ContentManager _content;
		private Dictionary<string, Texture2D> _textures;
		
		// --------------------------------------------------------------
		public MonogameImages(ContentManager content)
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
		
		// --------------------------------------------------------------
		/// <summary>
		/// Returns an image.
		/// </summary>
		/// <param name="path">Path of the image to be loaded.</param>
		/// <returns></returns>
		public Image GetImage(string path) { return GetImage(path, null, null, null); }
		public Image GetImage(string path, Pivot pivot) { return GetImage(path, null, null, pivot); }
		public Image GetImage(string path, string name) { return GetImage(path, name, null, null); }
		public Image GetImage(string path, ImageRect rect) { return GetImage(path, null, rect, null); }
		public Image GetImage(string path, string name, Pivot pivot) { return GetImage(path, name, null, pivot); }
		public Image GetImage(string path, ImageRect rect, Pivot pivot) { return GetImage(path, null, rect, pivot); }
		public Image GetImage(string path, string name, ImageRect rect) { return GetImage(path, name, rect, null); }

		public Image GetImage(string path, string name, ImageRect rect, Pivot pivot)
		{
			name = name ?? path;
			Texture2D texture = GetTexture(path, name);
			if (texture == null) return null;
			rect = rect ?? new ImageRect(0, 0, (ushort)texture.Width, (ushort)texture.Height);
			pivot = pivot ?? new Pivot(0, 0);
			return new Image(name, rect, pivot);
		}
	}
}
