using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VoxelEngine.Scripts.Systems;

namespace VoxelEngine
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        public static Matrix ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 3, 5), Vector3.Zero + Vector3.Up, Vector3.Up);
        public static Matrix ProjectionMatrix;

        public static VoxelWorld VoxelWorld;
        public static readonly BoundingFrustum BoundingFrustum = new BoundingFrustum(Matrix.Identity);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth *= 2;
            _graphics.PreferredBackBufferHeight *= 2;
            _graphics.ApplyChanges();
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)_graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight, 0.1f, 1000f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            VoxelWorld = new VoxelWorld(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            ViewMatrix = Matrix.CreateTranslation(-(VoxelData.worldSizeInChunks.X * VoxelData.chunkSize.X) / 2f,
                             -144, -(VoxelData.worldSizeInChunks.Z * VoxelData.chunkSize.Z) / 2f) * 
                         Matrix.CreateFromAxisAngle(Vector3.Up,
                             (float)(MathF.PI / 4 + gameTime.TotalGameTime.TotalSeconds * 0.5f)) * 
                         Matrix.CreateFromAxisAngle(Vector3.Right, 0) * 
                         Matrix.CreateTranslation(0f, 0f, -0);

            BoundingFrustum.Matrix = ViewMatrix * ProjectionMatrix;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Set the desired winding order to clockwise
            RasterizerState rasterizerStateCW = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace
            };
            GraphicsDevice.RasterizerState = rasterizerStateCW;
            
            VoxelWorld.Draw(GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}