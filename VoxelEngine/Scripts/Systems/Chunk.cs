using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;

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
    private VertexBuffer _vb;

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

    public float _visibilityScale = 0f;

    private bool _isDirty = false;

    private Random _random;

    public Chunk(ChunkCoord coord, VoxelWorld world, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _random = new Random();

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
        _ib = new IndexBuffer(_graphicsDevice, typeof(int), _triangles.Length, BufferUsage.WriteOnly);
        _ib.SetData(_triangles);

        _position = Coord.Vector * VoxelData.chunkSize;
    }

    public void Draw()
    {
        if (_vertexCount <= 0) return;
        if (_binding.VertexBuffer == null) return;
        if (_ib == null) return;
        var bounding = new BoundingBox(_position, _position + VoxelData.chunkSize);
        if (Game1.BoundingFrustum.Contains(bounding) == ContainmentType.Disjoint) return;

        _graphicsDevice.SetVertexBuffers(_binding);
        _graphicsDevice.Indices = _ib;
        _world.ParamWorldMatrix.SetValue(Matrix.CreateScale(_visibilityScale) * Matrix.CreateTranslation(_position));
        VoxelWorld.EffectPass.Apply();
        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VoxelData.ChunkSizeTotal * 36);
    }

    public Task GenerateVoxelMap()
    {
        ClearMeshData();

        var simplex = new SeamlessSimplexNoise(VoxelData.chunkSize.X * VoxelData.worldSizeInChunks.X, 1337);

        return Task.Factory.StartNew((() =>
        {
            for (var i = 0; i < VoxelMap.Length; i++)
            {
                var voxelPos = IndexToVector(i);
                Vector3 worldPos = voxelPos + _position;
                VoxelMap[i] = new VoxelState(0);
                var ySimplex = (simplex.GetData(worldPos.X, worldPos.Z, 0.5f)) * 8f +
                               (VoxelData.chunkSize.Y * VoxelData.worldSizeInChunks.Y - 1);
                if (worldPos.Y <= ySimplex)
                {
                    VoxelMap[i] = new VoxelState()
                    {
                        Index = (byte)Randomizer.Range(1, 4)
                    };
                }
            }

            CreateMeshData();
        }));
    }

    public void SetVoxel(Vector3 pos, byte index)
    {
        _isDirty = true;
        VoxelMap[PosToIndex(pos)].Index = index;
    }

    public void Update()
    {
        if (!_isDirty) return;
        _isDirty = false;
        CreateMesh();
    }

    public void CreateMeshData()
    {
        ClearMeshData();
        
        for (var z = 0; z < VoxelData.chunkSize.Z; z++)
        for (var y = 0; y < VoxelData.chunkSize.Y; y++)
        for (var x = 0; x < VoxelData.chunkSize.X; x++)
            UpdateMeshData(new Vector3(x, y, z));
    }

    public void CreateMesh()
    {
        var vertices = UpdateBlocks();
        _vb ??= new VertexBuffer(_graphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length,
            BufferUsage.None);
        _vb.SetData(vertices);
        _binding = new VertexBufferBinding(_vb);
        // Cannot dispose of vb here. Binding stores a reference to it, so it must stay in memory

        _visibilityScale = 1f;
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
    
    private bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkSize.X - 1 || y < 0 || y > VoxelData.chunkSize.Y - 1 || z < 0 ||
            z > VoxelData.chunkSize.Z - 1)
            return false;
        return true;
    }
    
    private bool IsVoxelInChunk(Vector3Int pos)
    {
        int x = pos.X;
        int y = pos.Y;
        int z = pos.Z;
        return (x >= 0 && x < VoxelData.chunkSize.X && 
                y >= 0 && y < VoxelData.chunkSize.Y && 
                z >= 0 && z < VoxelData.chunkSize.Z);
    }
    
    private bool IsVoxelInChunk(int index)
    {
        return index >= 0 && index < VoxelData.ChunkSizeTotal;
    }

    public bool CheckVoxelOccupied(Vector3Int pos, bool oobOccupied = false)
    {
        bool isThere = IsVoxelInChunk(pos);
        if (!isThere)
        {
            var targetChunkCoord = _world.GetChunkCoord(pos + _position);
            if (!_world.IsChunkInWorld(targetChunkCoord)) return oobOccupied;
            Chunk targetChunk = _world.GetChunk(targetChunkCoord);
            Vector3Int targetPos = pos % VoxelData.chunkSize;
            int chunkIndex = (targetChunk.PosToIndex(targetPos));
            return targetChunk.VoxelMap[chunkIndex].Index > 0;
            // return false;
        }
        return VoxelMap[PosToIndex(pos)].Index > 0;
    }

    private VertexPositionColor[] UpdateBlocks()
    {
        var result = new VertexPositionColor[VoxelData.ChunkSizeTotal * 24];
        int vertStride = 6 * 4;
        _vertexCount = 0;

        _random = new Random(Coord.X * Coord.Y * Coord.Z);

        for (int blockIndex = 0; blockIndex < VoxelMap.Length; blockIndex++)
        {
            var blockCurrent = VoxelMap[blockIndex];
            double randomVal = _random.NextDouble();
            if (blockCurrent.Index == 0) continue;
            
            var blockPos = IndexToVector(blockIndex);
            int thisVertexCount = 0;
            float randomBlockBrightness = MathHelper.Lerp(0.95f, 1.0f,(float)randomVal);
            
            for (var faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                var newCheck = new Vector3Int((int)(blockPos.X + VoxelData.faceChecks[faceIndex].X),
                    (int)(blockPos.Y + VoxelData.faceChecks[faceIndex].Y),
                    (int)(blockPos.Z + VoxelData.faceChecks[faceIndex].Z));

                if (CheckVoxelOccupied(newCheck)) continue;
                
                var vertNorm = VoxelData.faceChecks[faceIndex];
                Color normalColor = new Color((vertNorm + Vector3.One) / 2f);
                float horizontalBrightness = 1 - MathF.Abs(vertNorm.X) * 0.035f - MathF.Abs(vertNorm.Z) * 0.015f;

                for (var vertexIndex = 0; vertexIndex < 4; vertexIndex++)
                {
                    var vertPos = blockPos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIndex, vertexIndex]];

                    float avgAo = 1f;
                    Vector3Int side1 = new Vector3Int(newCheck + VoxelData.voxelAoChecks[faceIndex * 4 + vertexIndex, 0]);
                    Vector3Int side2 = new Vector3Int(newCheck + VoxelData.voxelAoChecks[faceIndex * 4 + vertexIndex, 1]);
                    Vector3Int corner = new Vector3Int(side1 + side2 - newCheck);
                    if (CheckVoxelOccupied(side1) && CheckVoxelOccupied(side2))
                    {
                        avgAo = 3;
                    }
                    else
                    {
                        avgAo = ((CheckVoxelOccupied(side1) ? 1 : 0) +
                                 (CheckVoxelOccupied(side2) ? 1 : 0) +
                                 (CheckVoxelOccupied(corner) ? 1 : 0));
                    }
                    avgAo *= 0.75f;
                    avgAo /= 3f;
                    avgAo = 1f - avgAo;
                    avgAo = MathF.Min(avgAo, 1f);
                    
                    result[blockIndex * vertStride + thisVertexCount] = (new VertexPositionColor(vertPos, Color.DarkGray *
                        (avgAo * horizontalBrightness * randomBlockBrightness)));
                    thisVertexCount++;
                    _vertexCount++;
                }
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
    
    private Vector3Int GetNeighborPosition(int currentIndex, int offsetX, int offsetY, int offsetZ)
    {
        int x = currentIndex % VoxelData.chunkSize.X + offsetX;
        int y = (currentIndex / VoxelData.chunkSize.X) % VoxelData.chunkSize.Y + offsetY;
        int z = currentIndex / (VoxelData.chunkSize.X * VoxelData.chunkSize.Y) + offsetZ;
        return new Vector3Int(x, y, z);
    }
    
    public int[] GetCornerAttachedIndices(Vector3Int cornerPosition)
    {
        int[] attachedIndices = new int[8];

        // Calculate the offsets for the 8 corner neighbors
        int[] xOffset = { 0, 1, 0, 1, 0, 1, 0, 1 };
        int[] yOffset = { 0, 0, 1, 1, 0, 0, 1, 1 };
        int[] zOffset = { 0, 0, 0, 0, 1, 1, 1, 1 };

        for (int i = 0; i < 8; i++)
        {
            Vector3Int neighborPosition = new Vector3Int(
                cornerPosition.X + xOffset[i],
                cornerPosition.Y + yOffset[i],
                cornerPosition.Z + zOffset[i]
            );

            attachedIndices[i] = PosToIndex(neighborPosition);
        }

        return attachedIndices;
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