using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ContactProtect;
public abstract class Entity
{
    public Vector2 Position = Vector2.Zero;
    public abstract void LoadContent();
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}