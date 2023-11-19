using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine.Scripts;

public class SkySphere
{
    private Effect _effect;
    private Model _model;

    private readonly RasterizerState _rs = new RasterizerState() { CullMode = CullMode.None };
    private readonly DepthStencilState _dsDepthOnLessThan = new DepthStencilState(){DepthBufferEnable = false, DepthBufferFunction = CompareFunction.Never, DepthBufferWriteEnable = false};

    public void LoadContent(ContentManager contentManager)
    {
        _effect = contentManager.Load<Effect>("Shaders/StarrySky");
        _model = contentManager.Load<Model>("Graphics/Fbx_Sphere");

        foreach (var part in _model.Meshes.SelectMany(mesh => mesh.MeshParts))
        {
            part.Effect = _effect;
        }
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        var originalRasterizerState = graphicsDevice.RasterizerState;
        graphicsDevice.RasterizerState = _rs;
        
        var originalDepthState = graphicsDevice.DepthStencilState;
        graphicsDevice.DepthStencilState = _dsDepthOnLessThan;
        
        foreach (ModelMesh modelMesh in _model.Meshes)
        {
            _effect.Parameters["_WorldMatrix"].SetValue(Matrix.CreateScale(2) * Matrix.CreateTranslation(Game1.CamPos));
            _effect.Parameters["_ViewMatrix"].SetValue(Game1.ViewMatrix);
            _effect.Parameters["_ProjectionMatrix"].SetValue(Game1.ProjectionMatrix);
            modelMesh.Draw();
        }
        graphicsDevice.RasterizerState = originalRasterizerState;
        graphicsDevice.DepthStencilState = originalDepthState;
    }
}