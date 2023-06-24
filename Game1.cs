using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public static Vector2 ScreenSize = new Vector2(240, 160);
    private RenderTarget2D _mainRenderTarget;

    private PlayerPaddle PlayerPaddle = new PlayerPaddle();
    private Ball Ball = new Ball();

    public static ContentManager ContentManager;
    
    public Game1()
    {
        ContentManager = Content;
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
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Vector2 directionInput;
        // directionInput.Y =  (Keyboard.GetState().IsKeyDown(Keys.Up) ? -1 : 0) +
        //                     (Keyboard.GetState().IsKeyDown(Keys.Down) ? 1 : 0);
        // directionInput.X =  (Keyboard.GetState().IsKeyDown(Keys.Left) ? -1 : 0) +
        //                     (Keyboard.GetState().IsKeyDown(Keys.Right) ? 1 : 0);
        // if (directionInput.LengthSquared() > 0)
        // {
        //     directionInput /= directionInput.Length();
        //     Position += directionInput * 80 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        // }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        GraphicsDevice.SetRenderTarget(_mainRenderTarget);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_mainRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
