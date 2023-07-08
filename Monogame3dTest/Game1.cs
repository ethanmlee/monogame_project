using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monogame3dTest.Systems._3D;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    
    private Matrix _view = Matrix.CreateLookAt(new Vector3(0, 3, 5), Vector3.Zero + Vector3.Up, Vector3.Up);
    private Matrix _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1800f / 1440f, 0.1f, 100f);
    // private Matrix _projection = Matrix.CreateOrthographic(8 * 2f, 4.8f * 2f, 0.1f, 100f);
    private Texture2D _texture;

    private ModelBasic _playerModel;
    private ModelBasic _cubeModel;

    private readonly List<ModelBasic> _groundTestModels = new List<ModelBasic>();
    private ModelBasic _waterPlane;
    
    SpriteBatch _spriteBatch;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();

        _graphics.PreferredBackBufferWidth = 1800;
        _graphics.PreferredBackBufferHeight = 1440;
        _graphics.PreferMultiSampling = true;
        _graphics.HardwareModeSwitch = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = true;
        IsMouseVisible = true;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = true;
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        _playerModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_TurnipKid", useIncludedTexture: true);

        Random rand = new Random();
        int halfRange = 3;
        for (int x = -halfRange; x <= halfRange; x++)
        {
            for (int y = -halfRange; y <= halfRange; y++)
            {
                MakeGroundBlock(x, y);
            }
        }
        _cubeModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_SelectionCube", useIncludedTexture: false);
        _waterPlane = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_WaterPlane");
    }

    private void MakeGroundBlock(float x, float y)
    {
        Random rand = new Random();
        int blockTexIndex = (int)(MathF.Abs((x + y) % 2)) + 1;
        ModelBasic groundModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_CubeTest",
            $"Graphics/Tex_NASU_Block{blockTexIndex}");

        float offsetSize = 0.035f;
        groundModel.WorldMatrix =
            Matrix.CreateFromYawPitchRoll(
                rand.Next(-1, 2) * offsetSize,
                rand.Next(-1, 2) * offsetSize,
                rand.Next(-1, 2) * offsetSize) *
            Matrix.CreateTranslation(
                x + rand.Next(-1, 1) * offsetSize,
                rand.Next(-1, 2) * offsetSize,
                y + rand.Next(-1, 2) * offsetSize);
        _groundTestModels.Add(groundModel);
    }

    private MouseState _mouseStateLast;
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Add your update logic here
        Vector3 mousePlanePos = ScreenToPlanePos(Mouse.GetState().Position.ToVector2(), 0.5f);
        _playerModel.WorldMatrix = Matrix.CreateRotationY(MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 4) * 0.2f) * Matrix.CreateTranslation(0, 0.5f, 0);

        var cubePosCurrent = _cubeModel.WorldMatrix.Translation;
        var snappedMousePosScene = new Vector3(MathF.Round(mousePlanePos.X), 0f, MathF.Round(mousePlanePos.Z));
        var cubePosLerp = Vector3.Lerp(cubePosCurrent, snappedMousePosScene,
            (float)gameTime.ElapsedGameTime.TotalSeconds * 10f);
        _cubeModel.WorldMatrix = Matrix.CreateScale(1.1f) * Matrix.CreateTranslation(cubePosLerp);
        if (Mouse.GetState().LeftButton == ButtonState.Pressed && _mouseStateLast.LeftButton == ButtonState.Released)
        {
            MakeGroundBlock(snappedMousePosScene.X, snappedMousePosScene.Z);
        }

        _view = Matrix.CreateTranslation(Matrix.Invert(_playerModel.WorldMatrix).Translation) * 
                // Matrix.CreateFromAxisAngle(Vector3.Up, (float)(MathF.PI / 4 + gameTime.TotalGameTime.TotalSeconds * 0.5f)) * 
                Matrix.CreateFromAxisAngle(Vector3.Right, MathF.PI / 4) * 
                Matrix.CreateTranslation(0f, 0f, -13f);
        // _view = Matrix.CreateLookAt(new Vector3(0, 3, 5), Vector3.Zero + Vector3.Up, Vector3.Up);
        _mouseStateLast = Mouse.GetState();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        
        // _waterPlane.Draw(_view, _projection);
        _playerModel.Draw(_view, _projection);
        foreach (ModelBasic groundTestModel in _groundTestModels)
        {
            groundTestModel.Draw(_view, _projection);
        }
        
        // Selection Cube
        // var state = new DepthStencilState();
        // state.DepthBufferFunction = CompareFunction.Greater;
        // state.StencilEnable = true;
        // GraphicsDevice.DepthStencilState = state;
        // cubeModel.Draw(_view, _projection, tint: Color.Gray);

        _cubeModel.Draw(_view, _projection);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public Vector3 ScreenToPlanePos(Vector2 screenPos, float desiredY = 0)
    {
        Vector3 nearScreenPoint = new Vector3(screenPos.X, screenPos.Y, 0);
        Vector3 farScreenPoint = new Vector3(screenPos.X, screenPos.Y, 1);
        Vector3 nearWorldPoint = GraphicsDevice.Viewport.Unproject(nearScreenPoint, _projection, _view, Matrix.Identity);
        Vector3 farWorldPoint = GraphicsDevice.Viewport.Unproject(farScreenPoint, _projection, _view, Matrix.Identity);

        Vector3 direction = farWorldPoint - nearWorldPoint;

        float yFactor = (desiredY - nearWorldPoint.Y) / direction.Y;
        Vector3 desiredWorldPoint = nearWorldPoint + direction * yFactor;

        return desiredWorldPoint;
    }

}