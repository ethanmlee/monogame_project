using System;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame_project.Helper_Tools;
using monogame_project.Input;
using PixelOven.Debug;
using EventInstance = FmodForFoxes.Studio.EventInstance;

namespace monogame_project;
public class Game1 : Game
{
    public static GraphicsDeviceManager Graphics;
    private SpriteBatch _spriteBatch;
    // The resolution the camera should render at, separate from the size
    public static Vector2 RenderResolution = new Vector2(240 * 4, 160 * 4);
    private RenderTarget2D _mainRenderTarget;

    private Texture2D _bgBathroomTex;
    private Texture2D _borderMockupTex;
    
    public static readonly PlayerPaddle PlayerPaddle1 = new PlayerPaddle(0);
    public static readonly PlayerPaddle PlayerPaddle2 = new PlayerPaddle(1);
    public static readonly Ball Ball = new Ball();

    public static ContentManager ContentManager;

    private readonly Camera _mainCamera = new Camera(new Vector2(120, 80), 0, 160);

    public static Rectangle OpenSpace = new Rectangle(12, 8, 216, 128);
    
    private EventInstance _audioInstance;

    public Game1()
    {
        ContentManager = Content;
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1f/60f);
        IsFixedTimeStep = true;
        Graphics.SynchronizeWithVerticalRetrace = false;
        
        // Sets the window size
        Graphics.PreferredBackBufferWidth = 240 * 10;
        Graphics.PreferredBackBufferHeight = 160 * 10;
        Graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        Window.Title = "Contact Protect";
        _mainRenderTarget = new RenderTarget2D(GraphicsDevice, (int)RenderResolution.X, (int)RenderResolution.Y);
        Graphics.PreparingDeviceSettings += ( sender,  args) =>
        {
            Graphics.PreferMultiSampling = true;
            Graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 4;
        };

        FmodController.Init();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _bgBathroomTex = ContentManager.Load<Texture2D>("Textures/BG_Bathroom");
        _borderMockupTex = ContentManager.Load<Texture2D>("Textures/BorderMockup");
        PlayerPaddle1.LoadContent();
        PlayerPaddle2.LoadContent();
        Ball.LoadContent();
        
        // FMOD
        FmodController.LoadContent();
        _audioInstance = StudioSystem.GetEvent("event:/SFX/Audio").CreateInstance();
        _audioInstance.Start();
        _audioInstance.Dispose();
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// game-specific content.
    /// </summary>
    protected override void UnloadContent()
    {
        FmodController.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        if (InputManager.KeyPressed(Keys.F3))
            DebugManager.ShowCollisionRectangles = !DebugManager.ShowCollisionRectangles;
        // FMOD
        FmodController.Update();

        // Update scene objects
        PlayerPaddle1.Update(gameTime);
        PlayerPaddle2.Update(gameTime);
        Ball.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        DrawCamera(gameTime);
        DrawRenderBufferToScreen();
        base.Draw(gameTime);
    }

    private void DrawCamera(GameTime gameTime)
    {
        // Setup drawing
        GraphicsDevice.SetRenderTarget(_mainRenderTarget);
        
        // Draw BACKGROUND
        _spriteBatch.Begin(transformMatrix: _mainCamera.TransformationMatrix, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        GraphicsDevice.Clear(Color.White);
        _spriteBatch.Draw(_bgBathroomTex, OpenSpace.Location.ToVector2(), Color.White);
        _spriteBatch.End();
        
        // Draw scene objects
        _spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, transformMatrix: _mainCamera.TransformationMatrix, samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        PlayerPaddle1.Draw(gameTime, _spriteBatch);
        PlayerPaddle2.Draw(gameTime, _spriteBatch);
        Ball.Draw(gameTime, _spriteBatch);
        // End drawing
        _spriteBatch.End();
        
        // Debug drawing (in another sprite batch so it's always on top)
        _spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, transformMatrix: _mainCamera.TransformationMatrix, samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_borderMockupTex, Vector2.Zero, Color.White);
        DebugManager.Draw(_spriteBatch);
        _spriteBatch.End();
    }

    private void DrawRenderBufferToScreen()
    {
        GraphicsDevice.SetRenderTarget(null);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_mainRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();
    }
}