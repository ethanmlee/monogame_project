using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame_project;
public class Ball : Entity
{
    private static Texture2D _ballTex;
    
    public BoundingBox BoundingBox;

    public Vector2 Direction = -Vector2.UnitX;
    private float _angle = 0f;
    public float SpinSpeed = 0f;
    
    public override void LoadContent()
    {
        base.LoadContent();
        _ballTex ??= Game1.ContentManager.Load<Texture2D>("Textures/DustGuy");

        Position = new Vector2(240, 160) / 2f;
        
        BoundingBox = new BoundingBox(this, Vector2.One * -4f, Vector2.One * 8);
    }

    public override void Update(GameTime gameTime)
    {
        Vector2 directionInput = Vector2.Normalize(Direction);

        if (directionInput.LengthSquared() > 0)
        {
            // directionInput /= directionInput.Length();
            Position += directionInput * 90 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        
        if (BoundingBox.Top < 0 && Direction.Y < 0) Direction.Y *= -1;
        if (BoundingBox.Bottom > 160 && Direction.Y > 0) Direction.Y *= -1;

        _angle += MathF.PI * SpinSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        const float size = 10;
        spriteBatch.Draw(_ballTex, Position, null, Color.White, _angle, _ballTex.Bounds.Size.ToVector2() / 2f,
            1, SpriteEffects.None, 0);
        BoundingBox.Draw(spriteBatch);
    }
}