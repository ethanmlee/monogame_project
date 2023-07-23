using Microsoft.Xna.Framework;

namespace VoxelEngine.Scripts.Systems;

using System.Collections;
using System.Collections.Generic;

public static class VoxelData
{

    public static readonly Vector3Int chunkSize = new Vector3Int(64, 64, 64); // Chunk size x and z must stay equal for rotation of rooms to work properly!!!
    public static readonly Vector3Int worldSizeInChunks = new Vector3Int(8,4,8);

    public static readonly float lightFalloff = 0.075f;

    public static int seed;

    public enum VoxelTypes
    {
        Block,
        EntitySpawner
    }

    public static Vector3Int WorldSizeInVoxels
    {
        get { return (chunkSize * worldSizeInChunks); }
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
    public static readonly int[,] voxelTris = new int[6, 4]
    {
        // Back, Front, Top, Bottom, Left, Right

        // Pattern: 0 1 2 2 1 3
        {0, 3, 1, 2 },    // Back Face
        {5, 6, 4, 7 },    // Front Face
        {3, 7, 2, 6 },    // Top Face
        {1, 5, 0, 4 },    // Bottom Face
        {4, 7, 0, 3 },    // Left Face
        {1, 2, 5, 6 }     // Right Face
    };

    // UV data position for each corner of a face
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {

        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f),

    };

}
