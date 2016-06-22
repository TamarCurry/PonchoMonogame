using Poncho.Display;
using Poncho.Geom;
using Poncho.Framework;
using Poncho.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Poncho.Events;
using Poncho.Text;

namespace PonchoMonogame
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class MonogameApp : Game, IGameApp
	{
		private enum MouseButton
		{
			LEFT,
			RIGHT,
			MIDDLE
		}

		private struct MouseButtonState
		{
			public Sprite downTarget;
			public Sprite upTarget;
			public MouseButton button {get; private set; }
			public string downEventType { get; private set; }
			public string upEventType { get; private set; }
			public string clickEventType { get; private set; }

			public MouseButtonState(MouseButton button, string downEventType, string upEventType, string clickEventType)
			{
				this.button = button;
				this.downEventType = downEventType;
				this.upEventType = upEventType;
				this.clickEventType = clickEventType;
				downTarget = null;
				upTarget = null;
			}
		}
		
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
		private Sprite _prevMouseTarget;
		private Vector2 _empty;
		private Vector2 _scale;
		private Vector2 _pivot;
		private Vector2 _mousePos;
		private Vector2[] _verts;
		private Rectangle _sourceRect;
		private MouseState _mouseState;
		private MouseState _prevMouseState;
		private SpriteBatch spriteBatch;
		private MonogameAudio _audio;
		private UpdateDelegate _onUpdate;
		private MonogameTextures _textures;
		private MouseButtonState[] _mouseButtonStates;
		private GraphicsDeviceManager graphics;
		private Dictionary<string, SpriteFont> _fonts;
		

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
			_fonts = new Dictionary<string, SpriteFont>();
			_empty = new Vector2();
			_pivot = new Vector2();
			_scale = new Vector2(1, 1);
			_mousePos = new Vector2();
			_sourceRect = new Rectangle();
			_verts = new Vector2[4];
			_audio = new MonogameAudio(Content);
			_textures = new MonogameTextures(Content);
			
			_mouseButtonStates = new MouseButtonState[]{
				new MouseButtonState(MouseButton.LEFT, MouseEvent.MOUSE_DOWN, MouseEvent.MOUSE_UP, MouseEvent.CLICK),
				new MouseButtonState(MouseButton.RIGHT, MouseEvent.RIGHT_DOWN, MouseEvent.RIGHT_UP, MouseEvent.RIGHT_CLICK),
				new MouseButtonState(MouseButton.MIDDLE, MouseEvent.MIDDLE_DOWN, MouseEvent.MIDDLE_UP, MouseEvent.MIDDLE_CLICK)
			};
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
		
		// --------------------------------------------------------------
		public TextFormat GetTextFormat(string path, ushort size)
		{
			return GetTextFormat(path, path, size);
		}
		
		// --------------------------------------------------------------
		public TextFormat GetTextFormat(string path, string name, ushort size)
		{
			name = name ?? path;
			SpriteFont font = GetFont(path, name);
			if (font == null) return null;
			return new TextFormat(name, size);
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
			return _textures.GetTexture(path, name);
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Returns a texture.
		/// </summary>
		/// <param name="name">Unique name of the texture.</param>
		/// <returns></returns>
		private Texture2D GetTexture(string name)
		{
			return _textures.GetTexture(name);
		}
		
		// --------------------------------------------------------------
		private SpriteFont GetFont(string name, string path)
		{
			SpriteFont font = null;
			if (!_fonts.TryGetValue(name, out font))
			{
				font = Content.Load<SpriteFont>(GetPath(path));
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
		private SpriteFont GetFont(string name)
		{
			SpriteFont font = null;
			_fonts.TryGetValue(name, out font);
			return font;
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
			_audio.Update();
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
			_prevMouseTarget = _mouseTarget;
			_mouseTarget = null;
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_prevMouseState = _mouseState;
			_mouseState = Mouse.GetState();
			_mousePos.X = _mouseState.Position.X;
			_mousePos.Y = _mouseState.Position.Y;
			
			int n = stage.numChildren;
			Matrix matrix = Matrix.Identity;
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(stage.GetChildAt(i), matrix);
			}
			
			UpdateMouseTargetState();
			
			base.Draw(gameTime);
		}
		
		// --------------------------------------------------------------
		private void UpdateMouseTargetState()
		{
			for ( int i = 0; i < _mouseButtonStates.Length; ++i )
			{
				ButtonState buttonState = _mouseState.LeftButton;
				ButtonState prevState = _prevMouseState.LeftButton;
				int delta = 0;

				if(_mouseButtonStates[i].button == MouseButton.RIGHT)
				{
					buttonState = _mouseState.RightButton;
					prevState = _prevMouseState.RightButton;
				}
				else if ( _mouseButtonStates[i].button == MouseButton.MIDDLE )
				{
					buttonState = _mouseState.MiddleButton;
					prevState = _prevMouseState.MiddleButton;
					delta = _mouseState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;
				}

				if(_mouseTarget == _prevMouseTarget && _mouseTarget != null) // target hasn't changed
				{
					if(buttonState != prevState) // state changed
					{
						if(buttonState == ButtonState.Pressed) {
							_mouseButtonStates[i].downTarget = _mouseTarget;
							_mouseButtonStates[i].upTarget = null;
							_mouseTarget.DispatchEvent(new MouseEvent(_mouseButtonStates[i].downEventType));
							// dispatch down event for the button here
						}
						else
						{
							_mouseButtonStates[i].upTarget = _mouseTarget;
							_mouseTarget.DispatchEvent(new MouseEvent(_mouseButtonStates[i].upEventType));
							// dispatch up event for the button here
						}

						if(_mouseButtonStates[i].downTarget == _mouseButtonStates[i].upTarget)
						{
							// dispatch click event for the button here
							// clear targets
							_mouseButtonStates[i].downTarget = null;
							_mouseButtonStates[i].upTarget = null;
							_mouseTarget.DispatchEvent(new MouseEvent(_mouseButtonStates[i].clickEventType));
						}
					}

					if (delta != 0)
					{
						_mouseTarget.DispatchEvent(new MouseEvent(MouseEvent.MOUSE_WHEEL, null, delta));
					}
				}
				else // target changed, clear states
				{
					_mouseButtonStates[i].upTarget = null;
					_mouseButtonStates[i].downTarget = null;
				}
			}
			
			if(_prevMouseTarget != _mouseTarget)
			{
				if(_prevMouseTarget != null)
				{
					// mouse out event goes here
					_prevMouseTarget.DispatchEvent( new MouseEvent(MouseEvent.MOUSE_OUT, _mouseTarget));
				}

				if(_mouseTarget != null)
				{
					// mouse over event goes here
					_mouseTarget.DispatchEvent( new MouseEvent(MouseEvent.MOUSE_OVER, _prevMouseTarget));
				}
			}

			_prevMouseTarget = null;
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
			if(!sprite.visible) return;
			
			Matrix matrix =  Matrix.CreateScale(sprite.scaleX, sprite.scaleY, 1) * Matrix.CreateRotationZ(MathHelper.ToRadians(sprite.rotation)) * Matrix.CreateTranslation(sprite.x, sprite.y, 0) * parentMatrix;

			int w = 0;
			int h = 0;

			if (sprite is TextField)
			{
				RenderText(sprite as TextField, matrix);
			}
			else if (RenderSpriteImage(sprite, matrix))
			{
				w = sprite.imageWidth;
				h = sprite.imageHeight;
			}

			if (w != 0 && h != 0 && !sprite.clickThrough)
			{
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
			
			// now, render all the children
			int n = sprite.numChildren;
			for ( int i = 0; i < n; ++i )
			{
				DrawSprite(sprite.GetChildAt(i), matrix);
			}
		}
		
		// --------------------------------------------------------------
		private bool RenderText(TextField textField, Matrix matrix)
		{
			if (!string.IsNullOrWhiteSpace(textField.text) && textField.format != null)
			{
				SpriteFont font = GetFont(textField.format.font);
				if (font != null)
				{
					string text = textField.text;
					
					if (!textField.multiline)
					{
						Regex reg = new Regex("\n|\r", RegexOptions.IgnoreCase);
						text = reg.Replace(text, " ");
					}

					ushort w = 0;
					ushort h = 0;
					if (textField.clipOverflow)
					{
						_sourceRect.X = 0;
						_sourceRect.Y = 0;
						w = textField.width;
						h = textField.height;
						_sourceRect.Width = textField.width;
						_sourceRect.Height = textField.height;
						if (textField.wordWrap && w > 0 && h > 0)
						{
							WrapText(font, text, w);
						}
					}
					else
					{
						Vector2 v = font.MeasureString(text);
						w = (ushort) v.X;
						h = (ushort) v.Y;
					}

					_pivot.X = textField.pivotX*w;
					_pivot.Y = textField.pivotY*h;

					spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, null, matrix);
					spriteBatch.DrawString(font, textField.text, _empty, Color.Black, 0, _pivot, _scale, SpriteEffects.None, 0);
					spriteBatch.End();
					return true;
				}
			}

			return false;
		}
		
		// --------------------------------------------------------------
		public string WrapText(SpriteFont spriteFont, string text, ushort maxLineWidth)
		{
			string[] words = text.Split(' ');
			StringBuilder sb = new StringBuilder();
			float lineWidth = 0f;
			float spaceWidth = spriteFont.MeasureString(" ").X;

			foreach (string word in words)
			{
				Vector2 size = spriteFont.MeasureString(word);

				if (lineWidth + size.X < maxLineWidth)
				{
					sb.Append(word + " ");
					lineWidth += size.X + spaceWidth;
				}
				else
				{
					sb.Append("\n" + word + " ");
					lineWidth = size.X + spaceWidth;
				}
			}

			return sb.ToString();
		}

		// --------------------------------------------------------------
		private bool RenderSpriteImage(Sprite sprite, Matrix matrix)
		{
			if (sprite.image != null)
			{
				Texture2D texture = GetTexture(sprite.image.name);
				if (texture != null) // we have a texture, so render it onto the screen
				{
					// set the pivot
					_pivot.X = sprite.pivotX*sprite.image.rect.width;
					_pivot.Y = sprite.pivotY*sprite.image.rect.height;

					// grab the source rect from the texture
					_sourceRect.X = sprite.image.rect.x;
					_sourceRect.Y = sprite.image.rect.y;
					_sourceRect.Width = sprite.imageWidth;
					_sourceRect.Height = sprite.imageHeight;

					// setup the render with the appropriate matrix and draw it
					spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, null, matrix);
					spriteBatch.Draw(texture, _empty, _sourceRect, Color.White, 0, _pivot, 1, SpriteEffects.None, 0);
					spriteBatch.End();

					return true;
				}
			}

			return false;
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
