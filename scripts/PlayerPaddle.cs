using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_project;
public class PlayerPaddle : Entity
{
    public Vector2 Position;
    private static Texture2D _paddleTex;

    public int PlayerIndex = 0;
    
    public BoundingBox BoundingBox;
    
    public PlayerPaddle(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }
    
    public override void LoadContent()
    {
        base.LoadContent();
        _paddleTex ??= Game1.ContentManager.Load<Texture2D>("Textures/Paddle");

        BoundingBox = new BoundingBox(new Vector2(PlayerIndex == 0 ? 0 : 10, 0), new Vector2(6, 32));
        int playerX = (PlayerIndex == 0) ? 5 : 240 - 5 - _paddleTex.Width;
        Position = new Vector2(playerX, 80 - _paddleTex.Height / 2f);
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

        if (Position.Y < 0) Position.Y = 0;
        if (Position.Y > 160 - _paddleTex.Height) Position.Y = 160 - _paddleTex.Height;
        
        if (BoundingBox.IsIntersecting(Game1.Ball.BoundingBox))
        {
            HitBall(Game1.Ball);
        }

        BoundingBox.Position = Position;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_paddleTex, Position, null, Color.White, 0, Vector2.Zero, Vector2.One,
            PlayerIndex == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
    }
    
    public void HitBall(Ball ball)
    {
        ball.Direction = PlayerIndex == 0 ? 1 : -1;
    }
}