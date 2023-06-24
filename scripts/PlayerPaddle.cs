using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame_project;
public class PlayerPaddle : Entity
{
    public Vector2 Position = new Vector2(0, 0);
    private static Texture2D _paddleTex;
    
    public override void LoadContent()
    {
        base.LoadContent();
        _paddleTex ??= Game1.ContentManager.Load<Texture2D>("Textures/Paddle");
    }
    
    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_paddleTex, new Vector2(5, Game1.ScreenSize.Y / 2f - _paddleTex.Height / 2f), Color.White);
    }
}