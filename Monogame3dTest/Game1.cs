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

    private ModelBasic playerModel;
    private ModelBasic cubeModel;

    private List<ModelBasic> groundTestModels = new List<ModelBasic>();
    
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
        _graphics.ApplyChanges();
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        playerModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_TurnipKid");

        Random rand = new Random();
        int halfRange = 3;
        for (int x = -halfRange; x <= halfRange; x++)
        {
            for (int y = -halfRange; y <= halfRange; y++)
            {
                ModelBasic groundModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_CubeTest");

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
                groundTestModels.Add(groundModel);
            }
        }
        cubeModel = new ModelBasic(GraphicsDevice, Content, "Graphics/Fbx_CubeTest", "Graphics/Fbx_TurnipKid_Material_BaseMap");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Add your update logic here
        playerModel.WorldMatrix = Matrix.CreateRotationY(MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 4) * 0.2f) * Matrix.CreateTranslation(0, 0.5f, 0);

        _view = Matrix.CreateTranslation(Matrix.Invert(playerModel.WorldMatrix).Translation) * 
                Matrix.CreateFromAxisAngle(Vector3.Up, (float)(MathF.PI / 4 + gameTime.TotalGameTime.TotalSeconds * 0.5f)) * 
                Matrix.CreateFromAxisAngle(Vector3.Right, MathF.PI / 6) * 
                Matrix.CreateTranslation(0f, 0f, -13f);
        // _view = Matrix.CreateLookAt(new Vector3(0, 3, 5), Vector3.Zero + Vector3.Up, Vector3.Up);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        playerModel.Draw(_view, _projection);
        // cubeModel.Draw(_view, _projection);
        foreach (ModelBasic groundTestModel in groundTestModels)
        {
            groundTestModel.Draw(_view, _projection);
        }
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}