using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D ballTex;
    private Texture2D paddleTex;
    public readonly Vector2 ScreenSize = new Vector2(240, 160);
    public Vector2 Position = new Vector2(0, 0);
    private RenderTarget2D _mainRenderTarget;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 240 * 10;
        _graphics.PreferredBackBufferHeight = 160 * 10;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _mainRenderTarget = new RenderTarget2D(GraphicsDevice, 240, 160);
        Position = ScreenSize / 2;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        ballTex = Content.Load<Texture2D>("Textures/FuckedMiyamoto");
        paddleTex = Content.Load<Texture2D>("Textures/Paddle");


        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Position.X += -80 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            Position.Y += -80 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        float size = 20;

        GraphicsDevice.SetRenderTarget(_mainRenderTarget);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(ballTex, new Rectangle((int)(Position.X - size / 2) , (int)(Position.Y - size / 2), (int)size, (int)size), Color.White);
        _spriteBatch.Draw(paddleTex, new Vector2(5, ScreenSize.Y / 2 - paddleTex.Height / 2), Color.White);
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_mainRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
