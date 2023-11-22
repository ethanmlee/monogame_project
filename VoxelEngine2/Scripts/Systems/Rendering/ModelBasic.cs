using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public bool IsUnlit { get; protected set; }
    
    public Matrix WorldMatrix { get; set; }
    
    public bool UseIncludedTexture { get; set; }
    
    private Effect _myEffect;
    public readonly EffectParameter ParamWorldMatrix;
    public readonly EffectParameter ParamViewMatrix;
    public readonly EffectParameter ParamProjectionMatrix;
    public readonly EffectParameter VertWobbleAmount; 

    public ModelBasic(GraphicsDevice graphicsDevice, ContentManager contentManager, string modelPath, string texturePath = "", bool useIncludedTexture = false, Color? diffuseColor = null, bool isUnlit = false)
    {
        _myEffect = contentManager.Load<Effect>("Shaders/SimpleEffect");
        
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
        
        foreach (var part in Model.Meshes.SelectMany(mesh => mesh.MeshParts))
        {
            // Get the "included" texture from the mesh itself
            if (UseIncludedTexture)
            {
                foreach (EffectParameter parameter in part.Effect.Parameters)
                {
                    if (parameter.Name != "Texture") continue;
                    Texture = parameter.GetValueTexture2D();
                    break;
                }
            }
                
            part.Effect = _myEffect;
        }

        DiffuseColor = diffuseColor ?? Color.White;
        WorldMatrix = Matrix.Identity;
        IsUnlit = isUnlit;
        
        // Shader parameters
        ParamWorldMatrix = _myEffect.Parameters["_WorldMatrix"];
        ParamViewMatrix = _myEffect.Parameters["_ViewMatrix"];
        ParamProjectionMatrix = _myEffect.Parameters["_ProjectionMatrix"];
        VertWobbleAmount = _myEffect.Parameters["_VertWobbleAmount"];
    }

    public void SetBrightness(float brightness)
    {
        _myEffect.Parameters["_GlobalAmbientBrightness"].SetValue(brightness);
    }

    public void Draw(Matrix viewMatrix, Matrix projectionMatrix, Color? tint = null)
    {
        Color tintUnwrapped = tint ?? Color.White;

        Model.CopyAbsoluteBoneTransformsTo(_modelTransforms);
        
        ParamViewMatrix.SetValue(viewMatrix);
        ParamProjectionMatrix.SetValue(projectionMatrix);
        VertWobbleAmount.SetValue(0.0f);
        
        foreach (ModelMesh mesh in Model.Meshes)
        {
            Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * WorldMatrix;
            
            ParamWorldMatrix.SetValue(localWorld);

            mesh.Draw();
        }
    }

    // public void DrawSimple(Matrix viewMatrix, Matrix projectionMatrix, Color? tint = null)
    // {
    //     Color tintUnwrapped = tint ?? Color.White;
    //
    //     Model.CopyAbsoluteBoneTransformsTo(_modelTransforms);
    //     foreach (ModelMesh mesh in Model.Meshes)
    //     {
    //         Matrix localWorld = _modelTransforms[mesh.ParentBone.Index] * WorldMatrix;
    //         
    //         foreach (var effect1 in mesh.Effects)
    //         {
    //             var effect = (BasicEffect)effect1;
    //             effect.DiffuseColor = DiffuseColor.ToVector3() * tintUnwrapped.ToVector3();
    //             if (Texture != null)
    //             {
    //                 effect.TextureEnabled = true;
    //                 effect.Texture = Texture;
    //             }
    //             else if (!UseIncludedTexture)
    //             {
    //                 effect.TextureEnabled = false;
    //             }
    //
    //             effect.World = localWorld;
    //             effect.View = viewMatrix;
    //             effect.Projection = projectionMatrix;
    //         }
    //         
    //         mesh.Draw();
    //     }
    // }
}
