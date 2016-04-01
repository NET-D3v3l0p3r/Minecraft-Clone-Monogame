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
        private static ChunkOptimized Old;
        private int ChunkIndexCounter = -1;

        private static int WidthCounter, DepthCounter;

        public static bool Generated { get; set; }
        public static double Progress { get; set; }

        public static bool UploadingShaderData { get; set; }
        public static bool PullingShaderData { get; set; }

        public static SimplexNoiseGenerator Generator { get; private set; }
        public static ChunkOptimized[] Chunks { get; private set; }

        public static List<int> IndexStack { get; private set; }

        public static ChunkOptimized CurrentChunk { get; private set; }

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

        public enum MoveDirection
        {
            North,
            East,
            South,
            West,
            NULL
            //TOOD: ADD NORTH-WEST[EAST] SOUTH-WEST[EAST]
        }

        public static MoveDirection MovedDirection { get; private set; }

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

            IndexStack = new List<int>();
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
            Generator.Factor = 200;
            Generator.Sealevel = SeaLevel;

            new Thread(new ThreadStart(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int y = 0; y < Depth; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        UploadNewChunk(new ChunkOptimized(new Vector3(x * ChunkOptimized.Width, 0, y * ChunkOptimized.Depth)));
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
                    //BoundingBoxRenderer.Render(Chunks[i].ChunkArea, Global.GlobalShares.GlobalDevice, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Red);   
                }
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
            float DistanceToCube = .0f;
            int Face = 0;

            if (CurrentChunk != null)
            {
                foreach (var Surrounding in CurrentChunk.SurroundingChunks)
                {
                    if (Surrounding != null && Surrounding.IndexRenderer != null)
                        for (int i = Surrounding.IndexRenderer.Count - 1; i >= 0; i--)
                        {
                            int Index = Surrounding.IndexRenderer[i];
                            if (BoundingBoxRenderer.IntersectRayVsBox(Surrounding.ChunkData[Index].PickingBox, r, out DistanceToCube, out Face))
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

        public static void GetChunkArea(Vector3 coordinates)
        {
            Old = CurrentChunk;
            if (ChunkManager.Generated)
                foreach (var chunk in Chunks)
                    if (chunk != null && chunk.ChunkArea.Contains(coordinates) == ContainmentType.Contains)
                    {
                        CurrentChunk = chunk;

                        if (CurrentChunk.SurroundingChunks[0] != null)
                        {
                            if (CurrentChunk.SurroundingChunks[0].Equals(Old))
                                MovedDirection = MoveDirection.West;
                        }
                        else CurrentChunk.ParseSurroundingChunks();

                        if (CurrentChunk.SurroundingChunks[1] != null)
                        {
                            if (CurrentChunk.SurroundingChunks[1].Equals(Old))
                                MovedDirection = MoveDirection.East;
                        }
                        else CurrentChunk.ParseSurroundingChunks();

                        if (CurrentChunk.SurroundingChunks[2] != null)
                        {
                            if (CurrentChunk.SurroundingChunks[2].Equals(Old))
                                MovedDirection = MoveDirection.North;
                        }
                        else CurrentChunk.ParseSurroundingChunks();

                        if (CurrentChunk.SurroundingChunks[3] != null)
                        {
                            if (CurrentChunk.SurroundingChunks[3].Equals(Old))
                                MovedDirection = MoveDirection.South;
                        }
                        else CurrentChunk.ParseSurroundingChunks();

                        GenerateNewChunks(MovedDirection);
                        break;
                    }
        }

        #region "OLD"

        //public void RequestTerrainGenerationZ()
        //{
        //    ChunkOptimized[] TemporaryChunkMap = Chunks.ToArray();
        //    ChunkGenerationQueue.Clear();
        //    IsRunningTask = false;

        //    for (int i = 0; i < Width; i++)
        //    {
        //        int Index = i + 0 * Width;
        //        if (!Camera3D.GlobalBox.Intersects(Chunks[Index].ChunkArea))
        //        {
        //            if (!IsRunningTask)
        //            {
        //                IsRunningTask = true;
        //                DepthCounter++;
        //            }
        //            TemporaryChunkMap[Index].Dispose();
        //            TemporaryChunkMap[Index] = new
        //                ChunkOptimized(new Vector3(i * ChunkOptimized.Width, 0, (Depth - 1) * ChunkOptimized.Depth),
        //                new Vector3(0, 0, DepthCounter * ChunkOptimized.Depth));

        //            for (int j = 0; j < Depth - 1; j++)
        //            {
        //                ChunkOptimized Old = TemporaryChunkMap[i + (j + 1) * Width];
        //                Old.ChunkTranslation -= new Vector3(0, 0, ChunkOptimized.Depth);
        //                Old.ParseSurroundingChunks();
        //                TemporaryChunkMap[i + (j + 1) * Width] = TemporaryChunkMap[i + j * Width];
        //                TemporaryChunkMap[i + j * Width] = Old;
        //            }
        //            ChunkGenerationQueue.Add(i + (Depth - 1) * Width);
        //        }
        //    }
        //    IsRunningTask = false;
        //    for (int i = 0; i < Width; i++)
        //    {
        //        int Index = i + (Depth - 1) * Width;
        //        if (!Camera3D.GlobalBox.Intersects(Chunks[Index].ChunkArea))
        //        {
        //            if (!IsRunningTask)
        //            {
        //                IsRunningTask = true;
        //                DepthCounter--;
        //            }
        //            TemporaryChunkMap[Index].Dispose();
        //            TemporaryChunkMap[Index] = new
        //                ChunkOptimized(new Vector3(i * ChunkOptimized.Width, 0, 0),
        //                new Vector3(0, 0, DepthCounter * ChunkOptimized.Depth));

        //            for (int j = Depth - 1; j >= 1; j--)
        //            {
        //                ChunkOptimized Old = TemporaryChunkMap[i + (j - 1) * Width];
        //                Old.ChunkTranslation += new Vector3(0, 0, ChunkOptimized.Depth);
        //                Old.ParseSurroundingChunks();
        //                TemporaryChunkMap[i + (j - 1) * Width] = TemporaryChunkMap[i + j * Width];
        //                TemporaryChunkMap[i + j * Width] = Old;
        //            }
        //            ChunkGenerationQueue.Add(i + 0 * Width);
        //        }
        //    }

        //    FlushChunkManager(TemporaryChunkMap);
        //}
        //public void RequestTerrainGenerationX()
        //{
        //    ChunkOptimized[] TemporaryChunkMap = Chunks.ToArray();
        //    ChunkGenerationQueue.Clear();
        //    IsRunningTask = false;

        //    for (int i = 0; i < Depth; i++)
        //    {
        //        int Index = 0 + i * Width;
        //        if (!Camera3D.GlobalBox.Intersects(Chunks[Index].ChunkArea))
        //        {
        //            if (!IsRunningTask)
        //            {
        //                IsRunningTask = true;
        //                WidthCounter++;
        //            }
        //            TemporaryChunkMap[Index].Dispose();
        //            TemporaryChunkMap[Index] = new ChunkOptimized(new Vector3((Width - 1) * ChunkOptimized.Width, 0, i * ChunkOptimized.Depth),
        //                new Vector3(WidthCounter * ChunkOptimized.Width, 0, 0));

        //            for (int j = 0; j < Width - 1; j++)
        //            {
        //                ChunkOptimized Old = TemporaryChunkMap[(j + 1) + i * Width];
        //                Old.ChunkTranslation -= new Vector3(ChunkOptimized.Width, 0, 0);
        //                Old.ParseSurroundingChunks();
        //                TemporaryChunkMap[(j + 1) + i * Width] = TemporaryChunkMap[j + i * Width];
        //                TemporaryChunkMap[j + i * Width] = Old;

        //            }

        //            ChunkGenerationQueue.Add((Width - 1) + i * Width);
        //        }
        //    }

        //    for (int i = 0; i < Depth; i++)
        //    {
        //        int Index = (Width - 1) + i * Width;
        //        if (!Camera3D.GlobalBox.Intersects(Chunks[Index].ChunkArea))
        //        {
        //            if (!IsRunningTask)
        //            {
        //                IsRunningTask = true;
        //                WidthCounter--;
        //            }
        //            TemporaryChunkMap[Index].Dispose();
        //            TemporaryChunkMap[Index] = new ChunkOptimized(new Vector3(0, 0, i * ChunkOptimized.Depth),
        //                new Vector3(WidthCounter * ChunkOptimized.Width, 0, 0));

        //            for (int j = Width - 1; j >= 1; j--)
        //            {
        //                ChunkOptimized Old = TemporaryChunkMap[(j - 1) + i * Width];
        //                Old.ChunkTranslation += new Vector3(ChunkOptimized.Width, 0, 0);
        //                Old.ParseSurroundingChunks();
        //                TemporaryChunkMap[(j - 1) + i * Width] = TemporaryChunkMap[j + i * Width];
        //                TemporaryChunkMap[j + i * Width] = Old;
        //            }
        //            ChunkGenerationQueue.Add(0 + i * Width);
        //        }
        //    }

        //    FlushChunkManager(TemporaryChunkMap);

        //}

        //public void FlushChunkManager(ChunkOptimized[] TemporaryChunkMap)
        //{
        //    if (ChunkGenerationQueue.Count > 0)
        //    {
        //        Chunks = TemporaryChunkMap;
        //        Parallel.ForEach(ChunkGenerationQueue, new Action<int>(index =>
        //        {
        //            Chunks[index].Generate();
        //        }));
        //        TemporaryChunkMap = new ChunkOptimized[0];
        //    }
        //}

        #endregion
        public static void GenerateNewChunks(MoveDirection direction)
        {
            //South : + Z
            //North : - Z

            //EAST : + X
            //WEST : - Z
            IndexStack.Clear();
            ChunkOptimized[] TemporaryChunks = Chunks.ToArray();

            switch (direction)
            {
                case MoveDirection.North:
                    ChunkOptimized.WorldTranslation-= new Vector3(0, 0, ChunkOptimized.Depth);
                    for (int i = 0; i < Width; i++)
                    {
                        int Index = i + (Depth - 1) * Width;
                        TemporaryChunks[Index].Dispose(); // FREE MEMORY
                        TemporaryChunks[Index] = new ChunkOptimized(new Vector3(i * ChunkOptimized.Width, 0, 0));
                        //SWAP CHUNKS
                        for (int j = Depth - 1; j > 0; j--)
                        {
                            ChunkOptimized OldChunk = TemporaryChunks[i + (j - 1) * Width];
                            OldChunk.ChunkTranslation += new Vector3(0, 0, ChunkOptimized.Depth);
                            TemporaryChunks[i + (j - 1) * Width] = TemporaryChunks[i + j * Width];
                            TemporaryChunks[i + j * Width] = OldChunk;

                        }
                        IndexStack.Add(i + 0 * Width);
                    }
                    break;

                case MoveDirection.South:
                    ChunkOptimized.WorldTranslation += new Vector3(0, 0, ChunkOptimized.Depth);
                    for (int i = 0; i < Width; i++)
                    {
                        int Index = i + 0 * Width;
                        TemporaryChunks[Index].Dispose(); // FREE MEMORY
                        TemporaryChunks[Index] = new ChunkOptimized(new Vector3(i * ChunkOptimized.Width, 0, (Depth - 1) * ChunkOptimized.Depth));
                        //SWAP CHUNKS
                        for (int j = 0; j < Depth - 1; j++)
                        {
                            ChunkOptimized OldChunk = TemporaryChunks[i + (j + 1) * Width];
                            OldChunk.ChunkTranslation -= new Vector3(0, 0, ChunkOptimized.Depth);
                            TemporaryChunks[i + (j + 1) * Width] = TemporaryChunks[i + j * Width];
                            TemporaryChunks[i + j * Width] = OldChunk;

                        }
                        IndexStack.Add(i + (Depth - 1) * Width);
                    }
                    break;

                case MoveDirection.East:
                    ChunkOptimized.WorldTranslation += new Vector3(ChunkOptimized.Width, 0, 0);
                    for (int j = 0; j < Depth; j++)
                    {
                        int Index = 0 + j * Width;
                        TemporaryChunks[Index].Dispose(); // FREE MEMORY
                        TemporaryChunks[Index] = new ChunkOptimized(new Vector3((Width - 1) * ChunkOptimized.Width, 0, j * ChunkOptimized.Depth));
                        //SWAP CHUNKS
                        for (int i = 0; i < Width - 1; i++)
                        {
                            ChunkOptimized OldChunk = TemporaryChunks[(i + 1) + j * Width];
                            OldChunk.ChunkTranslation -= new Vector3(ChunkOptimized.Width, 0, 0);
                            TemporaryChunks[(i + 1) + j * Width] = TemporaryChunks[i + j * Width];
                            TemporaryChunks[i + j * Width] = OldChunk;
                        }
                        IndexStack.Add((Width - 1) + j * Width);
                    }
                    break;

                case MoveDirection.West:
                    ChunkOptimized.WorldTranslation -= new Vector3(ChunkOptimized.Width, 0, 0);
                    for (int j = 0; j < Depth; j++)
                    {
                        int Index = (Width - 1) + j * Width;
                        TemporaryChunks[Index].Dispose(); // FREE MEMORY
                        TemporaryChunks[Index] = new ChunkOptimized(new Vector3(0, 0, j * ChunkOptimized.Depth));
                        //SWAP CHUNKS
                        for (int i = Width - 1; i > 0; i--)
                        {
                            ChunkOptimized OldChunk = TemporaryChunks[(i - 1) + j * Width];
                            OldChunk.ChunkTranslation += new Vector3(ChunkOptimized.Width, 0, 0);
                            TemporaryChunks[(i - 1) + j * Width] = TemporaryChunks[i + j * Width];
                            TemporaryChunks[i + j * Width] = OldChunk;
                        }
                        IndexStack.Add(0 + j * Width);
                    }
                    break;
            }


            if (IndexStack.Count > 0)
            {
                Chunks = TemporaryChunks;
                Parallel.ForEach(IndexStack, index =>
                {
                    Chunks[index].Generate();
                });
                TemporaryChunks = new ChunkOptimized[0];
                MovedDirection = MoveDirection.NULL;
            }
        }
    }
}
