using Poncho.Display;
using Poncho.Geom;
using Poncho.Framework;
using Poncho.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace PonchoMonogame
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class MonogameApp : Game, IGameApp
	{
		private string[] _filetypes =
		{
			".jpg", ".png", ".bmp", ".dds", ".ppm", ".tga", ".spritefont"
		};

		private int _elapsed;
		private int _frames;
		private int _fpsInterval;
		private bool _started;
		private Sprite _mouseOver;
		private Sprite _prevMouseOver;
		private Color[] _pixels;
		private Rectangle _pixelRect;
		private Dictionary<string, Texture2D> _textures;

		private UpdateDelegate _onUpdate;
		private Action _onInit;

		public int fpsInterval { get { return _fpsInterval; } set { _fpsInterval = value > 0 ? value : 1000; } }
		public int fps { get; private set; }
		public int time { get; private set; }
		public int deltaTimeMs { get; private set; }
		public float deltaTime { get; private set; }
		public Stage stage { get; private set; }

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		
		// --------------------------------------------------------------
		public MonogameApp(Action onInit)
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1080;
			graphics.PreferredBackBufferHeight = 720;
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			fpsInterval = 1000;
			_onInit = onInit;
			_onUpdate = () => { };
			_textures = new Dictionary<string, Texture2D>();
			_pixels = new Color[]{ new Color(0, 0, 0, 0) };
			_pixelRect = new Rectangle(0, 0, 1, 1);
		}
		
		// --------------------------------------------------------------
		public void Start()
		{
			if(_started) return;
			_started = true;
			Run();
		}
		
		// --------------------------------------------------------------
		public void UpdateTime(int gameTime)
		{
			int newTime = gameTime;
			deltaTimeMs = newTime - time;
			deltaTime = deltaTimeMs * 0.001f;
			time = newTime;
			_elapsed += deltaTimeMs;
			++_frames;

			if(_elapsed >= fpsInterval)
			{
				_elapsed %= fpsInterval;
				fps = (int)(_frames * 1000f / fpsInterval);
				_frames = 0;
			}
		}
		
		// --------------------------------------------------------------
		public void Subscribe(UpdateDelegate onUpdate, bool add)
		{
			if(onUpdate == null) return;
			if(add) _onUpdate += onUpdate;
			else _onUpdate -= onUpdate;
		}
		
		// --------------------------------------------------------------
		public ITextureImage GetImage(string path) { return GetImage(path, null, null, null); }
		public ITextureImage GetImage(string path, Pivot pivot) { return GetImage(path, null, null, pivot); }
		public ITextureImage GetImage(string path, string name) { return GetImage(path, name, null, null); }
		public ITextureImage GetImage(string path, ImageRect rect) { return GetImage(path, null, rect, null); }
		public ITextureImage GetImage(string path, string name, Pivot pivot) { return GetImage(path, name, null, pivot); }
		public ITextureImage GetImage(string path, ImageRect rect, Pivot pivot) { return GetImage(path, null, rect, pivot); }
		public ITextureImage GetImage(string path, string name, ImageRect rect) { return GetImage(path, name, rect, null); }

		public ITextureImage GetImage(string path, string name, ImageRect rect, Pivot pivot)
		{
			name = name ?? path;
			Texture2D texture = GetTexture(path, name);
			rect = rect ?? new ImageRect(0, 0, (uint)texture.Width, (uint)texture.Height);
			pivot = pivot ?? new Pivot(0, 0);
			return new MonogameImage(name, rect, pivot);
		}
		
		// --------------------------------------------------------------
		private Texture2D GetTexture(string path, string name)
		{
			if(!_textures.ContainsKey(name))
			{
				Texture2D t = Content.Load<Texture2D>(GetPath(path));
				if(t != null)
				{
					_textures.Add(name, t);
				}
			}

			return _textures[name];
		}
		
		// --------------------------------------------------------------
		private Texture2D GetTexture(string name)
		{
			return _textures.ContainsKey(name) ? _textures[name] : null;
		}
		
		// --------------------------------------------------------------
		private string GetPath(string path)
		{
			string s = path.ToLower();
			foreach(string extension in _filetypes)
			{
				if( s.EndsWith(extension) )
				{
					int i = s.IndexOf(extension);
					return s.Substring(0, i);
				}
			}
			return s;
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			stage = new Stage();
			base.Initialize();
			_onInit?.Invoke();
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// TODO: Add your update logic here
			UpdateTime((int)gameTime.TotalGameTime.TotalMilliseconds);
			_onUpdate();
			base.Update(gameTime);
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			_prevMouseOver = _mouseOver;
			_mouseOver = null;
			GraphicsDevice.Clear(Color.CornflowerBlue);
			MouseState m = Mouse.GetState();

			// TODO: Add your drawing code here
			spriteBatch.Begin();
			int n = stage.numChildren;
			Transforms t = new Transforms();
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(stage.GetChildAt(i), t, m);
			}
			spriteBatch.End();

			base.Draw(gameTime);

			if(_mouseOver != _prevMouseOver)
			{
				//if(_mouseOver != null) Console.WriteLine("Mouse over");
				//else Console.WriteLine("Mouse out");
			}
			_prevMouseOver = null;
		}
		
		// --------------------------------------------------------------
		private Rectangle ToRect(ImageRect r)
		{
			return new Rectangle(r.x, r.y, (int)r.width, (int)r.height);
		}
		
		// --------------------------------------------------------------
		private void DrawSprite(Sprite sprite, Transforms t, MouseState m)
		{
			Transforms c = t.Concatenate(sprite.transforms);
			if(sprite.image != null) {
				Texture2D texture = GetTexture(sprite.image.name);
				if(texture != null)
				{
					Rectangle source = ToRect(sprite.image.rect);
					int w = (int)(source.Width * c.scaleX);
					int h = (int)(source.Height * c.scaleY);
					SpriteEffects eff = SpriteEffects.None;
					if( w < 0 ) {
						w *= -1;
						eff = eff | SpriteEffects.FlipHorizontally;
					}
					if( h < 0 ) {
						h *= -1;
						eff = eff | SpriteEffects.FlipVertically;
					}
					Rectangle dest = new Rectangle((int)c.x, (int)c.y, w, h);
					Vector2 pivot = new Vector2((int)(sprite.image.pivot.x), (int)(sprite.image.pivot.y));
				
					spriteBatch.Draw( texture, dest, source, Color.White, (float)(c.rotation * Math.PI / 180), pivot, eff, 0);

					// check mouse coords to see if we're in the texture boundaries
					float mouseX = (m.Position.X - dest.X);
					float mouseY = (m.Position.Y - dest.Y);
					float dist = Distance(m.Position.X, dest.X, m.Position.Y, dest.Y);
					double r = Math.Atan2(mouseY, mouseX) - (c.rotation * Math.PI / 180);
					mouseX = (float)(Math.Cos(r) * dist) / (c.scaleX >= 0 ? c.scaleX : -c.scaleX);
					mouseY = (float)(Math.Sin(r) * dist) / (c.scaleY >= 0 ? c.scaleY : -c.scaleY);
					
					if(c.scaleY < 0) { mouseY = source.Height - mouseY; }
					if(c.scaleX < 0) { mouseX = source.Width - mouseX; }

					if(mouseX >= 0 && mouseX < source.Width && mouseY >= 0 && mouseY < source.Height) {
						// pixel hit test
						// THIS IS EXPENSIVE AND REQUIRES GARBAGE COLLECTION TO BE CALLED A LOT
						/*_pixels[0].A = 0;
						_pixelRect.X = (int)mouseX;
						_pixelRect.Y = (int)mouseY;
						texture.GetData<Color>(0, _pixelRect, _pixels, 0, 1);
						if(_pixels[0].A > 0)
						{
							_mouseOver = sprite;
						}*/
						_mouseOver = sprite;
					}
				}
			}

			int n = sprite.numChildren;
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(sprite.GetChildAt(i), c, m);
			}
		}

		private float Distance(float x1, float x2, float y1, float y2)
		{
			return (float)Math.Sqrt( ((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)));
		}
	}
}
