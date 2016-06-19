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

		private Action _onInit;
		private Sprite _mouseOver;
		private Sprite _prevMouseOver;
		private Vector2 _empty;
		private Vector2 _pivot;
		private Rectangle _sourceRect;
		private SpriteBatch spriteBatch;
		private UpdateDelegate _onUpdate;
		private GraphicsDeviceManager graphics;
		private Dictionary<string, Texture2D> _textures;

		#region GETTERS
		public int fpsInterval { get { return _fpsInterval; } set { _fpsInterval = value > 0 ? value : 1000; } }
		public int fps { get; private set; }
		public int time { get; private set; }
		public int deltaTimeMs { get; private set; }
		public float deltaTime { get; private set; }
		public Stage stage { get; private set; }
		#endregion

		#region METHODS
		// --------------------------------------------------------------

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="onInit">Callback for when the app is ready.</param>
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
			_empty = new Vector2();
			_pivot = new Vector2();
			_sourceRect = new Rectangle();
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Runs the app.
		/// </summary>
		public void Start()
		{
			if(_started) return;
			_started = true;
			Run();
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Updates the total time, time since last render, and FPS.
		/// </summary>
		/// <param name="gameTime"></param>
		private void UpdateTime(int gameTime)
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
		/// <summary>
		/// Subscribes or unsubscribes an UpdateDelegate to be called when the game updates.
		/// </summary>
		/// <param name="onUpdate"></param>
		/// <param name="add"></param>
		public void Subscribe(UpdateDelegate onUpdate, bool add)
		{
			if(onUpdate == null) return;
			if(add) _onUpdate += onUpdate;
			else _onUpdate -= onUpdate;
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Returns an image.
		/// </summary>
		/// <param name="path">Path of the image to be loaded.</param>
		/// <returns></returns>
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
		/// <summary>
		/// Returns a texture.
		/// </summary>
		/// <param name="path">Path of the texture to be loaded.</param>
		/// <param name="name">Unique name of the texture.</param>
		/// <returns></returns>
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
		/// <summary>
		/// Returns a texture.
		/// </summary>
		/// <param name="name">Unique name of the texture.</param>
		/// <returns></returns>
		private Texture2D GetTexture(string name)
		{
			return _textures.ContainsKey(name) ? _textures[name] : null;
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Returns a valid path for the texture. Strips the file type from the path, as it is not used by Monogame.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
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
			
			int n = stage.numChildren;
			Matrix matrix = Matrix.Identity;
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(stage.GetChildAt(i), matrix, m);
			}
			
			base.Draw(gameTime);

			if(_mouseOver != _prevMouseOver)
			{
				//if(_mouseOver != null) Console.WriteLine("Mouse over");
				//else Console.WriteLine("Mouse out");
			}
			_prevMouseOver = null;
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Draws a sprite to the screen.
		/// </summary>
		/// <param name="sprite">Sprite to be rendered.</param>
		/// <param name="parentMatrix">Current matrix to use for positioning, rotating, and scaling the sprite.</param>
		/// <param name="mouseState">MouseState instance</param>
		private void DrawSprite(Sprite sprite, Matrix parentMatrix, MouseState mouseState)
		{
			Matrix matrix =  Matrix.CreateScale(sprite.scaleX, sprite.scaleY, 1) * Matrix.CreateRotationZ(MathHelper.ToRadians(sprite.rotation)) * Matrix.CreateTranslation(sprite.x, sprite.y, 0) * parentMatrix;
			
			if(sprite.image != null) {
				Texture2D texture = GetTexture(sprite.image.name);
				if(texture != null)
				{
					_pivot.X = sprite.image.pivot.x;
					_pivot.Y = sprite.image.pivot.y;
					_sourceRect.X = sprite.image.rect.x;
					_sourceRect.Y = sprite.image.rect.y;
					_sourceRect.Width = (int)sprite.image.rect.width;
					_sourceRect.Height = (int)sprite.image.rect.height;
					spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, null, matrix);
					spriteBatch.Draw(texture, _empty, _sourceRect, Color.White, 0, _pivot, 1, SpriteEffects.None, 0);
					spriteBatch.End();
				}
			}

			// TODO - Use the mouse state to detect if this sprite is the current object the mouse is over or clicked on.

			int n = sprite.numChildren;
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(sprite.GetChildAt(i), matrix, mouseState);
			}
		}
		#endregion
	}
}
