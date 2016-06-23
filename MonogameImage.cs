using Microsoft.Xna.Framework.Graphics;
using Poncho.Display;
using Poncho.Geom;

namespace PonchoMonogame
{
	internal class MonogameImage : Image
	{
		public Texture2D texture { get; private set; }
		public MonogameImage(string name, Texture2D texture, ImageRect rect, Pivot pivot) : base(name, rect, pivot)
		{
			this.texture = texture;
		}
	}
}
