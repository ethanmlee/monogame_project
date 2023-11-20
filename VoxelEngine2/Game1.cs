using System;
using System.Diagnostics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tweening;
using MonoGame.ImGuiNet;
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

        public static Vector3 CamPos = new Vector3((VoxelData.WorldSizeChunks.X * VoxelData.chunkSize.X) / 2f,
            VoxelData.WorldSizeChunks.Y * VoxelData.chunkSize.Y + 2,
            (VoxelData.WorldSizeChunks.Z * VoxelData.chunkSize.Z) / 2f);
        public static Vector3 CamEulerAngles = new Vector3(0, 0, 0);
        public static Matrix CamRotationMatrix = Matrix.Identity;

        private Point _startMousePoint => new Point(Window.ClientBounds.Size.X / 2, Window.ClientBounds.Size.Y / 2);

        public static ContentManager ContentManager;

        public bool ShowDebugOverlay = false;

        private SelectionCube _selectionCube = new SelectionCube();
        private SkySphere _skySphere = new SkySphere();
        private OrientationArrows _orientationArrows;

        public ImGuiRenderer GuiRenderer;

        private int _brushSize = 1;

        private bool _isCamGrounded = false;
        private Vector3 _playerVelocity = Vector3.Zero;

        public static readonly float GravityEarth =    9.81f;
        public static readonly float GravityMoon =     1.62f;
        public static readonly float JumpVelocityEarthAverage = 3.77f; // 28 inches peak jump height on Earth
        public static readonly float JumpVelocityEarthOneMeter = 4.42944691807002f;
        public static readonly float PlayerHeight = 2.7f;

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

            GuiRenderer = new ImGuiRenderer(this);
            ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _selectionCube.LoadContent(GraphicsDevice, ContentManager);
            _skySphere.LoadContent(ContentManager);
            _orientationArrows = new OrientationArrows(GraphicsDevice);

            VoxelWorld = new VoxelWorld(GraphicsDevice);
            
            GuiRenderer.RebuildFontAtlas();
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = KeyboardExtended.GetState();
            var mouseState = MouseExtended.GetState();
            IsMouseVisible = !IsActive || ShowDebugOverlay;

            if (keyboardState.WasKeyJustDown(Keys.F3)) ShowDebugOverlay = !ShowDebugOverlay;
            
            // Rotation
            var mouseDelta = mouseState.Position - _startMousePoint;
            if (IsMouseVisible) mouseDelta = Point.Zero;
            if (mouseDelta.ToVector2().LengthSquared() > 1)
            {
                CamEulerAngles.X += -mouseDelta.Y * 0.001f;
                CamEulerAngles.Y += -mouseDelta.X * 0.001f;
            }

            CamEulerAngles.X = MathHelper.Clamp(CamEulerAngles.X, -MathF.PI * 0.499f, MathF.PI * 0.499f);
            CamRotationMatrix = Matrix.CreateRotationX(CamEulerAngles.X) * Matrix.CreateRotationY(CamEulerAngles.Y);
            if (!IsMouseVisible) Mouse.SetPosition(_startMousePoint.X, _startMousePoint.Y);
            // if (!IsMouseVisible) MouseExtended.SetPosition(_startMousePoint);

            // Flying Position
            var moveInput = new Vector3(
                (keyboardState.IsKeyDown(Keys.D) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.A) ? 1 : 0), 
                0, //(keyboardState.IsKeyDown(Keys.Space) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.LeftShift) ? 1 : 0), 
                (keyboardState.IsKeyDown(Keys.W) ? 1 : 0) - (keyboardState.IsKeyDown(Keys.S) ? 1 : 0));
            Vector3 forwardFlattened = Vector3.Normalize(CamRotationMatrix.Forward * new Vector3(1, 0, 1));
            Vector3 movementRelative = forwardFlattened * moveInput.Z + CamRotationMatrix.Right * moveInput.X + CamRotationMatrix.Up * moveInput.Y;
            if (movementRelative.Length() > 0) movementRelative = Vector3.Normalize(movementRelative) * movementRelative.Length();
            
            movementRelative.Y = 0;
            Vector3 playerPlanarVelocity = _playerVelocity.XZ();
            playerPlanarVelocity = Vector3.Lerp(playerPlanarVelocity,
                Vector3Utils.ClampMagnitude(movementRelative, 1f) * 6,
                (_isCamGrounded ? 20 : 1) * (float)gameTime.ElapsedGameTime.TotalSeconds);
            _playerVelocity.X = playerPlanarVelocity.X;
            _playerVelocity.Z = playerPlanarVelocity.Z;
            
            _playerVelocity += Vector3.Down * GravityMoon * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.WasKeyJustUp(Keys.Space))
            {
                _playerVelocity += Vector3.Up * JumpVelocityEarthAverage;
            }

            Vector3 velocityThisFrame = _playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            bool wasGrounded = _isCamGrounded;
            _isCamGrounded = false;
            // Ground Hit
            if (_playerVelocity.Y < 0)
            {
                // Ground Check
                VoxelWorld.PerformRaycast(CamPos + Vector3.Down * (PlayerHeight - 1.001f), Vector3.Down, 1.0001 + MathF.Abs(velocityThisFrame.Y),
                    (hitInfo) =>
                    {
                        float standingHeight = (hitInfo.BlockPos.Y + 1) + PlayerHeight;
                        // Stop falling, allow moving up
                        if (hitInfo.Distance > 0)
                        {
                            _playerVelocity.Y = MathF.Max(-0.2f, _playerVelocity.Y);
                        }

                        // If we are hitting ground while already grounded, lerp to new step height
                        if (wasGrounded)
                        {
                            
                            // TODO: Check to make sure this won't cause a ceiling collision, if it will, act like a wall collision
                            CamPos.Y = MathHelper.Lerp(CamPos.Y, standingHeight, 
                                10 * (float)gameTime.ElapsedGameTime.TotalSeconds);
                        } 
                        else
                        {
                            CamPos.Y = standingHeight;
                        }

                        _isCamGrounded = true;
                    });
            }
            else
            {
                // Ceiling Hit
                VoxelWorld.PerformRaycast(CamPos, Vector3.Up, 0.1 + velocityThisFrame.Y, (hitInfo) =>
                {
                    _playerVelocity.Y = 0;
                });
            }

            CamPos += _playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            ViewMatrix = Matrix.CreateLookAt(CamPos, CamPos + Vector3.Transform(Vector3.Forward, CamRotationMatrix), Vector3.Up);

            BoundingFrustum.Matrix = ViewMatrix * ProjectionMatrix;

            _selectionCube.IsVisible = false;
            VoxelWorld.PerformRaycast(CamPos, CamRotationMatrix.Forward, 10, (hitInfo) =>
            {
                _selectionCube.IsVisible = true;
                _selectionCube.Position = hitInfo.BlockPosWorld + Vector3.One * 0.5f;
                if (mouseState.WasButtonJustDown(MouseButton.Right))
                {
                    VoxelWorld.SetVoxel(hitInfo.BlockPos + hitInfo.FaceDirection, 1);
                }
                if (mouseState.WasButtonJustDown(MouseButton.Left))
                {
                    VoxelWorld.SetVoxel(hitInfo.BlockPos, 0);
                }
            });
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
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
            GraphicsDevice.Clear(Color.Black);
            _skySphere.Draw(GraphicsDevice);
            VoxelWorld.Draw(GraphicsDevice);
            _selectionCube.Draw();
            
            _orientationArrows.Draw();

            if (ShowDebugOverlay)
            {
                Vector3 nearestChunkFloorPlane = Vector3.Floor(CamPos / VoxelData.chunkSize.X) * VoxelData.chunkSize.X;
                CustomDraw.DrawGrid(GraphicsDevice, VoxelData.chunkSize.X, 1,
                    Matrix.CreateRotationX(MathF.PI / 2) * Matrix.CreateTranslation(nearestChunkFloorPlane));
            }
            _spriteBatch.End();

            base.Draw(gameTime);
            
            if (!ShowDebugOverlay) return;
            
            // GuiRenderer.BeginLayout(gameTime);
            // ImGui.Begin("My First Tool");
            // ImGui.End();
            // ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);
            //
            // ImGui.BeginMainMenuBar();
            //
            // ImGui.EndMainMenuBar();
            //
            // ImGui.Begin("Stats");
            //     ImGui.Text("Mem: " + (GC.GetTotalMemory(true) / 1000000));
            //     ImGui.Text("FPS: " + 1000f / gameTime.ElapsedGameTime.TotalMilliseconds);
            // ImGui.End();
            //
            // ImGui.Begin("Tools");
            //     ImGui.SliderInt("Brush Size", ref _brushSize, 1, 5);
            // ImGui.End();
            // GuiRenderer.EndLayout();
        }
        
        public float CalculateInitialVelocity(float height, float gravity = 9.81f)
        {
            if (height < 0)
            {
                throw new ArgumentException("Height must be a non-negative value.");
            }

            if (gravity <= 0)
            {
                throw new ArgumentException("Gravity must be a positive value.");
            }

            float initialVelocity = MathF.Sqrt(2 * gravity * height);
            return initialVelocity;
        }
    }
}