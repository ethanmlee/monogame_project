using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogame_project;
public abstract class Entity
{
    public Vector2 Position = Vector2.Zero;
    public virtual void LoadContent() {}
    public virtual void Update(GameTime gameTime) {}
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) {}
}