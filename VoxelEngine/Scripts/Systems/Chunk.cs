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
    public readonly VoxelState[] VoxelMap =
        new VoxelState[VoxelData.chunkSize.X * VoxelData.chunkSize.Y * VoxelData.chunkSize.Z];
    
    private VertexBufferBinding _binding;
    private IndexBuffer _ib;

    private readonly Vector3 _position;

    private int _vertexCount = 0;
    private int _vertexIndex;

    // Triangle Indices
    private static int[] _triangles;
    // private readonly Vector3[] _uvs;
    // private readonly Vector3[] _normals;

    private readonly Queue<Vector3> _lightBfsQueue = new();

    private readonly Queue<Vector3Int> _blocksToUpdate = new();

    private readonly VoxelWorld _world;

    private readonly GraphicsDevice _graphicsDevice;

    public Chunk(ChunkCoord coord, VoxelWorld world, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        Coord = coord;
        _world = world;

        if (_triangles == null)
        {
            int[] pattern = { 0, 1, 2, 2, 1, 3 };
            _triangles = new int[VoxelData.ChunkSizeTotal * 36];
            for (int i = 0; i < _triangles.Length; i++)
            {
                _triangles[i] = pattern[i % 6] + ((int)MathF.Floor(i / 6f) * 4);
            }
        }

        _position = Coord.Vector * VoxelData.chunkSize;

        GenerateVoxelMap();
    }

    public void Draw()
    {
        if (_vertexCount <= 0) return;
        if (_binding.VertexBuffer == null) return;
        if (_ib == null) return;
        // if (_verticesArray.Length < 3) return;
        var bounding = new BoundingBox(_position, _position + VoxelData.chunkSize);
        if (Game1.BoundingFrustum.Contains(bounding) == ContainmentType.Disjoint) return;

        _graphicsDevice.SetVertexBuffers(_binding);
        _graphicsDevice.Indices = _ib;
        VoxelWorld.Effect.World = Matrix.CreateTranslation(_position);
        VoxelWorld.EffectPass.Apply();
        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VoxelData.ChunkSizeTotal * 24);
        // _graphicsDevice.DrawUserIndexedPrimitives(
        //     PrimitiveType.TriangleList,
        //     _vertices, 0, _vertices.Length,
        //     _triangles, 0, _triangles.Length / 3
        // );
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
                var ySimplex = (simplex.GetData(worldPos.X, worldPos.Z, 0.5f)) * 8f +
                               (VoxelData.chunkSize.Y * (VoxelData.worldSizeInChunks.Y - 1));
                if (worldPos.Y <= ySimplex)
                {
                    VoxelMap[i] = new VoxelState()
                    {
                        Index = (byte)Randomizer.Range(1, 4)
                    };
                }
            }

            CreateMeshData();

            var vertices = UpdateBlocks();

            CreateMesh();
            
            VertexBuffer vb = new VertexBuffer(_graphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length,
                BufferUsage.WriteOnly);
            vb.SetData(vertices);
            _binding = new VertexBufferBinding(vb);
            vb.Dispose();

            _ib = new IndexBuffer(_graphicsDevice, typeof(int), _triangles.Length, BufferUsage.WriteOnly);
            _ib.SetData(_triangles);

            // _vertices = Array.Empty<VertexPositionColor>();
        })).Start();
    }

    public void CreateMeshData()
    {
        ClearMeshData();
        
        for (var z = 0; z < VoxelData.chunkSize.Z; z++)
        for (var y = 0; y < VoxelData.chunkSize.Y; y++)
        for (var x = 0; x < VoxelData.chunkSize.X; x++)
            UpdateMeshData(new Vector3(x, y, z));
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
        
    }

    private void ClearMeshData()
    {
        _vertexIndex = 0;
        // _vertices.Clear();
        // _triangles.Clear();
        // _uvs.Clear();
        // _normals.Clear();
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

    private VertexPositionColor[] UpdateBlocks()
    {
        var result = new VertexPositionColor[VoxelData.ChunkSizeTotal * 24];
        while (_blocksToUpdate.Count > 0)
        {
            var blockCurrent = _blocksToUpdate.Dequeue();
            var blockIndex = PosToIndex(blockCurrent);

            _vertexCount = 0;
            int vertStride = 6 * 4;
            
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
                    
                    result[blockIndex * vertStride + _vertexCount] = (new VertexPositionColor(vertPos, new Color((vertNorm + Vector3.One) / 2f)));
                    _vertexCount++;
                    
                    // _normals.Add(vertNorm);

                    float lightValue = 0;
                }
                    
                // if (p is 2 or 3)
                //     AddTexture(0);
                // else
                //     AddTexture(0);

                // _triangles.Add(_vertexIndex);
                // _triangles.Add(_vertexIndex + 1);
                // _triangles.Add(_vertexIndex + 2);
                // _triangles.Add(_vertexIndex + 2);
                // _triangles.Add(_vertexIndex + 1);
                // _triangles.Add(_vertexIndex + 3);

                // _vertexIndex += 4;
            }
        }

        return result;
    }

    private int PosToIndex(Vector3 pos)
    {
        return (int)(pos.X + VoxelData.chunkSize.X * (pos.Y + VoxelData.chunkSize.Y * pos.Z));
    }
    
    private int  PosToIndex(Vector3Int pos)
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
        // _uvs.Add(new Vector3(0, 0, blockIdTest));
        // _uvs.Add(new Vector3(0, 1, blockIdTest));
        // _uvs.Add(new Vector3(1, 0, blockIdTest));
        // _uvs.Add(new Vector3(1, 1, blockIdTest));
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
    public byte Index = 0;

    public VoxelState(byte index)
    {
        this.Index = index;
    }
}