using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Ball : Entity
{
    public Vector2 Position = new Vector2(0, 0);
    private Texture2D ballTex;


    public override void LoadContent()
    {
        base.LoadContent();
        // ballTex = Content.Load<Texture2D>("Textures/FuckedMiyamoto");
    }
    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        float size = 20;
        spriteBatch.Draw(ballTex, new Rectangle((int)(Position.X - size / 2) , (int)(Position.Y - size / 2), (int)size, (int)size), Color.White);
    }
}

