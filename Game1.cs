using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _sneed;
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
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _sneed = Content.Load<Texture2D>("Textures/FuckedMiyamoto");

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        GraphicsDevice.SetRenderTarget(_mainRenderTarget);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_sneed, new Rectangle(100, 50, 20, 20), Color.White);
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_mainRenderTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
