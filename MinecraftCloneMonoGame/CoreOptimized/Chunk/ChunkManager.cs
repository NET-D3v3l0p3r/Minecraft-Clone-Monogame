using Core.MapGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using MinecraftClone.CoreII.Chunk.SimplexNoise;
using MinecraftClone.CoreII.Models;
using MinecraftCloneMonoGame.CoreOptimized.Global;
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
        public static double Progress { get; set; }

        public static bool UploadingShaderData { get; set; }
        public static bool PullingShaderData { get; set; }

        public static SimplexNoiseGenerator Generator { get; private set; }
        public static ChunkOptimized[] Chunks { get; private set; }

        public static int Width { get; set; }
        public static int Depth { get; set; }

        public static int SeaLevel { get; set; }
        public static int Seed { get; set; }

        public static int MaximumRender { get; set; }

        public static int TotalRender { get; set; }
        public static int TotalUpdate { get; set; }

        public static int RenderingChunks { get; set; }
        public static int UpdatingChunks { get; set; }

        public static int[, ,] Indices { get; set; }

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

            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Grass, new Vector2(GlobalShares.Grass, 0));
            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Dirt, new Vector2(GlobalShares.Dirt, 0));
            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Stone, new Vector2(GlobalShares.Stone, 0));
            Models.GlobalModels.IndexTextureTuple.Add((int)Global.GlobalShares.Identification.Water, new Vector2(GlobalShares.Water, 0));

            Indices = new int[ChunkOptimized.Width, ChunkOptimized.Height, ChunkOptimized.Depth];

            for (int i = 0; i < ChunkOptimized.Width; i++)
            {
                for (int j = 0; j < ChunkOptimized.Depth; j++)
                {
                    for (int k = 0; k < ChunkOptimized.Height; k++)
                    {
                        Indices[i, k, j] = (j * ChunkOptimized.Width * ChunkOptimized.Height) + ((k) * ChunkOptimized.Depth) + i;
                    }
                }
            }

            RunGeneration(Seed);
        }

        public void RunGeneration(int seed)
        {
            Chunks = new ChunkOptimized[Width * Depth];
            Seed = seed;

            Generator = new SimplexNoise.SimplexNoiseGenerator(Seed, 1f / 512f, 1f / 512f, 1f / 512f, 1f / 512f);
            Generator.Octaves = 5;
            Generator.Factor = 230;

            new Thread(new ThreadStart(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int y = 0; y < Depth; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        UploadNewChunk(new ChunkOptimized( new Vector3(x * ChunkOptimized.Width, 0, y * ChunkOptimized.Depth)));
                    }
                }
                Parallel.ForEach(Chunks, new Action<ChunkOptimized>(Chunk =>
                {
                    Chunk.Generate();
                }));
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
            TotalRender = MaximumRender = RenderingChunks = 0;
            for (int i = 0; i < Chunks.Length; i++)
            {
                if (Chunks[i] != null && Camera3D.ViewFrustum.Contains(Chunks[i].ChunkArea) != ContainmentType.Disjoint)
                {
                    Chunks[i].Render();
                    RenderingChunks++;
                }
                    //BoundingBoxRenderer.Render(Chunks[i].ChunkArea, Global.GlobalShares.GlobalDevice, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Black);   
            }
        }
        public void UploadNewChunk(ChunkOptimized chunk)
        {
            if (ChunkIndexCounter + 1 >= Chunks.Length)
                ChunkIndexCounter = -1;
            ChunkIndexCounter++;
            Chunks[ChunkIndexCounter] = chunk;
        }

        private IEnumerable<Profile> GetAllIntersectingEntities(Ray r)
        {
            var Chunk = GetChunkArea(Camera3D.CameraPosition);

            float DistanceToCube = .0f;
            int Face = 0;

            if (Chunk != null)
            {
                foreach (var Surrounding in Chunk.SurroundingChunks)
                {
                    if (Surrounding != null)
                        for (int i = Surrounding.IndexRenderer.Count - 1; i >= 0; i--)
                        {
                            int Index = Surrounding.IndexRenderer[i];
                            if (BoundingBoxRenderer.IntersectRayVsBox(Surrounding.ChunkData[Index].BoundingBox, r, out DistanceToCube, out Face))
                                yield return new Profile(Surrounding, Index, Face, DistanceToCube);
                        }
                }
            }

        }
        /// <summary>
        /// Gets the targeted DefaultCubeStructure as Profile{Structure}
        /// <para />Notice: Profile is nullable.
        /// </summary>
        /// <param name="max_dist"></param>
        /// <returns></returns>
        public Profile? GetFocusedCube(float max_dist)
        {
            var entities = GetAllIntersectingEntities(Camera3D.Ray);
            if (entities.Count() > 0)
            {
                var entity = entities.ToList().OrderBy(p => (p.AABB.Min - Camera3D.CameraPosition).Length()).ToList()[0];
                if (entity.Distance <= max_dist)
                    return entity;
            }
            return null;
        }

        public ChunkOptimized GetChunkArea(Vector3 coordinates)
        {
            if (ChunkManager.Generated)
                foreach (var chunk in Chunks)
                    if (chunk.ChunkArea.Contains(coordinates) == ContainmentType.Contains)
                        return chunk;
            return null;
        }
    }
}
