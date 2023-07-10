using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;
using Monogame3dTest.Scripts;
using Monogame3dTest.Scripts.Systems;
using Monogame3dTest.Systems._3D;

public class Game1 : Game
{
    public static Matrix ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 3, 5), Vector3.Zero + Vector3.Up, Vector3.Up);
    public static Matrix ProjectionMatrix;
    // private Matrix _projection = Matrix.CreateOrthographic(8 * 2f, 4.8f * 2f, 0.1f, 100f);
    private Texture2D _texture;

    private ModelBasic _playerModel;
    private ModelBasic _cubeModel;

    private readonly List<Entity> _entities = new List<Entity>();
    private ModelBasic _waterPlane;

    public Effect SimpleEffect;

    SpriteBatch _spriteBatch;

    public static Tweener Tweener = new Tweener();

    public Game1()
    {
        Globals.Game = this;
        Globals.Graphics = new GraphicsDeviceManager(this);
        Globals.Content = Content;
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();

        GameScreen.Initialize(new Vector2(640, 360));
        Globals.Graphics.PreferMultiSampling = true;
        Globals.Graphics.HardwareModeSwitch = false;
        Globals.Graphics.SynchronizeWithVerticalRetrace = false;
        Globals.Graphics.IsFullScreen = true;
        IsFixedTimeStep = true;
        IsMouseVisible = true;
        Globals.Graphics.ApplyChanges();

        Window.AllowUserResizing = true;
        
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)GameScreen.RenderResolution.X / (float)GameScreen.RenderResolution.Y, 0.1f, 100f);
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        SimpleEffect = Content.Load<Effect>("Shaders/SimpleEffect");
        SimpleEffect.Parameters["_GlobalAmbientBrightness"].SetValue(1.0f);

        _playerModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_TurnipKid", "Graphics/Fbx_TurnipKid_Material_BaseMap");

        Random rand = new Random();
        int halfRange = 1;
        for (int x = -halfRange; x <= halfRange; x++)
        {
            for (int y = -halfRange; y <= halfRange; y++)
            {
                MakeGroundBlock(x, y);
            }
        }
        _cubeModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_SelectionCube", useIncludedTexture: false, isUnlit: true);
        _waterPlane = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_WaterPlane");
    }

    private GroundBlock MakeGroundBlock(float x, float y, bool isGate = false)
    {
        Random rand = new Random();
        int blockTexIndex = (int)(MathF.Abs((x + y) % 2)) + 1;
        GroundBlock groundBlock = new GroundBlock(new Vector3(x, 0, y), blockTexIndex, isGate);
        groundBlock.LoadContent();

        _entities.Add(groundBlock);

        return groundBlock;
    }

    private MouseState _mouseStateLast;
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Add your update logic here
        _playerModel.WorldMatrix = Matrix.CreateRotationY(MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 4) * 0.2f) * Matrix.CreateTranslation(0, 0.5f, 0);
        
        // Level editing cursor
        Vector3 mousePlanePos = ScreenToPlanePos(Mouse.GetState().Position.ToVector2(), 0.5f);
        var snappedMousePosScene = new Vector3(MathF.Round(mousePlanePos.X), 0f, MathF.Round(mousePlanePos.Z));
        var cubePosCurrent = _cubeModel.WorldMatrix.Translation;
        var cubePosLerp = Vector3.Lerp(cubePosCurrent, snappedMousePosScene,
            (float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
        _cubeModel.WorldMatrix = Matrix.CreateScale(1.1f) * Matrix.CreateTranslation(cubePosLerp);
        
        // Level editing creation tool basics
        if (Mouse.GetState().LeftButton == ButtonState.Pressed && _mouseStateLast.LeftButton == ButtonState.Released)
        {
            MakeGroundBlock(snappedMousePosScene.X, snappedMousePosScene.Z).ScaleAppear();
        }
        if (Mouse.GetState().RightButton == ButtonState.Pressed && _mouseStateLast.RightButton == ButtonState.Released)
        {
            MakeGroundBlock(snappedMousePosScene.X, snappedMousePosScene.Z, true).ScaleAppear();
        }
        if (Keyboard.GetState().IsKeyDown(Keys.B))
            _playerModel.SetBrightness(0f);

        // Update the camera
        ViewMatrix = Matrix.CreateTranslation(Matrix.Invert(_playerModel.WorldMatrix).Translation) * 
                // Matrix.CreateFromAxisAngle(Vector3.Up, (float)(MathF.PI / 4 + gameTime.TotalGameTime.TotalSeconds * 0.5f)) * 
                Matrix.CreateFromAxisAngle(Vector3.Right, MathF.PI / 4) * 
                Matrix.CreateTranslation(0f, 0f, -13f);
        // _view = Matrix.CreateLookAt(new Vector3(0, 3, 5), Vector3.Zero + Vector3.Up, Vector3.Up);

        // Update all entities
        foreach (Entity entity in _entities)
        {
            entity.Update(gameTime);
        }
        
        // Update Tweener
        Tweener.Update(gameTime.GetElapsedSeconds());
        
        _mouseStateLast = Mouse.GetState();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // GameScreen.StartRendering();
        GraphicsDevice.Clear(Color.DarkSlateBlue);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        
        // _waterPlane.Draw(_view, _projection);
        _playerModel.Draw(ViewMatrix, ProjectionMatrix);
        foreach (Entity entity in _entities)
        {
            entity.Draw(gameTime, _spriteBatch);
        }
        
        // Selection Cube
        // var state = new DepthStencilState();
        // state.DepthBufferFunction = CompareFunction.Greater;
        // state.StencilEnable = true;
        // GraphicsDevice.DepthStencilState = state;
        // cubeModel.Draw(_view, _projection, tint: Color.Gray);

        _cubeModel.Draw(ViewMatrix, ProjectionMatrix);
        CustomDraw.DrawGrid(GraphicsDevice, 15, 1, Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(-7.5f, 0.5f, -7.5f));
        _spriteBatch.End();

        // GameScreen.EndRendering();
        // GameScreen.DrawRenderBufferToScreen(_spriteBatch);
        
        base.Draw(gameTime);
    }

    public Vector3 ScreenToPlanePos(Vector2 screenPos, float desiredY = 0)
    {
        Vector3 nearScreenPoint = new Vector3(screenPos.X, screenPos.Y, 0);
        Vector3 farScreenPoint = new Vector3(screenPos.X, screenPos.Y, 1);
        Vector3 nearWorldPoint = GraphicsDevice.Viewport.Unproject(nearScreenPoint, ProjectionMatrix, ViewMatrix, Matrix.Identity);
        Vector3 farWorldPoint = GraphicsDevice.Viewport.Unproject(farScreenPoint, ProjectionMatrix, ViewMatrix, Matrix.Identity);

        Vector3 direction = farWorldPoint - nearWorldPoint;

        float yFactor = (desiredY - nearWorldPoint.Y) / direction.Y;
        Vector3 desiredWorldPoint = nearWorldPoint + direction * yFactor;

        return desiredWorldPoint;
    }

}