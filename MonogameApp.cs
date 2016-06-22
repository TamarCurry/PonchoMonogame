using Poncho.Display;
using Poncho.Framework;
using Poncho.Interfaces;
using Poncho.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace PonchoMonogame
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class MonogameApp : Game, IGameApp
	{
		private int _elapsed;
		private int _frames;
		private int _fpsInterval;
		private bool _started;

		private Action _onInit;
		private SpriteBatch spriteBatch;
		private MonogameAudio _audio;
		private UpdateDelegate _onUpdate;
		private MonogameFonts _fonts;
		private MonogameView _view;
		private MonogameImages _images;
		private GraphicsDeviceManager graphics;
		
		#region GETTERS
		public int fpsInterval { get { return _fpsInterval; } set { _fpsInterval = value > 0 ? value : 1000; } }
		public int fps { get; private set; }
		public int time { get; private set; }
		public int deltaTimeMs { get; private set; }
		public float deltaTime { get; private set; }
		public Stage stage { get; private set; }
		public IAppAudio audio { get { return _audio; } }
		public IAppImages images { get { return _images; } }
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
			_fonts = new MonogameFonts(Content);
			_audio = new MonogameAudio(Content);
			_images = new MonogameImages(Content);
			
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
		public TextFormat GetTextFormat(string path, ushort size)
		{
			return GetTextFormat(path, path, size);
		}
		
		// --------------------------------------------------------------
		public TextFormat GetTextFormat(string path, string name, ushort size)
		{
			name = name ?? path;
			SpriteFont font = _fonts.GetFont(path, name);
			if (font == null) return null;
			return new TextFormat(name, size);
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
			_view = new MonogameView(spriteBatch, _images, _fonts);
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
			_view.Update();
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
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_view.Update();
			base.Draw(gameTime);
		}
		
		#endregion
	}
}
