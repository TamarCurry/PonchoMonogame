namespace PonchoMonogame
{
	internal class MonogameFuncs
	{
		private static string[] _filetypes =
		{
			".jpg", ".png", ".bmp", ".dds", ".ppm", ".tga", ".spritefont"
		};

		// --------------------------------------------------------------
		/// <summary>
		/// Returns a valid path for the texture. Strips the file type from the path, as it is not used by Monogame.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetPath(string path)
		{
			string s = path.ToLower();
			foreach(string extension in _filetypes)
			{
				if( s.EndsWith(extension) )
				{
					int i = s.IndexOf(extension);
					return path.Substring(0, i);
				}
			}
			return s;
		}
		
	}
}
