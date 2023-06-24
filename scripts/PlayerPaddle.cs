using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using monogame_project;

public class PlayerPaddle : Entity
{
    public Vector2 Position = new Vector2(0, 0);
    private Texture2D paddleTex;

    
    public override void LoadContent()
    {
        base.LoadContent();
        paddleTex = Game1.ContentManager.Load<Texture2D>("Textures/Paddle");
    }
    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(paddleTex, new Vector2(5, Game1.ScreenSize.Y / 2 - paddleTex.Height / 2), Color.White);
    }
}