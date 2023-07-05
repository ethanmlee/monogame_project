using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ContactProtect.InterfaceTest;

public interface IPlugin
{
    string Name{get;set;}
    void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}