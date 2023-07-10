using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Monogame3dTest.Systems._3D;

public class GameModel : ModelBasic
{
    public GameModel(GraphicsDevice graphicsDevice, ContentManager contentManager, string modelPath, string texturePath = "", bool useIncludedTexture = false, Color? diffuseColor = null) : base(graphicsDevice, contentManager, modelPath, texturePath, useIncludedTexture, diffuseColor)
    {
        
    }
}