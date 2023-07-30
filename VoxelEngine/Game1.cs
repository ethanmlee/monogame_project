using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tweening;
using VoxelEngine.Scripts;
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

        public static Tweener Tweener = new Tweener();

        public static Vector3 CamPos = new Vector3((VoxelData.worldSizeInChunks.X * VoxelData.chunkSize.X) / 2f,
            VoxelData.worldSizeInChunks.Y * VoxelData.chunkSize.Y + 2,
            (VoxelData.worldSizeInChunks.Z * VoxelData.chunkSize.Z) / 2f);
        public static Vector3 CamEulerAngles = new Vector3(0, 0, 0);

        private Point _startMousePoint => new Point(Window.ClientBounds.Size.X / 2, Window.ClientBounds.Size.Y / 2);

        public static ContentManager ContentManager;

        public bool ShowDebugOverlay = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            ContentManager = Content;
            IsMouseVisible = true;

            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth *= 2;
            _graphics.PreferredBackBufferHeight *= 2;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)_graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight, 0.1f, 1000f);

            MouseExtended.SetPosition(_startMousePoint);
            IsMouseVisible = false;
            
            Randomizer.Range(0, 1);
            
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
            IsMouseVisible = !IsActive;

            var keyboardState = KeyboardExtended.GetState();
            var mouseState = MouseExtended.GetState();

            if (keyboardState.WasKeyJustDown(Keys.F3)) ShowDebugOverlay = !ShowDebugOverlay;
            if (keyboardState.WasKeyJustDown(Keys.G))
            {
                foreach (Chunk chunk in VoxelWorld.Chunks.Values)
                {
                    Task.Factory.StartNew((() =>
                    {
                        chunk.CreateMesh();
                        // Game1.Tweener.TweenTo(chunk, expression: chunk => chunk._visibilityScale, toValue: 1f, 0.5f)
                        //     .Easing(EasingFunctions.SineOut);
                    }));
                }
            }

            // Rotation
            var mouseDelta = mouseState.Position - _startMousePoint;
            CamEulerAngles.X += -mouseDelta.Y * 0.001f;
            CamEulerAngles.Y += -mouseDelta.X * 0.001f;
            Matrix rotationMatrix = Matrix.CreateRotationX(CamEulerAngles.X) * Matrix.CreateRotationY(CamEulerAngles.Y);
            if (IsActive) MouseExtended.SetPosition(_startMousePoint);

            // Flying Position
            var moveInput = new Vector3((keyboardState.IsKeyDown(Keys.D) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.A) ? 1 : 0), (keyboardState.IsKeyDown(Keys.Space) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.LeftShift) ? 1 : 0), (keyboardState.IsKeyDown(Keys.W) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.S) ? 1 : 0));
            Vector3 movementRelative = rotationMatrix.Forward * moveInput.Z + rotationMatrix.Right * moveInput.X + rotationMatrix.Up * moveInput.Y;
            if (movementRelative.Length() > 0) movementRelative = Vector3.Normalize(movementRelative) * movementRelative.Length();
            CamPos += movementRelative * (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
            
            ViewMatrix = Matrix.CreateLookAt(CamPos, CamPos + Vector3.Transform(Vector3.Forward, rotationMatrix), Vector3.Up);

            BoundingFrustum.Matrix = ViewMatrix * ProjectionMatrix;
            
            if (mouseState.WasButtonJustDown(MouseButton.Left))
            {
                Vector3 voxelAddPos = CamPos + rotationMatrix.Forward * 6;
                ChunkCoord coord = VoxelWorld.GetChunkCoord(voxelAddPos);
                if (VoxelWorld.IsChunkInWorld(coord))
                {
                    Chunk chunk = VoxelWorld.GetChunk(coord);
                    chunk.SetVoxel(new Vector3Int(voxelAddPos) % VoxelData.chunkSize, 1);
                }
            }
            
            VoxelWorld.Update();
            Tweener.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Set the desired winding order to clockwise
            RasterizerState rasterizerStateCw = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace
            };
            GraphicsDevice.RasterizerState = rasterizerStateCw;
            
            VoxelWorld.Draw(GraphicsDevice);

            if (ShowDebugOverlay)
            {
                Vector3 nearestChunkFloorPlane = Vector3.Floor(CamPos / VoxelData.chunkSize.X) * VoxelData.chunkSize.X;
                CustomDraw.DrawGrid(GraphicsDevice, VoxelData.chunkSize.X, 1,
                    Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(nearestChunkFloorPlane));
            }

            base.Draw(gameTime);
        }
    }
}