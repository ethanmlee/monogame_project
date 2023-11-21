using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VoxelEngine.Scripts.Systems;

public class VoxelWorld
{
    public readonly Dictionary<ChunkCoord, Chunk> Chunks = new Dictionary<ChunkCoord, Chunk>(); 
    
    public static Effect Effect;
    public static EffectPass EffectPass;
    
    public readonly EffectParameter ParamWorldMatrix;
    public readonly EffectParameter ParamViewMatrix;
    public readonly EffectParameter ParamProjectionMatrix;

    public HashSet<ChunkCoord> ValidChunkCoords = new HashSet<ChunkCoord>();

    private CancellationToken _worldGenCancellationToken;

    private Queue<ChunkCoord> _chunksToGenerate = new Queue<ChunkCoord>();
    private Queue<ChunkCoord> _chunksToRefresh = new Queue<ChunkCoord>();
    private Queue<ChunkCoord> _chunksToRefreshUser = new Queue<ChunkCoord>();

    public VoxelWorld(GraphicsDevice graphicsDevice)
    {
        Effect = Game1.ContentManager.Load<Effect>("Shaders/SimpleEffect");
        ParamWorldMatrix = Effect.Parameters["_WorldMatrix"];
        ParamViewMatrix = Effect.Parameters["_ViewMatrix"];
        ParamProjectionMatrix = Effect.Parameters["_ProjectionMatrix"];

        // Populate room map
        for (var x = 0; x < VoxelData.WorldSizeChunks.X; x++)
        {
            for (var y = 0; y < VoxelData.WorldSizeChunks.Y; y++)
            {
                for (var z = 0; z < VoxelData.WorldSizeChunks.Z; z++)
                {
                    ChunkCoord coord = new ChunkCoord(x, y, z);
                    var newChunk = new Chunk(coord, this, graphicsDevice);
                    Chunks.Add(coord, newChunk);
                }
            }
        }

        GenerateValidChunks();

        Task.Factory.StartNew(AsyncChunkGenerationTask);
        Task.Factory.StartNew(AsyncChunkMeshingTask);

        // GenerateAllChunks();
    }

    public void GenerateValidChunks()
    {
        ChunkCoord camChunkCoord = GetChunkCoord(Game1.CamPos);
        var newValidChunks = new HashSet<ChunkCoord>((int)Math.Pow(VoxelData.RenderDistance * 2, 3));
        for (int y = -VoxelData.RenderDistance; y <= VoxelData.RenderDistance; y++)
        for (int x = -VoxelData.RenderDistance; x <= VoxelData.RenderDistance; x++)
        for (int z = -VoxelData.RenderDistance; z <= VoxelData.RenderDistance; z++)
        {
            var newChunkCoord = new ChunkCoord(camChunkCoord.X + x, camChunkCoord.Y + y, camChunkCoord.Z + z);
            if (!IsChunkInWorld(newChunkCoord)) continue;
            newValidChunks.Add(newChunkCoord);
        }
        // Remove chunks
        // var chunksToRemove = ValidChunkCoords.Except(newValidChunks);
        // await Parallel.ForEachAsync(chunksToRemove, _worldGenCancellationToken,  (chunk, ct) =>
        // {
        //     Chunks[chunk].DisposeMesh();
        //     return ValueTask.CompletedTask;
        // });
        // Create chunks
        var chunksToCreate = newValidChunks.Except(ValidChunkCoords).ToArray();
        var newChunks = Chunks
            .Where(chunk => chunksToCreate.Contains(chunk.Value.Coord))
            .OrderBy((coord => ((coord.Key.Vector * VoxelData.chunkSize) - Game1.CamPos).Length()))
            .ToArray();
        // await Parallel.ForEachAsync(newChunks, _worldGenCancellationToken,  (chunk, ct) =>
        // {
        //     chunk.Value.GenerateVoxelMap().WaitAsync(ct);
        //     return ValueTask.CompletedTask;
        // });
        // await Parallel.ForEachAsync(newChunks, _worldGenCancellationToken,  (chunk, ct) =>
        // {
        //     chunk.Value.CreateMesh().WaitAsync(ct);
        //     return ValueTask.CompletedTask;
        // });
        
        foreach (var (key, value) in newChunks)
        {
            _chunksToGenerate.Enqueue(key);
        }
        
        ValidChunkCoords = newValidChunks;
    }

    private async Task AsyncChunkGenerationTask()
    {
        while (true)
        {
            if (_chunksToGenerate.TryDequeue(out var chunkToGenerate))
            {
                Chunks[chunkToGenerate].GenerateVoxelMap();
                RefreshChunkGenerated(chunkToGenerate);
            }
            await Task.Yield();
        }
    }
    
    private async Task AsyncChunkMeshingTask()
    {
        int loopCount = 3;
        while (true)
        {
            for (int i = 0; i < loopCount; i++)
            {
                if (_chunksToRefresh.TryDequeue(out var chunkToRefresh))
                {
                    Chunks[chunkToRefresh].CreateMesh();
                }
            }
            await Task.Yield();
        }
    }

    public void Update()
    {
        // Refresh all user edited chunks on main thread so it's an instant response for the player
        int loopCount = 3;
        while (_chunksToRefreshUser.Count > 0)
        {
            for (int i = 0; i < loopCount; i++)
            {
                if (_chunksToRefreshUser.TryDequeue(out var chunkToRefresh))
                {
                    Chunks[chunkToRefresh].CreateMesh();
                }
            }

            break;
        }
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        ParamViewMatrix.SetValue(Game1.ViewMatrix);
        ParamProjectionMatrix.SetValue(Game1.ProjectionMatrix);
        EffectPass = Effect.CurrentTechnique.Passes[0];
        
        foreach (Chunk chunk in Chunks.Values)
        {
            chunk.Draw();
        }
    }

    public void SetVoxel(Vector3 worldPos, byte index) 
    {
        ChunkCoord coord = GetChunkCoord(worldPos);
        if (!IsChunkInWorld(coord)) return;
        
        Chunk chunk = GetChunk(coord);
        Vector3Int posInChunk = new Vector3Int(worldPos) % VoxelData.chunkSize;
        chunk.SetVoxel(posInChunk, index);
        RefreshChunk(coord);

        if (posInChunk.X == 0)
            RefreshChunk(new ChunkCoord(coord.X - 1, coord.Y, coord.Z));
        if (posInChunk.X == VoxelData.chunkSize.X - 1) 
            RefreshChunk(new ChunkCoord(coord.X + 1, coord.Y, coord.Z));
        if (posInChunk.Y == 0)
            RefreshChunk(new ChunkCoord(coord.X, coord.Y - 1, coord.Z));
        if (posInChunk.Y == VoxelData.chunkSize.Y - 1) 
            RefreshChunk(new ChunkCoord(coord.X, coord.Y + 1, coord.Z));
        if (posInChunk.Z == 0)
            RefreshChunk(new ChunkCoord(coord.X, coord.Y, coord.Z - 1));
        if (posInChunk.Z == VoxelData.chunkSize.Z - 1) 
            RefreshChunk(new ChunkCoord(coord.X, coord.Y, coord.Z + 1));
    }

    public byte GetVoxel(Vector3 worldPos)
    {
        ChunkCoord coord = GetChunkCoord(worldPos);
        if (!IsChunkInWorld(coord)) return 0;
        
        Chunk chunk = GetChunk(coord);
        Vector3Int posInChunk = new Vector3Int(worldPos) % VoxelData.chunkSize;
        return chunk.GetVoxel(posInChunk);
    }
    
    public void RefreshChunkGenerated(ChunkCoord coord)
    {
        if (!IsChunkInWorld(coord)) return;
        if (_chunksToRefresh.Contains(coord)) return;
        _chunksToRefresh.Enqueue(coord);
    }

    public void RefreshChunk(ChunkCoord coord)
    {
        if (!IsChunkInWorld(coord)) return;
        if (_chunksToRefreshUser.Contains(coord)) return;
        _chunksToRefreshUser.Enqueue(coord);
    }

    public bool TryGetChunk(ChunkCoord chunkCoord, [MaybeNullWhen(false)] out Chunk chunk)
    {
        if (Chunks.TryGetValue(chunkCoord, out var gotChunk))
        {
            chunk = gotChunk;
            return true;
        }

        chunk = null;
        return false;
    }

    public Chunk GetChunk(ChunkCoord chunkCoord)
    {
        return Chunks[chunkCoord];
    }
    
    public ChunkCoord GetChunkCoord(Vector3 pos)
    {
        int x = (pos.X / VoxelData.chunkSize.X).FloorToInt();
        int y = (pos.Y / VoxelData.chunkSize.Y).FloorToInt();
        int z = (pos.Z / VoxelData.chunkSize.Z).FloorToInt();
        return new ChunkCoord(x, y, z);
    }
    
    public ChunkCoord GetChunkCoord(Vector3Int pos)
    {
        int x = (int)MathF.Floor((float)pos.X / VoxelData.chunkSize.X);
        int y = (int)MathF.Floor((float)pos.Y / VoxelData.chunkSize.Y);
        int z = (int)MathF.Floor((float)pos.Z / VoxelData.chunkSize.Z);
        return new ChunkCoord(x, y, z);
    }

    public bool IsChunkInWorld(ChunkCoord chunkCoord)
    {
        return (chunkCoord.X >= 0 && chunkCoord.X < VoxelData.WorldSizeChunks.X) &&
               (chunkCoord.Y >= 0 && chunkCoord.Y < VoxelData.WorldSizeChunks.Y) &&
               (chunkCoord.Z >= 0 && chunkCoord.Z < VoxelData.WorldSizeChunks.Z);
    }
    
    public void PerformRaycast(Vector3 origin, Vector3 direction, double radius,
        Action<RaycastVoxelHitInfo> callback, bool isLooping = true)
    {
        if (direction.X == -0) direction.X = 0;
        if (direction.Y == -0) direction.Y = 0;
        if (direction.Z == -0) direction.Z = 0;
        int x = (int)Math.Floor(origin.X);
        int y = (int)Math.Floor(origin.Y);
        int z = (int)Math.Floor(origin.Z);

        int xWorld = x;
        int yWorld = y;
        int zWorld = z;

        double dx = direction.X;
        double dy = direction.Y;
        double dz = direction.Z;

        int stepX = Signum(dx);
        int stepY = Signum(dy);
        int stepZ = Signum(dz);

        double tMaxX = Intbound(origin.X, dx);
        double tMaxY = Intbound(origin.Y, dy);
        double tMaxZ = Intbound(origin.Z, dz);

        double tDeltaX = stepX / dx;
        double tDeltaY = stepY / dy;
        double tDeltaZ = stepZ / dz;

        Vector3 face = new Vector3();
        
        // Check if the ray starts inside a voxel
        byte initialVoxelId = GetVoxel(new Vector3(x, y, z));
        if (initialVoxelId != 0)
        {
            Vector3Int hitBlockPos = new Vector3Int(x, y, z);
            Vector3Int hitBlockPosWorld = new Vector3Int(xWorld, yWorld, zWorld);
            callback.Invoke(new RaycastVoxelHitInfo(hitBlockPos, hitBlockPosWorld, initialVoxelId, face, 0f));
            return;
        }

        if (dx == 0 && dy == 0 && dz == 0)
            throw new ArgumentOutOfRangeException("Raycast in zero direction!");

        radius /= Math.Sqrt(dx*dx + dy*dy + dz*dz);

        var worldSizeX = VoxelData.WorldSizeChunks.X * VoxelData.chunkSize.X;
        var worldSizeY = VoxelData.WorldSizeChunks.Y * VoxelData.chunkSize.Y;
        var worldSizeZ = VoxelData.WorldSizeChunks.Z * VoxelData.chunkSize.Z;
        while (true)
        {
            x = xWorld;
            y = yWorld;
            z = zWorld;
            
            if (isLooping)
            {
                // Wrap around when reaching the edge of the world
                if (x < 0) x += worldSizeX;
                if (z < 0) z += worldSizeZ;
                if (x >= worldSizeX) x %= worldSizeX;
                if (z >= worldSizeZ) z %= worldSizeZ;
            }
            else
            {
                if ((stepX > 0 ? x >= worldSizeX : x < 0) ||
                    (stepY > 0 ? y >= worldSizeY : y < 0) ||
                    (stepZ > 0 ? z >= worldSizeZ : z < 0))
                {
                    // Stop ray if outside the world bounds (non-looping)
                    break;
                }
            }

            byte voxelId = GetVoxel(new Vector3(x, y, z));
            if (voxelId != 0)
            {
                Vector3Int hitBlockPos = new Vector3Int(x, y, z);
                Vector3Int hitBlockPosWorld = new Vector3Int(xWorld, yWorld, zWorld);
                callback.Invoke(new RaycastVoxelHitInfo(hitBlockPos, hitBlockPosWorld, voxelId, face,
                    Vector3.Distance(origin, hitBlockPos)));
                break;
            }

            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    if (tMaxX > radius) break;
                    xWorld += stepX;
                    tMaxX += tDeltaX;
                    face = new Vector3(-stepX, 0, 0);
                }
                else
                {
                    if (tMaxZ > radius) break;
                    zWorld += stepZ;
                    tMaxZ += tDeltaZ;
                    face = new Vector3(0, 0, -stepZ);
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    if (tMaxY > radius) break;
                    yWorld += stepY;
                    tMaxY += tDeltaY;
                    face = new Vector3(0, -stepY, 0);
                }
                else
                {
                    if (tMaxZ > radius) break;
                    zWorld += stepZ;
                    tMaxZ += tDeltaZ;
                    face = new Vector3(0, 0, -stepZ);
                }
            }
        }
    }

    private double Intbound(double s, double ds)
    {
        if (ds < 0)
        {
            return Intbound(-s, -ds);
        }
        else
        {
            s = Mod(s, 1);
            return (1 - s) / ds;
        }
    }

    private int Signum(double x)
    {
        return x > 0 ? 1 : x < 0 ? -1 : 0;
    }

    private double Mod(double value, double modulus)
    {
        return (value % modulus + modulus) % modulus;
    }

    public struct RaycastVoxelHitInfo
    {
        public Vector3Int BlockPos;
        public Vector3Int BlockPosWorld;
        public byte BlockId;
        public Vector3 FaceDirection;
        public float Distance;

        public RaycastVoxelHitInfo(Vector3Int blockPos, Vector3Int blockPosWorld, byte blockId, Vector3 faceDirection, float distance)
        {
            BlockPos = blockPos;
            BlockPosWorld = blockPosWorld;
            BlockId = blockId;
            FaceDirection = faceDirection;
            Distance = distance;
        }
    }

}