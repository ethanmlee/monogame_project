using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using Monogame3dTest.Systems._3D;

namespace Monogame3dTest.Scripts;

public class GroundBlock : Entity
{
    private ModelBasic _model;
    private ModelBasic _rockModel;
    private int _textureIndex;
    private bool _isGate = false;

    public GroundBlock(Vector3 pos, int textureIndex, bool isGate = false)
    {
        Transform.Position = pos;
        _textureIndex = textureIndex;
        
        float offsetSize = 0.02f;
        Random rand = new Random();
        Transform.EulerAngles = new Vector3(rand.Next(-1, 2) * offsetSize,
                                rand.Next(-1, 2) * offsetSize,
                                rand.Next(-1, 2) * offsetSize);

        _isGate = isGate;
    }

    public override void LoadContent()
    {
        _model = new ModelBasic(Globals.Graphics.GraphicsDevice, Globals.Content, !_isGate ? "Graphics/Fbx_CubeTest" : "Graphics/Fbx_GateBlock", "Graphics/Tex_BlockColors");
        // _rockModel = new ModelBasic(Globals.Graphics.GraphicsDevice, Globals.Content, "Graphics/Fbx_Rock", "Graphics/Tex_BlockColors");
        
        // _model = new ModelBasic(Globals.Graphics.GraphicsDevice, Globals.Content, "Graphics/Fbx_GateBlock", $"Graphics/Tex_BlockColors");
    }

    public override void Update(GameTime gameTime)
    {
        Matrix transformMatrix = Transform.GetTransformMatrix();
        // transformMatrix *= Matrix.CreateTranslation(0,
        //     PerlinNoiseGenerator.GenerateNoise((float)(Transform.Position.X + gameTime.TotalGameTime.TotalSeconds * 5),
        //         (float)(Transform.Position.Z + gameTime.TotalGameTime.TotalSeconds * 6.5), 8) * 0.1f, 0);
        _model.WorldMatrix = transformMatrix;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _model.Draw(Game1.ViewMatrix, Game1.ProjectionMatrix, tint: _textureIndex == 1 ? Color.White : Color.White * 0.925f);

        // _rockModel.WorldMatrix = _model.WorldMatrix * Matrix.CreateTranslation(Vector3.Up * 0.5f);
        // _rockModel.Draw(Game1.ViewMatrix, Game1.ProjectionMatrix);
        // CustomDraw.DrawQuad(Globals.Graphics.GraphicsDevice, _model.Texture, _model.WorldMatrix * Matrix.CreateTranslation(Vector3.Up), 1, 1, Vector2.Zero, _model.Texture.Bounds.Size.ToVector2());
    }

    public void ScaleAppear()
    {
        Transform.Scale = Vector3.One * 0.7f;
        Game1.Tweener.TweenTo(target: Transform, expression: t => t.Scale, toValue: Vector3.One, duration: 0.3f)
            .Easing(EasingFunctions.BackOut);
    }
}