using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame_project;
public class Ball : Entity
{
    public Vector2 Position = new Vector2(0, 0);
    private static Texture2D _ballTex;

    public override void LoadContent()
    {
        base.LoadContent();
        _ballTex ??= Game1.ContentManager.Load<Texture2D>("Textures/FuckedMiyamoto");

        // Position = Game1.ScreenSize / 2f;
        Position = new Vector2(240, 160) / 2f;
    }

    public override void Update(GameTime gameTime)
    {
        Vector2 directionInput = new Vector2();
        directionInput.Y =  0;
        directionInput.X =  -1f;

        if (directionInput.LengthSquared() > 0)
        {
            directionInput /= directionInput.Length();
            Position += directionInput * 60 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        const float size = 20;
        spriteBatch.Draw(_ballTex, Position, null, Color.White, 0f, Vector2.One * _ballTex.Height / 2f,
            size / _ballTex.Height, SpriteEffects.None, 0);
    }
}

