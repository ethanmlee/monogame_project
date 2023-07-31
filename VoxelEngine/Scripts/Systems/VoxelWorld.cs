using System.Linq;
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

    public readonly HashSet<ChunkCoord> ValidChunkCoords = new HashSet<ChunkCoord>();

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
        
        // GenerateAllChunks();
    }

    public async void GenerateAllChunks()
    {
        ChunkCoord camChunkCoord = GetChunkCoord(Game1.CamPos);
        ValidChunkCoords.Clear();
        for (int x = -VoxelData.RenderDistance; x <= VoxelData.RenderDistance; x++)
        for (int y = -VoxelData.RenderDistance; y <= VoxelData.RenderDistance; y++)
        for (int z = -VoxelData.RenderDistance; z <= VoxelData.RenderDistance; z++)
        {
            ValidChunkCoords.Add(new ChunkCoord(camChunkCoord.X + x, camChunkCoord.Y + y, camChunkCoord.Z + z));
        }

        await Task.WhenAll(Chunks.Values.Select((chunk => chunk.GenerateVoxelMap())));

        await Task.WhenAll(Chunks.Values.Select((chunk => chunk.CreateMesh())));
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
        return Chunks.TryGetValue(chunkCoord, out var _);
    }

    public bool IsChunkInWorld(Vector3 chunkCoordVector3)
    {
        return IsChunkInWorld(new ChunkCoord(chunkCoordVector3));
    }
}