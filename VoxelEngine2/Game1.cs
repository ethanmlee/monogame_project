using System;
using System.Collections.Generic;
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
using VoxelEngine2.Scripts.Systems.Physics;

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
        public static readonly float PlayerCamHeight = 2.5f;

        public static KeyboardStateExtended KeyboardState;
        public static MouseStateExtended MouseState;

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
            // _graphics.PreferredBackBufferWidth = 512;
            // _graphics.PreferredBackBufferHeight = 288;
            _graphics.PreferredBackBufferWidth *= 4;
            _graphics.PreferredBackBufferHeight *= 4;
            // _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = false;
            IsFixedTimeStep = true;
            _graphics.ApplyChanges();
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), (float)_graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight, 0.1f, 1000f);
            
            
            MouseExtended.SetPosition(_startMousePoint);
            IsMouseVisible = false;

            GuiRenderer = new ImGuiRenderer(this);
            ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable | 
                                        ImGuiConfigFlags.DpiEnableScaleFonts |
                                        ImGuiConfigFlags.DpiEnableScaleViewports;

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
            KeyboardState = KeyboardExtended.GetState();
            MouseState = MouseExtended.GetState();
            IsMouseVisible = !IsActive || ShowDebugOverlay;

            if (!ShowDebugOverlay)
            {
                HandlePlayerInput(gameTime);
                HandlePlayerPhysics(gameTime);
            }

            if (KeyboardState.WasKeyJustDown(Keys.Escape)) ShowDebugOverlay = !ShowDebugOverlay;
            
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
            
            GuiRenderer.BeginLayout(gameTime);
            ImGui.GetIO().FontGlobalScale = 2;
            ImGui.DockSpaceOverViewport(null, 
                ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingInCentralNode);
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("VoxelEngine"))
                {
                    if (ImGui.Button("Quit"))
                    {
                        Exit();
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }

            ImGui.Begin("Stats");
                ImGui.Text("Mem: " + (GC.GetTotalMemory(true) / 1000000));
                ImGui.Text("FPS: " + 1000f / gameTime.ElapsedGameTime.TotalMilliseconds);
                ImGui.Spacing();
                ImGui.Text($"Font Size: {ImGui.GetFont().FontSize}");
            ImGui.End();
            
            ImGui.Begin("Tools");
                ImGui.SliderInt("Brush Size", ref _brushSize, 1, 5);
            ImGui.End();
            GuiRenderer.EndLayout();
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

        public void HandlePlayerInput(GameTime gameTime)
        {
            // Rotation
            var mouseDelta = MouseState.Position - _startMousePoint;
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
                (KeyboardState.IsKeyDown(Keys.D) ? 1 : 0) - (KeyboardState.IsKeyDown(Keys.A) ? 1 : 0), 
                0, 
                (KeyboardState.IsKeyDown(Keys.W) ? 1 : 0) - (KeyboardState.IsKeyDown(Keys.S) ? 1 : 0));
            Vector3 forwardFlattened = Vector3.Normalize(CamRotationMatrix.Forward.XZ());
            Vector3 movementRelative = forwardFlattened * moveInput.Z + CamRotationMatrix.Right * moveInput.X + CamRotationMatrix.Up * moveInput.Y;
            if (movementRelative.Length() > 0) movementRelative = Vector3.Normalize(movementRelative) * movementRelative.Length();
            
            movementRelative.Y = 0;
            Vector3 playerPlanarVelocity = _playerVelocity.XZ();
            playerPlanarVelocity = Vector3.Lerp(playerPlanarVelocity,
                Vector3Utils.ClampMagnitude(movementRelative, 1f) * 6,
                (_isCamGrounded ? 20 : 1) * (float)gameTime.ElapsedGameTime.TotalSeconds);
            _playerVelocity.X = playerPlanarVelocity.X;
            _playerVelocity.Z = playerPlanarVelocity.Z;
            
            if (KeyboardState.WasKeyJustUp(Keys.Space))
            {
                _playerVelocity += Vector3.Up * JumpVelocityEarthAverage;
            }
            
            _selectionCube.IsVisible = false;
            VoxelWorld.PerformRaycast(CamPos, CamRotationMatrix.Forward, 10, (hitInfo) =>
            {
                _selectionCube.IsVisible = true;
                _selectionCube.Position = hitInfo.BlockPosWorld + Vector3.One * 0.5f;
                if (MouseState.WasButtonJustDown(MouseButton.Right))
                {
                    Vector3 newBlockPos = hitInfo.BlockPos + hitInfo.FaceDirection;
                    foreach (Vector3 spherePosition in GenerateSpherePositions(newBlockPos, _brushSize - 1))
                    {
                        VoxelWorld.SetVoxel(spherePosition, 1);
                    }
                    _selectionCube.IsVisible = false;
                }
                if (MouseState.WasButtonJustDown(MouseButton.Left))
                {
                    VoxelWorld.SetVoxel(hitInfo.BlockPos, 0);
                    _selectionCube.IsVisible = false;
                }
            });
            
            if (MouseState.WasButtonJustDown(MouseButton.Middle))
            {
                VoxelWorld.PerformRaycast(CamPos, CamRotationMatrix.Forward, 400, (hitInfo) =>
                {
                    CamPos = hitInfo.BlockPos + Vector3.Up * (PlayerHeight + 1) + Vector3.One.XZ() * 0.5f;
                });
            }
            _selectionCube.Update(gameTime);
        }

        public void HandlePlayerPhysics(GameTime gameTime)
        {
            _playerVelocity += Vector3.Down * GravityMoon * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 velocityThisFrame = _playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            bool wasGrounded = _isCamGrounded;
            _isCamGrounded = false;
            // Ground Hit
            // if (_playerVelocity.Y < 0)
            // {
            //     // Ground Check
            //     VoxelWorld.PerformRaycast(CamPos + Vector3.Down * (PlayerHeight - 1.1f), Vector3.Down,
            //         1.1 + MathF.Abs(velocityThisFrame.Y),
            //         (hitInfo) =>
            //         {
            //             float standingHeight = (hitInfo.BlockPos.Y + 1) + PlayerHeight;
            //             // Stop falling, allow moving up
            //             if (hitInfo.Distance > 0)
            //             {
            //                 _playerVelocity.Y = MathF.Max(-0.2f, _playerVelocity.Y);
            //
            //                 // If we are hitting ground while already grounded, lerp to new step height
            //                 // if (wasGrounded)
            //                 // {
            //                 //     // TODO: Check to make sure this won't cause a ceiling collision, if it will, act like a wall collision
            //                 //     CamPos.Y = MathHelper.Lerp(CamPos.Y, standingHeight,
            //                 //         10 * (float)gameTime.ElapsedGameTime.TotalSeconds);
            //                 // }
            //             }
            //
            //             CamPos.Y = standingHeight;
            //
            //             _isCamGrounded = true;
            //         });
            // }
            // else
            // {
            //     // Ceiling Hit
            //     VoxelWorld.PerformRaycast(CamPos, Vector3.Up, 0.1 + velocityThisFrame.Y, (hitInfo) =>
            //     {
            //         _playerVelocity.Y = 0;
            //     });
            // }

            // Wall Collision
            float skinWidth = 0.05f;
            float skinWidthHalf = skinWidth * 0.5f;
            
            AABB boundingBox = new AABB(CamPos + (Vector3.Down * PlayerCamHeight) + (Vector3.Up * (PlayerHeight * 0.5f)),
                new Vector3(0.6f, PlayerHeight, 0.6f));
            VoxelWorld.RaycastVoxelHitInfo hitInfo = new VoxelWorld.RaycastVoxelHitInfo();
            CollideAndSlide(boundingBox, boundingBox.Center, Vector3.Right, (int)MathF.Sign(_playerVelocity.X),
                velocityThisFrame.X, boundingBox.Right, boundingBox.Front, boundingBox.Top, Vector3.Forward,
                Vector3.Up, skinWidthHalf, out hitInfo);
            CollideAndSlide(boundingBox, boundingBox.Center, Vector3.Forward, (int)MathF.Sign(_playerVelocity.Z),
                velocityThisFrame.Z, boundingBox.Front, boundingBox.Right, boundingBox.Top, Vector3.Right,
                Vector3.Up, skinWidthHalf, out hitInfo);
            
            _isCamGrounded = false;
            if (CollideAndSlide(boundingBox, boundingBox.Center, Vector3.Up, (int)MathF.Sign(_playerVelocity.Y),
                    velocityThisFrame.Y, boundingBox.Top, boundingBox.Right, boundingBox.Front, Vector3.Right,
                    Vector3.Forward, skinWidthHalf, out hitInfo))
            {
                if (MathF.Sign(velocityThisFrame.Y) < 0)
                {
                    _isCamGrounded = true;
                }
            }

            CamPos += _playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            ViewMatrix = Matrix.CreateLookAt(CamPos, CamPos + Vector3.Transform(Vector3.Forward, CamRotationMatrix), Vector3.Up);

            BoundingFrustum.Matrix = ViewMatrix * ProjectionMatrix;
        }
        
        public Vector3[] GenerateSpherePositions(Vector3 center, float radius)
        {
            // Create a list to store positions.
            List<Vector3> positions = new List<Vector3>();

            int radiusCeiling = (int)Math.Ceiling(radius);

            for (int x = (int)Math.Floor(center.X) - radiusCeiling; x <= (int)Math.Floor(center.X) + radiusCeiling; x++)
            {
                for (int y = (int)Math.Floor(center.Y) - radiusCeiling; y <= (int)Math.Floor(center.Y) + radiusCeiling; y++)
                {
                    for (int z = (int)Math.Floor(center.Z) - radiusCeiling; z <= (int)Math.Floor(center.Z) + radiusCeiling; z++)
                    {
                        Vector3 position = new Vector3(x, y, z);
                        float distance = Vector3.Distance(position, center);

                        if (distance <= radius)
                        {
                            positions.Add(position);
                        }
                    }
                }
            }

            // Convert the list to an array before returning.
            return positions.ToArray();
        }

        public bool CollideAndSlide(AABB boundingBox, Vector3 center, Vector3 direction, int directionSign, float distance, float mainAxisSpan, float span1, float span2, Vector3 span1Dir, Vector3 span2Dir, float skinWidthHalf, out VoxelWorld.RaycastVoxelHitInfo hitInfo)
        {
            bool result = false;
            VoxelWorld.RaycastVoxelHitInfo resultHitInfo = new VoxelWorld.RaycastVoxelHitInfo();
            
            direction = new Vector3(MathF.Abs(direction.X), MathF.Abs(direction.Y), MathF.Abs(direction.Z));
            span1Dir = new Vector3(MathF.Abs(span1Dir.X), MathF.Abs(span1Dir.Y), MathF.Abs(span1Dir.Z));
            span2Dir = new Vector3(MathF.Abs(span2Dir.X), MathF.Abs(span2Dir.Y), MathF.Abs(span2Dir.Z));
            if (directionSign != 0)
            {
                int rayDir = directionSign;
                for (float yOff = -span1; yOff <= span1; yOff += (span1 * 2f) * 0.25f)
                for (float zOff = -span2; zOff <= span2; zOff += (span2 * 2f) * 0.25f)
                {
                    Vector3 rayStartPos = center + direction * mainAxisSpan * rayDir;
                    rayStartPos += direction * -(skinWidthHalf * rayDir);
                    rayStartPos += span1Dir * yOff;
                    rayStartPos += span2Dir * zOff;

                    VoxelWorld.PerformRaycast(rayStartPos, direction * rayDir,
                        MathF.Abs(distance) + skinWidthHalf,
                        (hitInfo) =>
                        {
                            if (hitInfo.Distance < skinWidthHalf) return;
                            Vector3 blockPosWorldCenter = hitInfo.BlockPosWorld + Vector3.One * 0.5f;

                            float blockPosWorldCenterAxis = Vector3.Dot(blockPosWorldCenter, direction);
                            // Calculate the difference between the signed magnitudes of position2 and positionVector
                            float magnitudeDifference = Vector3.Dot(center, direction) - blockPosWorldCenterAxis + (0.5f + mainAxisSpan) * rayDir;

                            // Calculate the new aligned position2 by adjusting it along the cancelDirection axis
                            Vector3 alignedPosition2 = center - magnitudeDifference * direction;
                            CamPos = alignedPosition2 - (Vector3.Up * boundingBox.Size.Y * 0.5f) + (Vector3.Up * PlayerCamHeight);
                            
                            Vector3 resultingVelocity = _playerVelocity - (Vector3.Dot(_playerVelocity, direction)) * direction;
                            _playerVelocity = resultingVelocity;

                            resultHitInfo = hitInfo;
                            result = true;
                        });
                }
            }

            hitInfo = resultHitInfo;
            return result;
        }
    }
}