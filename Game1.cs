using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    // The resolution the camera should render at, separate from the size
    public static Vector2 RenderResolution = new Vector2(240 * 4, 160 * 4);
    private RenderTarget2D _mainRenderTarget;

    public static readonly PlayerPaddle PlayerPaddle1 = new PlayerPaddle(0);
    public static readonly PlayerPaddle PlayerPaddle2 = new PlayerPaddle(1);
    public static readonly Ball Ball = new Ball();

    public static ContentManager ContentManager;

    private readonly Camera _mainCamera = new Camera(new Vector2(120, 80), 0.0f, 160);

    public Game1()
    {
        ContentManager = Content;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1f/60f);
        IsFixedTimeStep = true;
        _graphics.SynchronizeWithVerticalRetrace = false;
        
        // Sets the window size
        _graphics.PreferredBackBufferWidth = 240 * 10;
        _graphics.PreferredBackBufferHeight = 160 * 10;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _mainRenderTarget = new RenderTarget2D(GraphicsDevice, (int)RenderResolution.X, (int)RenderResolution.Y);
        _graphics.PreparingDeviceSettings += ( sender,  args) =>
        {
            _graphics.PreferMultiSampling = true;
            _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 4;
        };
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        PlayerPaddle1.LoadContent();
        PlayerPaddle2.LoadContent();
        Ball.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

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
        _spriteBatch.Begin(transformMatrix: _mainCamera.TransformationMatrix, samplerState: SamplerState.PointClamp);
        GraphicsDevice.Clear(new Color(100, 34, 125));
        
        // Draw scene objects
        PlayerPaddle1.Draw(gameTime, _spriteBatch);
        PlayerPaddle2.Draw(gameTime, _spriteBatch);
        Ball.Draw(gameTime, _spriteBatch);
        
        // End drawing
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