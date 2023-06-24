using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;
public class Ball : Entity
{
    public Vector2 Position = new Vector2(0, 0);
    private static Texture2D _ballTex;

    public override void LoadContent()
    {
        base.LoadContent();
        _ballTex ??= Game1.ContentManager.Load<Texture2D>("Textures/FuckedMiyamoto");

        Position = Game1.ScreenSize / 2f;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Vector2 directionInput;
        directionInput.Y =  (Keyboard.GetState().IsKeyDown(Keys.Up) ? -1 : 0) +
                            (Keyboard.GetState().IsKeyDown(Keys.Down) ? 1 : 0);
        directionInput.X =  (Keyboard.GetState().IsKeyDown(Keys.Left) ? -1 : 0) +
                            (Keyboard.GetState().IsKeyDown(Keys.Right) ? 1 : 0);
        if (directionInput.LengthSquared() > 0)
        {
            directionInput /= directionInput.Length();
            Position += directionInput * 80 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        const float size = 20;
        spriteBatch.Draw(_ballTex, new Rectangle((int)(Position.X - size / 2) , (int)(Position.Y - size / 2), (int)size, (int)size), Color.White);
    }
}

