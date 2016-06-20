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
		private Sprite _mouseTarget;
		private Sprite _prevMouseOver;
		private Vector2 _empty;
		private Vector2 _pivot;
		private Vector2 _mousePos;
		private Vector2[] _verts;
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
			_mousePos = new Vector2();
			_sourceRect = new Rectangle();
			_verts = new Vector2[4];
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
			rect = rect ?? new ImageRect(0, 0, (ushort)texture.Width, (ushort)texture.Height);
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
			_prevMouseOver = _mouseTarget;
			_mouseTarget = null;
			GraphicsDevice.Clear(Color.CornflowerBlue);
			MouseState m = Mouse.GetState();
			_mousePos.X = m.Position.X;
			_mousePos.Y = m.Position.Y;
			
			int n = stage.numChildren;
			Matrix matrix = Matrix.Identity;
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(stage.GetChildAt(i), matrix);
			}
			
			// debug to see if the mouse target changed
			/*if(_mouseTarget != _prevMouseOver)
			{
				if(_mouseTarget != null) Console.WriteLine("Mouse over");
				else Console.WriteLine("Mouse out");
			}*/

			_prevMouseOver = null;
			base.Draw(gameTime);
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Draws a sprite to the screen.
		/// </summary>
		/// <param name="sprite">Sprite to be rendered.</param>
		/// <param name="parentMatrix">Current matrix to use for positioning, rotating, and scaling the sprite.</param>
		/// <param name="mouseState">MouseState instance</param>
		private void DrawSprite(Sprite sprite, Matrix parentMatrix)
		{
			Matrix matrix =  Matrix.CreateScale(sprite.scaleX, sprite.scaleY, 1) * Matrix.CreateRotationZ(MathHelper.ToRadians(sprite.rotation)) * Matrix.CreateTranslation(sprite.x, sprite.y, 0) * parentMatrix;
			
			if(sprite.image != null) {
				Texture2D texture = GetTexture(sprite.image.name);
				if(texture != null) // we have a texture, so render it onto the screen
				{
					// set the pivot
					_pivot.X = sprite.image.pivot.x * sprite.image.rect.width;
					_pivot.Y = sprite.image.pivot.y * sprite.image.rect.height;

					// grab the source rect from the texture
					_sourceRect.X = sprite.image.rect.x;
					_sourceRect.Y = sprite.image.rect.y;
					_sourceRect.Width = sprite.image.rect.width;
					_sourceRect.Height = sprite.image.rect.height;

					// setup the render with the appropriate matrix and draw it
					spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, null, matrix);
					spriteBatch.Draw(texture, _empty, _sourceRect, Color.White, 0, _pivot, 1, SpriteEffects.None, 0);
					spriteBatch.End();

					// Use the mouse state to detect if this sprite is the current object the mouse is over or clicked on.

					// Grab the vertices for each corner of the sprite
					_verts[0].X = -_pivot.X;
					_verts[0].Y = -_pivot.Y;
					_verts[1].X = sprite.imageWidth - _pivot.X;
					_verts[1].Y = -_pivot.Y;
					_verts[2].X = -_pivot.X;
					_verts[2].Y = sprite.imageHeight - _pivot.Y;
					_verts[3].X = sprite.imageWidth - _pivot.X;
					_verts[3].Y = sprite.imageHeight - _pivot.Y;

					// transform the vertices
					for( int i = 0; i < 4; ++i )
					{
						ConvertPoint(ref _verts[i], matrix);
					}

					// sort them by y coordinates and then by x coordinates
					Array.Sort(_verts, 
						(j, k) => {
							if(j.Y < k.Y) return -1;
							if(j.Y > k.Y) return 1;
							if(j.X < k.X) return -1;
							if(j.X > k.X) return 1;
							return 0;
						}
					);
					
					// Check to see if the mouse is in the sprite bounds
					if(PointInTri(_mousePos, _verts[0], _verts[1], _verts[2]) || PointInTri(_mousePos, _verts[2], _verts[1], _verts[3]))
					{
						// if so, this is the active mouse target
						_mouseTarget = sprite;
					}
				}
			}
			
			// now, render all the children
			int n = sprite.numChildren;
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(sprite.GetChildAt(i), matrix);
			}
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Checks to see if the specified point lies within the three other points
		/// </summary>
		/// <param name="pt"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool PointInTri( Vector2 pt, Vector2 a, Vector2 b, Vector2 c)
		{
			bool b1, b2, b3;

			b1 = GetSign(pt, a, b) < 0.0f;
			b2 = GetSign(pt, b, c) < 0.0f;
			b3 = GetSign(pt, c, a) < 0.0f;

			return ((b1 == b2) && (b2 == b3));
		}
		
		// --------------------------------------------------------------
		private float GetSign( Vector2 a, Vector2 b, Vector2 c )
		{
			return (a.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (a.Y - c.Y);
		}
		
		// --------------------------------------------------------------
		
		/// <summary>
		/// Adjusts the specified point using the specified matrix.
		/// </summary>
		/// <param name="pt"></param>
		/// <param name="m"></param>
		private void ConvertPoint( ref Vector2 pt, Matrix m )
		{
			float a		= m.M11;
			float b		= m.M12;
			float c		= m.M21;
			float d		= m.M22;
			float tx	= m.M41;
			float ty	= m.M42;
			
			float x		= (pt.X * a) + (pt.Y * c) + tx;
			float y		= (pt.X * b) + (pt.Y * d) + ty;

			pt.X		= x;
			pt.Y		= y;
		}
		#endregion
	}
}
