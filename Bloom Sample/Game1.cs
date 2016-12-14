using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bloom_Sample
{
    /// <summary>
    /// This is a sample program to showcase the bloom filter
    /// 
    /// To implement this filter in your game you need the Bloomfilter.cs file 
    /// and the Bloom.fx shader file, 
    /// found in Content/Shaders/BloomFilter/Bloom.fx
    /// 
    /// Ideally you place this file in the same location relative to your root, but you can adjust the directory in BloomFilter.cs->Load()
    /// 
    /// You need to initialize a BloomFilter class and call Load(...) to make it load the shaders
    /// 
    /// You can largely follow the render order presented in this file -> Draw()
    /// 
    /// 
    /// TheKosmonaut, 2016
    /// </summary>
    public class Game1 : Game
    {
        public readonly GraphicsDeviceManager Graphics;
        SpriteBatch _spriteBatch;
        private int _width = 1280;
        private int _height = 800;
        private bool _halfRes = false;
        private float _avgFps = 0;

        private bool _isActiveWindow = true;

        private Texture2D _sampleImage;

        private SpriteFont _defaultSpriteFont;
        private StringBuilder _infoString;

        private BloomFilter _bloomFilter;


        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Graphics.PreferredBackBufferWidth = _width;
            Graphics.PreferredBackBufferHeight = _height;

            //Frametime not limited to 16.66 Hz / 60 FPS
            IsFixedTimeStep = false;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            IsMouseVisible = true;

            //Active window?
            Activated += IsActivated;
            Deactivated += IsDeactivated;
        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _sampleImage = Content.Load<Texture2D>("sample");
            _defaultSpriteFont = Content.Load<SpriteFont>("arial");
            _infoString = new StringBuilder("Use F1 - F8 for settings and left mouse button + drag for bloom threshold");

            //Load our Bloomfilter!
            _bloomFilter = new BloomFilter();
            _bloomFilter.Load(GraphicsDevice, Content, _width, _height);


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _bloomFilter.Dispose();
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!_isActiveWindow) return;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            KeyboardState state = Keyboard.GetState();
            MouseState mstate = Mouse.GetState();

            if (state.IsKeyDown(Keys.F1)) _bloomFilter.BloomPreset = BloomFilter.BloomPresets.Wide;
            if (state.IsKeyDown(Keys.F2)) _bloomFilter.BloomPreset = BloomFilter.BloomPresets.SuperWide;
            if (state.IsKeyDown(Keys.F3)) _bloomFilter.BloomPreset = BloomFilter.BloomPresets.Focussed;
            if (state.IsKeyDown(Keys.F4)) _bloomFilter.BloomPreset = BloomFilter.BloomPresets.Small;
            if (state.IsKeyDown(Keys.F5)) _bloomFilter.BloomPreset = BloomFilter.BloomPresets.Cheap;

            if (state.IsKeyDown(Keys.F9)) _bloomFilter.BloomStreakLength = 1;
            if (state.IsKeyDown(Keys.F10)) _bloomFilter.BloomStreakLength = 2;

            if (state.IsKeyDown(Keys.F7)) _halfRes = true;
            if (state.IsKeyDown(Keys.F8)) _halfRes = false;

            if (mstate.LeftButton == ButtonState.Pressed)
            {
                float x = (float) mstate.X/Window.ClientBounds.Width;
                _bloomFilter.BloomThreshold = x;
            }
            
            float fps = (float) Math.Round(1000 / gameTime.ElapsedGameTime.TotalMilliseconds, 1);

            //Set _avgFPS to the first fps value when started.
            if (_avgFps < 0.01f) _avgFps = fps;

            //Average over 20 frames
            _avgFps = _avgFps*0.95f + fps*0.05f;

            Window.Title = "BloomFilter Preset: " + _bloomFilter.BloomPreset +
                           " with " + _bloomFilter.BloomDownsamplePasses +
                           " Passes | Threshold: " + Math.Round(_bloomFilter.BloomThreshold, 2) +
                           " | Half res: " + _halfRes +
                           " | Streaks: " + _bloomFilter.BloomStreakLength +
                           " | FPS : " + _avgFps;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!_isActiveWindow) return;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int w = _width;
            int h = _height;

            //For performance reasons we might want to calculate the blur in half resolution (or lower!)
            if (_halfRes)
            {
                w /= 2;
                h /= 2;
            }

            //Default 
            //_bloomFilter.BloomUseLuminance = true;

            Texture2D bloom = _bloomFilter.Draw(_sampleImage, w, h);
            
            GraphicsDevice.SetRenderTarget(null);

            //Use additive blending
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            _spriteBatch.Draw(_sampleImage, new Rectangle(0, 0, _width, _height), Color.White);
            _spriteBatch.Draw(bloom, new Rectangle(0,0,_width,_height),Color.White);

            _spriteBatch.DrawString(_defaultSpriteFont, _infoString, Vector2.One, Color.White );
            
            _spriteBatch.End();


            base.Draw(gameTime);
        }


        private void IsDeactivated(object sender, EventArgs e)
        {
            _isActiveWindow = false;
        }

        private void IsActivated(object sender, EventArgs e)
        {
            _isActiveWindow = true;
        }
    }
}
