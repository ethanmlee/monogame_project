using System;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tweening;
using VoxelEngine.Scripts;
using VoxelEngine.Scripts.Systems;
using MonoGame.ImGui;

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

        private SelectionCube _selectionCube = new SelectionCube();
        private SkySphere _skySphere = new SkySphere();

        public ImGuiRenderer ImGuiRenderer;

        private int _brushSize = 1;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            ContentManager = Content;
            IsMouseVisible = false;

            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth *= 4;
            _graphics.PreferredBackBufferHeight *= 4;
            _graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = true;
            _graphics.ApplyChanges();
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), (float)_graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight, 0.1f, 1000f);

            MouseExtended.SetPosition(_startMousePoint);
            IsMouseVisible = false;

            ImGuiRenderer = new ImGuiRenderer(this).Initialize().RebuildFontAtlas();
            ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _selectionCube.LoadContent(GraphicsDevice, ContentManager);
            _skySphere.LoadContent(ContentManager);

            VoxelWorld = new VoxelWorld(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = KeyboardExtended.GetState();
            var mouseState = MouseExtended.GetState();
            IsMouseVisible = !IsActive || ShowDebugOverlay;

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
            if (IsMouseVisible) mouseDelta = Point.Zero;
            if (mouseDelta.ToVector2().LengthSquared() > 1)
            {
                CamEulerAngles.X += -mouseDelta.Y * 0.001f;
                CamEulerAngles.Y += -mouseDelta.X * 0.001f;
            }
            Matrix rotationMatrix = Matrix.CreateRotationX(CamEulerAngles.X) * Matrix.CreateRotationY(CamEulerAngles.Y);
            if (!IsMouseVisible) MouseExtended.SetPosition(_startMousePoint);

            // Flying Position
            var moveInput = new Vector3((keyboardState.IsKeyDown(Keys.D) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.A) ? 1 : 0), (keyboardState.IsKeyDown(Keys.Space) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.LeftShift) ? 1 : 0), (keyboardState.IsKeyDown(Keys.W) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.S) ? 1 : 0));
            Vector3 movementRelative = rotationMatrix.Forward * moveInput.Z + rotationMatrix.Right * moveInput.X + rotationMatrix.Up * moveInput.Y;
            if (movementRelative.Length() > 0) movementRelative = Vector3.Normalize(movementRelative) * movementRelative.Length();
            CamPos += movementRelative * (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
            
            ViewMatrix = Matrix.CreateLookAt(CamPos, CamPos + Vector3.Transform(Vector3.Forward, rotationMatrix), Vector3.Up);

            BoundingFrustum.Matrix = ViewMatrix * ProjectionMatrix;

            Vector3 voxelAddPos = Vector3.Round(CamPos + rotationMatrix.Forward * 6 - Vector3.One / 2) + Vector3.One / 2;
            if (mouseState.WasButtonJustDown(MouseButton.Right))
            {
                VoxelWorld.SetVoxel(voxelAddPos, 1);
            }
            if (mouseState.WasButtonJustDown(MouseButton.Left))
            {
                VoxelWorld.SetVoxel(voxelAddPos, 0);
            }
            _selectionCube.Position = Vector3.Lerp(_selectionCube.Position, voxelAddPos,
                25f * (float)gameTime.ElapsedGameTime.TotalSeconds);
            _selectionCube.Update(gameTime);
            
            VoxelWorld.Update();
            Tweener.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _spriteBatch.Begin(blendState: BlendState.Opaque);
            // Set the desired winding order to clockwise
            RasterizerState rasterizerStateCw = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace
            };
            GraphicsDevice.RasterizerState = rasterizerStateCw;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            GraphicsDevice.Clear(Color.Black);
            _skySphere.Draw(GraphicsDevice);
            VoxelWorld.Draw(GraphicsDevice);
            _selectionCube.Draw();

            if (ShowDebugOverlay)
            {
                Vector3 nearestChunkFloorPlane = Vector3.Floor(CamPos / VoxelData.chunkSize.X) * VoxelData.chunkSize.X;
                CustomDraw.DrawGrid(GraphicsDevice, VoxelData.chunkSize.X, 1,
                    Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(nearestChunkFloorPlane));
            }
            _spriteBatch.End();

            base.Draw(gameTime);
            
            if (!ShowDebugOverlay) return;
            _spriteBatch.Begin();
            ImGuiRenderer.BeginLayout(gameTime);
            ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);

            ImGui.BeginMainMenuBar();
            
            ImGui.EndMainMenuBar();

            ImGui.Begin("Stats");
                ImGui.Text("Mem: " + GC.GetTotalMemory(true).ToString());
                ImGui.Text("FPS: " + 1000f / gameTime.ElapsedGameTime.TotalMilliseconds);
            ImGui.End();
            
            ImGui.Begin("Tools");
                ImGui.SliderInt("Brush Size", ref _brushSize, 1, 5);
            ImGui.End();
            ImGuiRenderer.EndLayout();
            _spriteBatch.End();
        }
    }
}