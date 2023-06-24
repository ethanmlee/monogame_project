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
    public static Vector2 RenderResolution = new Vector2(240 * 4, 160 * 4);
    private RenderTarget2D _mainRenderTarget;

    private readonly PlayerPaddle _playerPaddle = new PlayerPaddle();
    private readonly Ball _ball = new Ball();

    public static ContentManager ContentManager;

    private readonly Camera _mainCamera = new Camera(Vector2.Zero, 0f, 160);

    public Game1()
    {
        ContentManager = Content;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1f/60f);
        IsFixedTimeStep = true;
        _graphics.SynchronizeWithVerticalRetrace = false;
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
        _playerPaddle.LoadContent();
        _ball.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        // Update scene objects
        _playerPaddle.Update(gameTime);
        _ball.Update(gameTime);
        


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
        _playerPaddle.Draw(gameTime, _spriteBatch);
        _ball.Draw(gameTime, _spriteBatch);
        
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
