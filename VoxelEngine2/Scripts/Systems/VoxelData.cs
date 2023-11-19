using Microsoft.Xna.Framework;

namespace VoxelEngine.Scripts.Systems;

using System.Collections;
using System.Collections.Generic;

public static class VoxelData
{

    public static readonly Vector3Int chunkSize = new Vector3Int(32, 32, 32); // Chunk size x and z must stay equal for rotation of rooms to work properly!!!
    public static int ChunkSizeTotal => chunkSize.X * chunkSize.Y * chunkSize.Z;
    public const int WorldPlanarSizeChunks = 16;
    public const int WorldHeightChunks = 2;
    public static readonly Vector3Int WorldSizeChunks = new Vector3Int(WorldPlanarSizeChunks,WorldHeightChunks,WorldPlanarSizeChunks);
    public static readonly int RenderDistance = 8;

    public static readonly float lightFalloff = 0.075f;

    public static int seed;

    public enum VoxelTypes
    {
        Block,
        EntitySpawner
    }

    public static Vector3Int WorldSizeInVoxels
    {
        get { return (chunkSize * WorldSizeChunks); }
    }

    public static readonly int textureAtlasSizeInBlocks = 16; 
    public static float normalizedBlockTextureSize
    {
        get { return 1f / (float)textureAtlasSizeInBlocks; }
    }

    // Offsets from the "block" origin point for each corner
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };

    // Directions to check depending on each face
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {

        new Vector3(0.0f, 0.0f, -1.0f),    // Back Face
        new Vector3(0.0f, 0.0f, 1.0f),     // Front Face
        new Vector3(0.0f, 1.0f, 0.0f),     // Top Face
        new Vector3(0.0f, -1.0f, 0.0f),    // Bottom Face
        new Vector3(-1.0f, 0.0f, 0.0f),    // Left Face
        new Vector3(1.0f, 0.0f, 0.0f),     // Right Face

    };

    // Triangle data that makes up the faces
    // Triangle data that makes up the faces with reversed winding order
    public static readonly int[,] voxelTris = new int[6, 4]
    {
        // Back, Front, Top, Bottom, Left, Right

        // Pattern: 0 1 2 2 1 3
        { 1, 2, 0, 3 },    // Back Face
        { 4, 7, 5, 6 },    // Front Face
        { 2, 6, 3, 7 },    // Top Face
        { 0, 4, 1, 5 },    // Bottom Face
        { 0, 3, 4, 7 },    // Left Face
        { 5, 6, 1, 2 }     // Right Face
    };


    // UV data position for each corner of a face
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {

        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f),

    };

    public static readonly Vector3[,] voxelAoChecks = new Vector3[24,2]
    {
        { Vector3.UnitX, -Vector3.UnitY },
        { Vector3.UnitX, Vector3.UnitY },
        { -Vector3.UnitX, -Vector3.UnitY },
        { -Vector3.UnitX, Vector3.UnitY },

        { -Vector3.UnitX, -Vector3.UnitY },
        { -Vector3.UnitX, Vector3.UnitY },
        { Vector3.UnitX, -Vector3.UnitY },
        { Vector3.UnitX, Vector3.UnitY },

        { Vector3.UnitX, -Vector3.UnitZ },
        { Vector3.UnitX, Vector3.UnitZ },
        { -Vector3.UnitX, -Vector3.UnitZ },
        { -Vector3.UnitX, Vector3.UnitZ },

        { -Vector3.UnitX, -Vector3.UnitZ },
        { -Vector3.UnitX, Vector3.UnitZ },
        { Vector3.UnitX, -Vector3.UnitZ },
        { Vector3.UnitX, Vector3.UnitZ },

        { -Vector3.UnitZ, -Vector3.UnitY },
        { -Vector3.UnitZ, Vector3.UnitY },
        { Vector3.UnitZ, -Vector3.UnitY },
        { Vector3.UnitZ, Vector3.UnitY },

        { Vector3.UnitZ, -Vector3.UnitY },
        { Vector3.UnitZ, Vector3.UnitY },
        { -Vector3.UnitZ, -Vector3.UnitY },
        { -Vector3.UnitZ, Vector3.UnitY },
    };

}
