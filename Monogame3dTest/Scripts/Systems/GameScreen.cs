using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monogame3dTest.Scripts.Systems;

public static class GameScreen
{
    public static Vector2 RenderResolution = new Vector2(100, 100);
    private static int RenderWidth => (int)RenderResolution.X;
    private static int RenderHeight => (int)RenderResolution.Y;
    private static GraphicsDevice GraphicsDevice => Globals.Graphics.GraphicsDevice;
    private static GameWindow Window => Globals.Game.Window;

    private static RenderTarget2D _mainRenderTarget;
    
    private static Effect _psxDitherEffect;

    public static void Initialize(Vector2 renderResolution)
    {
        RenderResolution = renderResolution;
        _mainRenderTarget = new RenderTarget2D(GraphicsDevice, (int)RenderResolution.X, (int)RenderResolution.Y, false, SurfaceFormat.NormalizedByte4, GraphicsDevice.PresentationParameters.DepthStencilFormat,
            GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PlatformContents);

        int maxScaleFactor = CalculateMaxScaleFactor(new Vector2(
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));
        
        Globals.Graphics.PreferredBackBufferWidth = (int)RenderResolution.X * maxScaleFactor;
        Globals.Graphics.PreferredBackBufferHeight = (int)RenderResolution.Y * maxScaleFactor;

        Globals.Graphics.ApplyChanges();
    }
    
    public static void StartRendering()
    {
        GraphicsDevice.SetRenderTarget(_mainRenderTarget);
        _psxDitherEffect = Globals.Content.Load<Effect>("Shaders/PsxDither");
    }

    public static void EndRendering()
    {
        GraphicsDevice.SetRenderTarget(null);
    }

    public static int CalculateMaxScaleFactor(Vector2 targetSize)
    {
        float targetAspectRatio = targetSize.Y / targetSize.X;
        float gameAspectRatio = (float)RenderHeight / RenderWidth;
        bool scaleByWidth = gameAspectRatio < targetAspectRatio;

        int maxScaleFactor = 1;
        if (scaleByWidth)
            maxScaleFactor = (int)MathF.Floor(targetSize.X / RenderWidth);
        else
            maxScaleFactor = (int)MathF.Floor(targetSize.Y / RenderHeight);

        return maxScaleFactor;
    }
    
    public static void DrawRenderBufferToScreen(SpriteBatch spriteBatch)
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        
        float width = Window.ClientBounds.Width;
        float height = Window.ClientBounds.Height;
        int maxScaleFactor = CalculateMaxScaleFactor(Window.ClientBounds.Size.ToVector2());

        int scaledWidth = RenderWidth * maxScaleFactor;
        int scaledHeight = RenderHeight * maxScaleFactor;
        int offsetX = (int)(width - scaledWidth) / 2;
        int offsetY = (int)(height - scaledHeight) / 2;
        Rectangle destinationRectangle =
            new Rectangle(offsetX, offsetY, scaledWidth, scaledHeight);
        
        // spriteBatch.Begin(samplerState: SamplerState.LinearWrap);
        // spriteBatch.Draw(_bgTex, Vector2.Zero, GraphicsDevice.Viewport.Bounds, Color.Gray, 0, Vector2.Zero, 3f, SpriteEffects.None, 0);
        // spriteBatch.End();
        
        Texture2D psxDitherTexture = Globals.Content.Load<Texture2D>("Graphics/Bayer8x8");
        _psxDitherEffect.Parameters["_DitherPattern"].SetValue(psxDitherTexture);
        _psxDitherEffect.Parameters["_Colors"].SetValue(32f);
        _psxDitherEffect.Parameters["_MainTex_TexelSize"].SetValue(new Vector4(1f / _mainRenderTarget.Width,
            1f / _mainRenderTarget.Height, _mainRenderTarget.Width, _mainRenderTarget.Height));
        _psxDitherEffect.Parameters["_DitherPattern_TexelSize"].SetValue(new Vector4(1f / psxDitherTexture.Width,
            1f / psxDitherTexture.Height, psxDitherTexture.Width, psxDitherTexture.Height));

        GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState: SamplerState.PointWrap, effect: _psxDitherEffect);
        spriteBatch.Draw(_mainRenderTarget, destinationRectangle, Color.White);
        spriteBatch.End();
    }
}