using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Graphics.ES11;
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
		private Image HandleImageRequest(string path, string name, ImageRect rect, Pivot pivot, ImageRectF rectf, PivotF pivotf)
		{
			name = name ?? path;
			Texture2D texture = GetTexture(path, name);
			if (texture == null) return null;

			ushort w	= (ushort) texture.Width;
			ushort h	= (ushort) texture.Height;
			ushort x	= 0;
			ushort y	= 0;
			ushort pX	= 0;
			ushort pY	= 0;

			if (rectf != null)
			{
				x = (ushort) (texture.Width*rectf.x);
				y = (ushort) (texture.Height*rectf.y);
				w = (ushort) (texture.Width*rectf.width);
				h = (ushort) (texture.Height*rectf.height);
			}

			if (pivotf != null)
			{
				pX = (ushort) (pivotf.x * texture.Width);
				pY = (ushort) (pivotf.y * texture.Height);
			}

			rect = rect ?? new ImageRect(x, y, w, h);
			pivot = pivot ?? new Pivot(pX, pY);
			
			return new Image(name, rect, pivot);
		}

		// --------------------------------------------------------------
		/// <summary>
		/// Returns an image.
		/// </summary>
		/// <param name="path">Path of the image to be loaded.</param>
		/// <returns></returns>
		public Image GetImage(string path)
		{
			return HandleImageRequest(path, null, null, null, null, null);
		}

		public Image GetImage(string path, Pivot pivot)
		{
			return HandleImageRequest(path, null, null, pivot, null, null);
		}
		
		public Image GetImage(string path, PivotF pivot)
		{
			return HandleImageRequest(path, null, null, null, null, pivot);
		}

		public Image GetImage(string path, string name)
		{
			return HandleImageRequest(path, name, null, null, null, null);
		}

		public Image GetImage(string path, ImageRect rect)
		{
			return HandleImageRequest(path, null, rect, null, null, null);
		}
		
		public Image GetImage(string path, ImageRectF rect)
		{
			return HandleImageRequest(path, null, null, null, rect, null);
		}

		public Image GetImage(string path, string name, Pivot pivot)
		{
			return HandleImageRequest(path, name, null, pivot, null, null);
		}
		
		public Image GetImage(string path, string name, PivotF pivot)
		{
			return HandleImageRequest(path, name, null, null, null, pivot);
		}

		public Image GetImage(string path, ImageRect rect, Pivot pivot)
		{
			return HandleImageRequest(path, null, rect, pivot, null, null);
		}
		
		public Image GetImage(string path, ImageRect rect, PivotF pivot)
		{
			return HandleImageRequest(path, null, rect, null, null, pivot);
		}
		
		public Image GetImage(string path, ImageRectF rect, Pivot pivot)
		{
			return HandleImageRequest(path, null, null, pivot, rect, null);
		}
		
		public Image GetImage(string path, ImageRectF rect, PivotF pivot)
		{
			return HandleImageRequest(path, null, null, null, rect, pivot);
		}
		
		public Image GetImage(string path, string name, ImageRect rect)
		{
			return HandleImageRequest(path, name, rect, null, null, null);
		}
		
		public Image GetImage(string path, string name, ImageRectF rect)
		{
			return HandleImageRequest(path, name, null, null, rect, null);
		}

		public Image GetImage(string path, string name, ImageRect rect, Pivot pivot)
		{
			return HandleImageRequest(path, name, rect, pivot, null, null);
		}

		public Image GetImage(string path, string name, ImageRectF rect, PivotF pivot)
		{
			return HandleImageRequest(path, name, null, null, rect, pivot);
		}
	}
}
