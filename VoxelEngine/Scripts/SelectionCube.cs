using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Monogame3dTest.Systems._3D;

namespace VoxelEngine.Scripts;

public class SelectionCube
{
    public Vector3 Position = Vector3.Zero;
    private ModelBasic _model;

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        _model = new ModelBasic(graphicsDevice, contentManager, "Graphics/Fbx_SelectionCube", diffuseColor: Color.White);
    }

    public void Update(GameTime gameTime)
    {
        _model.WorldMatrix = Matrix.CreateScale(1.1f) * Matrix.CreateTranslation(Position);
    }

    public void Draw()
    {
        _model.Draw(Game1.ViewMatrix, Game1.ProjectionMatrix);
    }
}