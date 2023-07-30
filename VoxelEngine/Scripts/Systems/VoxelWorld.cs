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
    public readonly Hashtable Chunks = new Hashtable(); 
    
    public static Effect Effect;
    public static EffectPass EffectPass;
    
    public readonly EffectParameter ParamWorldMatrix;
    public readonly EffectParameter ParamViewMatrix;
    public readonly EffectParameter ParamProjectionMatrix;

    public VoxelWorld(GraphicsDevice graphicsDevice)
    {
        Effect = Game1.ContentManager.Load<Effect>("Shaders/SimpleEffect");
        ParamWorldMatrix = Effect.Parameters["_WorldMatrix"];
        ParamViewMatrix = Effect.Parameters["_ViewMatrix"];
        ParamProjectionMatrix = Effect.Parameters["_ProjectionMatrix"];

        // Populate room map
        for (var x = 0; x < VoxelData.worldSizeInChunks.X; x++)
        {
            for (var y = 0; y < VoxelData.worldSizeInChunks.Y; y++)
            {
                for (var z = 0; z < VoxelData.worldSizeInChunks.Z; z++)
                {
                    var newChunk = new Chunk(new ChunkCoord(x, y, z), this, graphicsDevice);
                    Chunks.Add(new ChunkCoord(x, y, z), newChunk);
                }
            }
        }
        
        Task.WhenAll(Chunks.Values.Cast<Chunk>().Select((chunk => chunk.GenerateVoxelMap()))).Wait();
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
        graphicsDevice.Clear(Color.Black);
        ParamViewMatrix.SetValue(Game1.ViewMatrix);
        ParamProjectionMatrix.SetValue(Game1.ProjectionMatrix);
        EffectPass = Effect.CurrentTechnique.Passes[0];
        
        foreach (Chunk chunk in Chunks.Values)
        {
            chunk.Draw();
        }
    }

    public Chunk GetChunk(Vector3 pos)
    {
        int x = (pos.X / VoxelData.chunkSize.X).FloorToInt();
        int y = (pos.Y / VoxelData.chunkSize.Y).FloorToInt();
        int z = (pos.Z / VoxelData.chunkSize.Z).FloorToInt();
        return Chunks[new Vector3Int(pos)] as Chunk;
    }

    public Chunk GetChunk(ChunkCoord chunkCoord)
    {
        return Chunks[chunkCoord] as Chunk;
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
        return Chunks.Contains(chunkCoord);
    }

    public bool IsChunkInWorld(Vector3 chunkCoordVector3)
    {
        return IsChunkInWorld(new ChunkCoord(chunkCoordVector3));
    }
}