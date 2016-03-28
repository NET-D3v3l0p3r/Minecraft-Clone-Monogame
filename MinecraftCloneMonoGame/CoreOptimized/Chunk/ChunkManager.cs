using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using MinecraftClone.CoreII.Chunk.SimplexNoise;
using MinecraftClone.CoreII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace MinecraftClone.CoreII.Chunk
{
    public class ChunkManager
    {
        private int ChunkIndexCounter = -1;

        public static bool Generated { get; set; }
        public static bool UploadingShaderData { get; set; }
        public static bool PullingShaderData { get; set; }

        public static SimplexNoiseGenerator Generator { get; private set; }
        public ChunkOptimized[] Chunks { get; private set; }

        public static int Width { get; set; }
        public static int Depth { get; set; }

        public int SeaLevel { get; set; }
        public int Seed { get; set; }

        public static int MaximumRender { get; set; }

        public static int TotalRender { get; set; }
        public static int TotalUpdate { get; set; }

        public static int UpdatingChunks { get; set; }

        public void Start(int sea_level, int seed)
        {
            SeaLevel = sea_level;
            Seed = seed;
            Initialize();
        }

        public void Start(int sea_level, string seed)
        {
            SeaLevel = sea_level;
            Seed = seed.GetHashCode();
            Initialize();
        }

        private void Initialize()
        {
            Models.GlobalModels.IndexModelTuple.Add(0, Global.GlobalShares.GlobalContent.Load<Model>(@"Model\Cube"));
            Cube.Initialize();

            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Grass, new Vector2(Core.MapGenerator.GlobalShares.Grass, 0));
            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Dirt, new Vector2(Core.MapGenerator.GlobalShares.Dirt, 0));
            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Stone, new Vector2(Core.MapGenerator.GlobalShares.Stone, 0));
            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Water, new Vector2(Core.MapGenerator.GlobalShares.Water, 0));

            Chunks = new ChunkOptimized[Width * Depth];

            Generator = new SimplexNoise.SimplexNoiseGenerator(Seed, 1f / 256f, 1f / 256f, 1f / 256f, 1f / 256f);

            Generator.Octaves = 5;
            Generator.Factor = 0;

            ChunkOptimized.Indices = new int[16, 256, 16];

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    for (int k = 0; k < 256; k++)
                    {
                        ChunkOptimized.Indices[i, k, j] = (j * 16 * 256) + ((k) * 16) + i;
                    }
                }
            }

            new Thread(new ThreadStart(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int y = 0; y < Depth; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        UploadNewChunk(new ChunkOptimized(16, 256, 16, new Vector3(x * 15, 0, y * 15)));
                    }
                }
                sw.Stop();
                Console.WriteLine("DONE[MAP_GENERATION: {0} s]", sw.Elapsed.TotalSeconds);
            })).Start();
        }

        public void Update(GameTime gTime)
        {
            TotalUpdate = UpdatingChunks = 0;
            Parallel.For(0, Chunks.Length, new Action<int>((i) =>
            {
                if (Chunks[i] != null)
                    Chunks[i].Update(gTime);
            }));
        }

        public void RenderChunks()
        {
            TotalRender = MaximumRender = 0;
            for (int i = 0; i < Chunks.Length; i++)
            {
                if (Chunks[i] != null)
                    Chunks[i].Render();
            }
        }
        public void UploadNewChunk(ChunkOptimized chunk)
        {
            if (ChunkIndexCounter + 1 >= Chunks.Length)
                ChunkIndexCounter = -1;
            ChunkIndexCounter++;
            chunk.Generate();
            Chunks[ChunkIndexCounter] = chunk;
        }

        public void Remove(Ray r, float max_distance)
        {
            var Chunk = GetChunkArea(Camera3D.CameraPosition);
            if (Chunk != null)
                for (int i = 0; i < Chunk.RenderingCubes.Count; i++)
                {
                    int Index = Chunk.RenderingCubes[i];
                    if ((int?)Chunk.ChunkData[Index].BoundingBox.Intersects(r) < (int)max_distance)
                    {
                        //Chunk.DeleteShaderData(Index);
                        Chunk.RenderingCubes.RemoveAt(i);
                        Chunk.ChunkData[Index].Id = (int)Global.GlobalShares.Identification.DefautCubeStructure;
                        Chunk.Invalidate = true;
                        return;
                    }
                }
        }

        public ChunkOptimized GetChunkArea(Vector3 coordinates)
        {
            if (ChunkManager.Generated)
                foreach (var chunk in Chunks)
                    if (chunk.ChunkArea.Contains(coordinates) != ContainmentType.Disjoint)
                        return chunk;
            return null;
        }
    }
}
