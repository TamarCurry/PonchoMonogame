using Poncho.Geom;
using Poncho.Interfaces;

namespace PonchoMonogame
{
	internal class MonogameImage : ITextureImage
	{
		public string name { get; }
		public Pivot pivot { get; }
		public ImageRect rect { get; }

		public MonogameImage(string name, ImageRect rect, Pivot pivot)
		{
			this.name = name;
			this.rect = rect;
			this.pivot = pivot;
		}
	}
}
