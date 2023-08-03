using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Monogame3dTest.Systems._3D;

namespace VoxelEngine.Scripts;

public class SelectionCube
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public bool IsVisible { get; set; } = false;
    private ModelBasic Model { get; set; }

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        Model = new ModelBasic(graphicsDevice, contentManager, "Graphics/Fbx_SelectionCube", diffuseColor: Color.White);
    }

    public void Update(GameTime gameTime)
    {
        Model.WorldMatrix = Matrix.CreateScale(1.1f) * Matrix.CreateTranslation(Position);
    }

    public void Draw()
    {
        if (!IsVisible) return;
        Model.Draw(Game1.ViewMatrix, Game1.ProjectionMatrix);
    }
}