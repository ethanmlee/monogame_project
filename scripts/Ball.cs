using System;
using System.Collections.Generic;
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

    private struct SpriteState
    {
        public Vector2 Position;
        public float Angle;

        public SpriteState(Vector2 position, float angle)
        {
            Position = position;
            Angle = angle;
        }
    }
    private Queue<SpriteState> _spriteStates = new Queue<SpriteState>();
    private float _spriteStateTimer = 0f;

    public override void LoadContent()
    {
        _ballTex ??= Game1.ContentManager.Load<Texture2D>("Textures/IzzeCan");

        Position = new Vector2(240, 160) / 2f;
        
        BoundingBox = new BoundingBox(this, Vector2.One * -4f, Vector2.One * 8);
        
        BoundingBox.Center = Game1.OpenSpace.Center.ToVector2();
    }

    public override void Update(GameTime gameTime)
    {
        Vector2 directionInput = Vector2.Normalize(Direction);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds * Game1.GameAudioSpeedMod;
        
        if (directionInput.LengthSquared() > 0)
        {
            Position += directionInput * 90 * deltaTime;
        }
        
        if (BoundingBox.Top < Game1.OpenSpace.Top && Direction.Y < 0) Direction.Y *= -1;
        if (BoundingBox.Bottom > Game1.OpenSpace.Bottom && Direction.Y > 0) Direction.Y *= -1;

        _angle += MathF.PI * SpinSpeed * deltaTime;

        _spriteStateTimer += deltaTime;
        if (_spriteStateTimer > 0.05f)
        {
            _spriteStateTimer %= 0.05f;
            _spriteStates.Enqueue(new SpriteState(Position, _angle));
            if (_spriteStates.Count > 5)
                _spriteStates.Dequeue();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        const float size = 10;

        // for (int i = 0; i < _spriteStates.Count; i++)
        // {
        //     SpriteState spriteState = _spriteStates.ToArray()[i];
        //     float alpha = i / 15f;
        //     Color color = Color.White * alpha;
        //     spriteBatch.Draw(_ballTex, spriteState.Position, null, color, spriteState.Angle, _ballTex.Bounds.Size.ToVector2() / 2f,
        //         0.025f, SpriteEffects.None, 0.1f);
        // }
        
        spriteBatch.Draw(_ballTex, Position + Vector2.One, null, Color.Black, _angle, _ballTex.Bounds.Size.ToVector2() / 2f,
            0.025f, SpriteEffects.None, 0);
        spriteBatch.Draw(_ballTex, Position, null, Color.White, _angle, _ballTex.Bounds.Size.ToVector2() / 2f,
            0.025f, SpriteEffects.None, 0);
        BoundingBox.Draw(spriteBatch);
    }
}