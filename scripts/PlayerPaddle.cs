using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;
public class PlayerPaddle : Entity
{
    public Vector2 Position;
    private static Texture2D _paddleTex;
    
    public override void LoadContent()
    {
        base.LoadContent();
        _paddleTex ??= Game1.ContentManager.Load<Texture2D>("Textures/Paddle");
        Position = new Vector2(5, 80 - _paddleTex.Height / 2f);
    }

    public override void Update(GameTime gameTime)
    {
        // Player movement
        Vector2 directionInput;
        directionInput.Y =  (Keyboard.GetState().IsKeyDown(Keys.Up) ? -1 : 0) +
                            (Keyboard.GetState().IsKeyDown(Keys.Down) ? 1 : 0);
        directionInput.X =  0;
        if (directionInput.LengthSquared() > 0)
        {
            directionInput /= directionInput.Length();
            Position += directionInput * 80 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        if (Position.Y < 0)Position.Y = 0;
        if (Position.Y > 160 - _paddleTex.Height)Position.Y = 160 - _paddleTex.Height;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_paddleTex, Position, Color.White);
    }
}