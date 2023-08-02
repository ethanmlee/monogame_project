using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine.Scripts.Systems;

using System;
using System.Collections;
using System.Collections.Generic;


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

        Task.Factory.StartNew((async () =>
        {
            while (!_worldGenCancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
                await GenerateValidChunks();
            }

            return Task.CompletedTask;
        }));

        // GenerateAllChunks();
    }

    public async Task GenerateAllChunks()
    {
        ChunkCoord camChunkCoord = GetChunkCoord(Game1.CamPos);
        var newValidChunks = new HashSet<ChunkCoord>();
        for (int x = -VoxelData.RenderDistance; x <= VoxelData.RenderDistance; x++)
        for (int y = -VoxelData.RenderDistance; y <= VoxelData.RenderDistance; y++)
        for (int z = -VoxelData.RenderDistance; z <= VoxelData.RenderDistance; z++)
        {
            newValidChunks.Add(new ChunkCoord(camChunkCoord.X + x, camChunkCoord.Y + y, camChunkCoord.Z + z));
        }

        ValidChunkCoords = newValidChunks;
        
        await Task.WhenAll(Chunks.Values.Select((chunk => chunk.GenerateVoxelMap())));

        await Task.WhenAll(Chunks.Values.Select((chunk => chunk.CreateMesh())));
    }

    public async Task GenerateValidChunks()
    {
        ChunkCoord camChunkCoord = GetChunkCoord(Game1.CamPos);
        var newValidChunks = new HashSet<ChunkCoord>();
        for (int y = -VoxelData.RenderDistance; y <= VoxelData.RenderDistance; y++)
        for (int x = -VoxelData.RenderDistance; x <= VoxelData.RenderDistance; x++)
        for (int z = -VoxelData.RenderDistance; z <= VoxelData.RenderDistance; z++)
        {
            var newChunkCoord = new ChunkCoord(camChunkCoord.X + x, camChunkCoord.Y + y, camChunkCoord.Z + z);
            if (!IsChunkInWorld(newChunkCoord)) continue;
            newValidChunks.Add(new ChunkCoord(camChunkCoord.X + x, camChunkCoord.Y + y, camChunkCoord.Z + z));
        }
        // Remove chunks
        var chunksToRemove = ValidChunkCoords.Except(newValidChunks);
        foreach (var chunkCoord in chunksToRemove)
        {
            Chunks[chunkCoord].DisposeMesh();
        }
        // Create chunks
        var chunksToCreate = newValidChunks.Except(ValidChunkCoords).ToArray();
        var newChunks = Chunks
            .Where(chunk => chunksToCreate.Contains(chunk.Value.Coord))
            .OrderBy((coord => (coord.Key.Vector * VoxelData.chunkSize - Game1.CamPos).LengthSquared()))
            .ToArray();
        await Task.WhenAll(newChunks.Select((chunk => chunk.Value.GenerateVoxelMap())));
        await Task.WhenAll(newChunks.Select((chunk => chunk.Value.CreateMesh())));
        
        ValidChunkCoords = newValidChunks;
    }

    public void Update()
    {
        foreach (Chunk chunk in Chunks.Values)
        {
            chunk.Update();
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

    public void RefreshChunk(ChunkCoord coord)
    {
        if (!IsChunkInWorld(coord)) return;
        GetChunk(coord).SetDirty();
    }

    public Chunk GetChunk(Vector3 pos)
    {
        int x = (pos.X / VoxelData.chunkSize.X).FloorToInt();
        int y = (pos.Y / VoxelData.chunkSize.Y).FloorToInt();
        int z = (pos.Z / VoxelData.chunkSize.Z).FloorToInt();
        return Chunks[new ChunkCoord(pos)] as Chunk;
    }

    public Chunk? TryGetChunk(ChunkCoord chunkCoord)
    {
        if (Chunks.TryGetValue(chunkCoord, out var gotChunk))
        {
            return gotChunk;
        }

        return null;
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
        return Chunks.ContainsKey(chunkCoord);
    }

    public bool IsChunkInWorld(Vector3 chunkCoordVector3)
    {
        return IsChunkInWorld(new ChunkCoord(chunkCoordVector3));
    }
}