using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;
public class PlayerPaddle : Entity
{
    private static Texture2D _paddleTex;
    public readonly int PlayerIndex = 0;
    public BoundingBox BoundingBox;
    private const int DistanceFromSide = 8;
    
    public PlayerPaddle(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }
    
    public override void LoadContent()
    {
        base.LoadContent();
        _paddleTex ??= Game1.ContentManager.Load<Texture2D>("Textures/Paddle");

        BoundingBox = new BoundingBox(this, new Vector2(PlayerIndex == 0 ? 0 : 10, 0), new Vector2(6, 32));
        if (PlayerIndex == 0) BoundingBox.Left = Game1.OpenSpace.Left + DistanceFromSide;
        if (PlayerIndex == 1) BoundingBox.Right = Game1.OpenSpace.Right - DistanceFromSide;
        BoundingBox.CenterY = Game1.OpenSpace.Center.Y;
    }

    public override void Update(GameTime gameTime)
    {
        // Player movement
        Vector2 directionInput;
        directionInput.Y =  (Keyboard.GetState().IsKeyDown(PlayerIndex == 0 ? Keys.W : Keys.Up) ? -1 : 0) +
                            (Keyboard.GetState().IsKeyDown(PlayerIndex == 0 ? Keys.S : Keys.Down) ? 1 : 0);
        directionInput.X =  0;
        if (directionInput.LengthSquared() > 0)
        {
            directionInput /= directionInput.Length();
            Position += directionInput * 80 * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        if (BoundingBox.Top < Game1.OpenSpace.Top) BoundingBox.Top = Game1.OpenSpace.Top;
        if (BoundingBox.Bottom > Game1.OpenSpace.Bottom) BoundingBox.Bottom = Game1.OpenSpace.Bottom;
        
        if (BoundingBox.IsIntersecting(Game1.Ball.BoundingBox))
        {
            HitBall(Game1.Ball);
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_paddleTex, Position, null, Color.White, 0, Vector2.Zero, Vector2.One,
            PlayerIndex == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        BoundingBox.Draw(spriteBatch);
    }
    
    private void HitBall(Ball ball)
    {
        // ball.Direction = PlayerIndex == 0 ? 1 : -1;
        Vector2 centerPaddle = Position + (Vector2.UnitY * 16) + (Vector2.UnitX * (PlayerIndex == 0 ? 0 : 15));
        Vector2 hitDir = ball.Position - centerPaddle;
        ball.Direction = hitDir;
        ball.SpinSpeed = MathF.Abs(hitDir.Y) * 0.2f * MathF.Sign(hitDir.X);
        Debug.WriteLine("HIT");
    }
}