using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame_project;
public class Ball : Entity
{
    private static Texture2D _ballTex;
    
    public BoundingBox BoundingBox;

    public Vector2 Direction = Vector2.UnitY;
    
    public override void LoadContent()
    {
        base.LoadContent();
        _ballTex ??= Game1.ContentManager.Load<Texture2D>("Textures/FuckedMiyamoto");

        // Position = Game1.ScreenSize / 2f;
        Position = new Vector2(240, 160) / 2f;
        
        BoundingBox = new BoundingBox(this, Vector2.One * -4f, Vector2.One * 8);
    }

    public override void Update(GameTime gameTime)
    {
        Vector2 directionInput = Vector2.Normalize(Direction);

        if (directionInput.LengthSquared() > 0)
        {
            directionInput /= directionInput.Length();
            Position += directionInput * 60 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        const float size = 10;
        spriteBatch.Draw(_ballTex, Position, null, Color.White, 0f, Vector2.One * _ballTex.Height / 2f,
            size / _ballTex.Height, SpriteEffects.None, 0);
        BoundingBox.Draw(spriteBatch);
    }
}