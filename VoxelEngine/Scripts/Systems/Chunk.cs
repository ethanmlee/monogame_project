using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEngine.Scripts.Systems;

using System;
using System.Collections.Generic;
using System.IO;

public class Chunk
{
    public ChunkCoord Coord;
    public VoxelState[] VoxelMap =
        new VoxelState[VoxelData.chunkSize.X * VoxelData.chunkSize.Y * VoxelData.chunkSize.Z];
    
    private VertexBufferBinding _binding;
    private VertexBuffer _vb;
    private IndexBuffer _ib;

    private readonly Vector3 _position;

    private int _vertexIndex;

    // Vertices
    private readonly List<VertexPositionColor> _vertices;
    private  VertexPositionColor[] _verticesArray = Array.Empty<VertexPositionColor>();
    
    // Triangle Indices
    private readonly List<int> _triangles;
    private  int[] _trianglesArray = Array.Empty<int>();
    
    private readonly List<Vector3> _uvs = new();
    private readonly List<Vector3> _normals;

    private readonly Queue<Vector3> _lightBfsQueue = new();
    private readonly List<Color> _colors = new();

    private readonly Queue<Vector3Int> _blocksToUpdate = new();

    private readonly VoxelWorld _world;

    private readonly GraphicsDevice _graphicsDevice;

    public Chunk(ChunkCoord coord, VoxelWorld world, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        Coord = coord;
        _world = world;
        int vertCount = VoxelData.chunkSize.X * VoxelData.chunkSize.Y * VoxelData.chunkSize.Z * 8;
        _vertices = new List<VertexPositionColor>();
        _triangles = new List<int>();
        _normals = new List<Vector3>();

        _position = Coord.Vector * VoxelData.chunkSize;

        GenerateVoxelMap();
    }

    public void Draw()
    {
        if (_verticesArray.Length < 3) return;
        var bounding = new BoundingBox(_position, _position + VoxelData.chunkSize);
        if (Game1.BoundingFrustum.Contains(bounding) == ContainmentType.Disjoint) return;
        
        VoxelWorld.Effect.World = Matrix.CreateTranslation(_position);
        VoxelWorld.EffectPass.Apply();
        _graphicsDevice.DrawUserIndexedPrimitives(
            PrimitiveType.TriangleList,
            _verticesArray, 0, _verticesArray.Length,
            _trianglesArray, 0, _trianglesArray.Length / 3
        );
    }

    public void GenerateVoxelMap()
    {
        ClearMeshData();

        var simplex = new SeamlessSimplexNoise(VoxelData.chunkSize.X * VoxelData.worldSizeInChunks.X, 1337);
        
        new Thread((() =>
        {
            for (var i = 0; i < VoxelMap.Length; i++)
            {
                var voxelPos = IndexToVector(i);
                Vector3 worldPos = voxelPos + _position;
                VoxelMap[i] = new VoxelState(0);
                var ySimplex = (simplex.GetData(worldPos.X, worldPos.Z, 0.5f)) * 8f + 128;
                if (worldPos.Y <= ySimplex)
                {
                    VoxelMap[i] = new VoxelState()
                    {
                        Index = Randomizer.Range(1, 4)
                    };
                }
            }
            
            CreateMeshData();
            CreateMesh();
            
            _vertices.Clear();
            _vertices.Capacity = 0;
            _triangles.Clear();
            _triangles.Capacity = 0;
        })).Start();
    }

    public void CreateMeshData()
    {
        ClearMeshData();
        
        for (var z = 0; z < VoxelData.chunkSize.Z; z++)
        for (var y = 0; y < VoxelData.chunkSize.Y; y++)
        for (var x = 0; x < VoxelData.chunkSize.X; x++)
            UpdateMeshData(new Vector3(x, y, z));

        UpdateBlocks();
    }

    private bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkSize.X - 1 || y < 0 || y > VoxelData.chunkSize.Y - 1 || z < 0 ||
            z > VoxelData.chunkSize.Z - 1)
            return false;
        return true;
    }

    public void CreateMesh()
    {
        _trianglesArray = _triangles.ToArray();
        _verticesArray = _vertices.ToArray();
    }

    private void ClearMeshData()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();
        _normals.Clear();
        _colors.Clear();
        _lightBfsQueue.Clear();
        _blocksToUpdate.Clear();
    }

    private bool CheckVoxel(Vector3 pos)
    {
        int x = (int)pos.X;
        int y = (int)pos.Y;
        int z = (int)pos.Z;

        // If position is outside of this chunk...
        if (!IsVoxelInChunk(x, y, z)) return false;

        return true;
    }

    private void UpdateBlocks()
    {
        while (_blocksToUpdate.Count > 0)
        {
            var blockCurrent = _blocksToUpdate.Dequeue();
            
            for (var p = 0; p < 6; p++)
            {
                var newCheck = new Vector3Int((int)(blockCurrent.X + VoxelData.faceChecks[p].X),
                    (int)(blockCurrent.Y + VoxelData.faceChecks[p].Y),
                    (int)(blockCurrent.Z + VoxelData.faceChecks[p].Z));

                VoxelState neighbor = default;
                if (CheckVoxel(newCheck)) neighbor = VoxelMap[PosToIndex(newCheck)];

                if (neighbor.Index != 0) continue;
                
                for (var i = 0; i < 4; i++)
                {
                    var vertPos = blockCurrent + VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
                    var vertNorm = VoxelData.faceChecks[p];
                    
                    _vertices.Add(new VertexPositionColor(vertPos, new Color((vertNorm + Vector3.One) / 2f)));
                    _normals.Add(vertNorm);

                    float lightValue = 0;

                    _colors.Add(new Color(0, 0, 0, 0));
                }
                    
                if (p is 2 or 3)
                    AddTexture(0);
                else
                    AddTexture(0);

                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);

                _vertexIndex += 4;
            }
        }
    }

    private int PosToIndex(Vector3 pos)
    {
        return (int)(pos.X + VoxelData.chunkSize.X * (pos.Y + VoxelData.chunkSize.Y * pos.Z));
    }
    
    private int PosToIndex(Vector3Int pos)
    {
        return pos.X + VoxelData.chunkSize.X * (pos.Y + VoxelData.chunkSize.Y * pos.Z);
    }
    
    private int PosToIndex(int x, int y, int z)
    {
        return x + VoxelData.chunkSize.X * (y + VoxelData.chunkSize.Y * z);
    }

    private Vector3Int IndexToVector(int index)
    {
        int x = index % VoxelData.chunkSize.X;
        int y = (index / VoxelData.chunkSize.X) % VoxelData.chunkSize.Y;
        int z = index / (VoxelData.chunkSize.X * VoxelData.chunkSize.Y);
        return new Vector3Int(x, y, z);
    }

    private void UpdateMeshData(Vector3 pos)
    {
        if (VoxelMap[PosToIndex(pos)].Index == 0) return;
        
        // BLOCKS
        _blocksToUpdate.Enqueue(new Vector3Int(pos));
    }

    private void AddTexture(int blockIdTest = 0)
    {
        var rand = Randomizer.Range(0, 29);
        _uvs.Add(new Vector3(0, 0, blockIdTest));
        _uvs.Add(new Vector3(0, 1, blockIdTest));
        _uvs.Add(new Vector3(1, 0, blockIdTest));
        _uvs.Add(new Vector3(1, 1, blockIdTest));
    }

    public int GetBlockArrayIndex(Vector3 pos)
    {
        return (int)(VoxelData.chunkSize.Y * VoxelData.chunkSize.X * pos.Z + VoxelData.chunkSize.X * pos.Y + pos.X);
    }
}

public readonly struct ChunkCoord
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public Vector3 Vector => new Vector3(X, Y, Z);

    public ChunkCoord(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public ChunkCoord(Vector3 pos)
    {
        var posInt = new Vector3Int(pos.X.RoundToInt(), pos.Y.RoundToInt(), pos.Z.RoundToInt());

        X = posInt.X / VoxelData.chunkSize.X;
        Y = posInt.Y / VoxelData.chunkSize.Y;
        Z = posInt.Z / VoxelData.chunkSize.Z;
    }
}

[Serializable]
public struct VoxelState
{
    public int Index = 0;

    public VoxelState(int index)
    {
        this.Index = index;
    }
}