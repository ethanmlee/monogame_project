using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine.Scripts.Systems;

using System;
using System.Collections;
using System.Collections.Generic;


public class VoxelWorld
{
    public readonly Hashtable Chunks = new Hashtable(); 
    
    public static BasicEffect Effect;
    public static EffectPass EffectPass;

    public VoxelWorld(GraphicsDevice graphicsDevice)
    {
        Effect = new BasicEffect(graphicsDevice);

        // Populate room map
        for (var x = 0; x < VoxelData.worldSizeInChunks.X; x++)
        {
            for (var y = 0; y < VoxelData.worldSizeInChunks.Y; y++)
            {
                for (var z = 0; z < VoxelData.worldSizeInChunks.Z; z++)
                {
                    Chunks.Add(new Vector3Int(x, y, z),
                        new Chunk(new ChunkCoord(x, y, z), this, graphicsDevice));
                }
            }
        }
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        Effect.View = Game1.ViewMatrix;
        Effect.Projection = Game1.ProjectionMatrix;
        Effect.VertexColorEnabled = true;
        EffectPass = Effect.CurrentTechnique.Passes[0];
        
        foreach (Chunk chunk in Chunks.Values)
        {
            chunk.Draw();
        }
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = (pos.X / VoxelData.chunkSize.X).FloorToInt();
        int y = (pos.Y / VoxelData.chunkSize.Y).FloorToInt();
        int z = (pos.Z / VoxelData.chunkSize.Z).FloorToInt();
        return Chunks[new Vector3Int(pos)] as Chunk;
    }

    public bool IsChunkInWorld(ChunkCoord chunkCoord)
    {
        if (chunkCoord.X < 0 && chunkCoord.X > VoxelData.worldSizeInChunks.X) return false;
        if (chunkCoord.Y < 0 && chunkCoord.Y > VoxelData.worldSizeInChunks.Y) return false;
        if (chunkCoord.Z < 0 && chunkCoord.Z > VoxelData.worldSizeInChunks.Z) return false;

        return true;
    }

    public bool IsChunkInWorld(Vector3 chunkCoordVector3)
    {
        return IsChunkInWorld(new ChunkCoord(chunkCoordVector3));
    }
}


[System.Serializable]
public class Room
{
    public Openings openings;
    public int roomTypeID;
    public int rotations;

    public Room(Openings _openings, int _roomTypeID, int _rotations)
    {
        //// EXAMPLE: How to add masks to a flag
        //openings |= Openings.Front | Openings.Left;

        //// EXAMPLE: How to remove masks from a flag
        //openings &= ~Openings.Front & ~Openings.Left;

        openings = _openings;
        roomTypeID = _roomTypeID;
        rotations = _rotations;
    }

    [System.Flags]
    public enum Openings
    {
        Front = 1 << 1,
        Back = 1 << 2,
        Up = 1 << 3,
        Down = 1 << 4,
        Left = 1 << 5,
        Right = 1 << 6,
        Count
    };
}
