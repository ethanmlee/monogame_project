using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Monogame3dTest.Systems._3D;

public class ModelBasic
{
    private Matrix[] _modelTransforms;
    public string ModelPath { get; protected set; }
    public static Dictionary<string, Model> ModelCache { get; protected set; } = new Dictionary<string, Model>();
    
    public string TexturePath { get; protected set; }
    public static Dictionary<string, Texture2D> TextureCache { get; protected set; } =
        new Dictionary<string, Texture2D>();

    public Model Model { get; protected set; }
    public Texture2D Texture { get; protected set; }
    public Color DiffuseColor { get; protected set; }
    
    public Matrix WorldMatrix { get; set; }
    
    public bool UseIncludedTexture { get; set; }

    public ModelBasic(GraphicsDevice graphicsDevice, ContentManager contentManager, string modelPath, string texturePath = "", bool useIncludedTexture = true, Color? diffuseColor = null)
    {
        // LOADING MODEL
        ModelPath = modelPath;
        if (ModelCache.ContainsKey(ModelPath))
        {
            Model = ModelCache[ModelPath];
        }
        else
        {
            Model = contentManager.Load<Model>(modelPath);
            ModelCache[ModelPath] = Model;
        }
        _modelTransforms = new Matrix[Model.Bones.Count];

        // LOADING TEXTURE
        TexturePath = texturePath;
        if (!String.IsNullOrWhiteSpace(texturePath))
        {
            if (TextureCache.ContainsKey(TexturePath))
            {
                Texture = TextureCache[TexturePath];
            }
            else
            {
                Texture = contentManager.Load<Texture2D>(texturePath);
                Debug.WriteLine($"Loaded texture at path: {texturePath}");
                TextureCache[TexturePath] = Texture;
            }
        }
        UseIncludedTexture = useIncludedTexture;

        DiffuseColor = diffuseColor ?? Color.White;

        WorldMatrix = Matrix.Identity;
    }

    public void Draw(Matrix viewMatrix, Matrix projectionMatrix, Color? tint = null)
    {
        Color tintUnwrapped = tint ?? Color.White;
        
        Model.CopyAbsoluteBoneTransformsTo(_modelTransforms);
        // GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        foreach (ModelMesh mesh in Model.Meshes)
        {
            Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * WorldMatrix;

            foreach (var effect1 in mesh.Effects)
            {
                var effect = (BasicEffect)effect1;
                effect.DiffuseColor = DiffuseColor.ToVector3() * tintUnwrapped.ToVector3();
                if (Texture != null)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = Texture;
                }
                else if (!UseIncludedTexture)
                {
                    effect.TextureEnabled = false;
                }
                effect.World = localWorld;
                effect.View = viewMatrix;
                effect.Projection = projectionMatrix;
            }
            
            mesh.Draw();
        }
    }
}
