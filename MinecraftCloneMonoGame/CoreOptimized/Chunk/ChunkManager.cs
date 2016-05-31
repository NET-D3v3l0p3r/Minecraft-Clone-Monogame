using Core.MapGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using MinecraftClone.CoreII.Chunk.SimplexNoise;
using MinecraftClone.CoreII.Models;
using MinecraftCloneMonoGame.CoreOptimized.Chunk.DiamondSquare;
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

        public static bool Generated { get; set; }
        public static double Progress { get; set; }

        public static bool UploadingShaderData { get; set; }
        public static bool PullingShaderData { get; set; }

        public static SimplexNoiseGenerator TerrainGeneratorSimplex { get; private set; }
        public static DiamondSquare TerrainGeneratorDiamondSquare { get; private set; }

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

        public enum MoveDirection
        {
            North,
            East,
            South,
            West,
            NULL
            //TOOD: ADD NORTH-WEST[EAST] SOUTH-WEST[EAST]
        }
        public enum GeneratorAlgorithm
        {
            SimplexNoise,
            DiamondSquare
        };

        public static GeneratorAlgorithm Algorithm { get; set; }
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

        public void Initialize()
        {
            Models.GlobalModels.IndexModelTuple.Add(0, Global.GlobalShares.GlobalContent.Load<Model>(@"Model\Cube"));
            Cube.Initialize();

            Models.GlobalModels.IndexTextureTuple.Add((short)Global.GlobalShares.Identification.Grass, GlobalShares.Grass);
            Models.GlobalModels.IndexTextureTuple.Add((short)Global.GlobalShares.Identification.Dirt, GlobalShares.Dirt);
            Models.GlobalModels.IndexTextureTuple.Add((short)Global.GlobalShares.Identification.Stone, GlobalShares.Stone);
            Models.GlobalModels.IndexTextureTuple.Add((short)Global.GlobalShares.Identification.Water, GlobalShares.Water);

            IndexStack = new List<int>();

            Chunks = new ChunkOptimized[Width * Depth];

            TerrainGeneratorSimplex = new SimplexNoise.SimplexNoiseGenerator(Seed, 1f / 512f, 1f / 512f, 1f / 512f, 1f / 512f);
            TerrainGeneratorSimplex.Octaves = 5;
            TerrainGeneratorSimplex.Factor = 200;
            TerrainGeneratorSimplex.Sealevel = SeaLevel;
        }

        public void RunGeneration()
        {  
            new Thread(new ThreadStart(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int y = 0; y < Depth; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        UploadNewChunk(new ChunkOptimized(new Vector3(x * ChunkOptimized.Width, 0, y * ChunkOptimized.Depth), (ushort)(x + y * Width)));
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

        private static IEnumerable<Profile> GetAllIntersectingEntities(Ray r)
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
                            if (Surrounding.ChunkData[Surrounding.IndexRenderer[i]].Id != (short)Global.GlobalShares.Identification.Water &&
                                Surrounding.ChunkData[Surrounding.IndexRenderer[i]].Id != -1 &&
                                BoundingBoxRenderer.IntersectRayVsBox(Surrounding.ChunkData[Index].PickingBox, r, out DistanceToCube, out Face))
                                yield return new Profile(Surrounding, (int)Index, Face, DistanceToCube);
                        }
                }
            }
        }

        private static IEnumerable<Profile> GetAllIntersectingEntitiesLocalAll(Ray r)
        {
            float DistanceToCube = .0f;
            int Face = 0;
            if (CurrentChunk != null)
            {
                for (int i = CurrentChunk.IndexRenderer.Count - 1; i >= 0; i--)
                {
                    int Index = CurrentChunk.IndexRenderer[i];
                    float? rParam = CurrentChunk.ChunkData[Index].PickingBox.Intersects(r);
                    if (rParam.HasValue)
                        yield return new Profile(CurrentChunk, (int)Index, -1, rParam);
                }

            }

        }

        /// <summary>
        /// Gets the targeted DefaultCubeStructure as Profile{Structure}
        /// <para />Notice: Profile is nullable.
        /// </summary>
        /// <param name="max_dist"></param>
        /// <returns></returns>
        public static Profile? GetFocusedCube(float max_dist)
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

        public static Profile? GetFocusedCubeSpecified(float max_dist, Ray r, int id)
        {
            var entities = GetAllIntersectingEntitiesLocalAll(r);
            if (entities.Count() > 0)
            {
                var entity = entities.ToList().OrderBy(p => (p.AABB.Min - Camera3D.CameraPosition).Length()).ToList()[0];
                if (entity.Distance <= max_dist && CurrentChunk.ChunkData[entity.Index].Id == id)
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
                        if (Algorithm != GeneratorAlgorithm.DiamondSquare)
                            GenerateNewChunks(MovedDirection);
                        break;
                    }
        }
        public static void GenerateNewChunks(MoveDirection direction)
        {
            //South : + Z
            //North : - Z

            //EAST : + X
            //WEST : - Z
            IndexStack.Clear();
            switch (direction)
            {
                case MoveDirection.North:
                    ChunkOptimized.WorldTranslation-= new Vector3(0, 0, ChunkOptimized.Depth);
                    for (int i = 0; i < Width; i++)
                    {
                        int Index = i + (Depth - 1) * Width;
                        Chunks[Index].Dispose(); // FREE MEMORY
                        Chunks[Index] = new ChunkOptimized(new Vector3(i * ChunkOptimized.Width, 0, 0), (ushort)Index);
                        //SWAP CHUNKS
                        for (int j = Depth - 1; j > 0; j--)
                        {
                            ChunkOptimized OldChunk = Chunks[i + (j - 1) * Width];
                            OldChunk.ChunkTranslation += new Vector3(0, 0, ChunkOptimized.Depth);
                            Chunks[i + (j - 1) * Width] = Chunks[i + j * Width];
                            Chunks[i + j * Width] = OldChunk;

                        }
                        IndexStack.Add(i + 0 * Width);
                    }
                    break;

                case MoveDirection.South:
                    ChunkOptimized.WorldTranslation += new Vector3(0, 0, ChunkOptimized.Depth);
                    for (int i = 0; i < Width; i++)
                    {
                        int Index = i + 0 * Width;
                        Chunks[Index].Dispose(); // FREE MEMORY
                        Chunks[Index] = new ChunkOptimized(new Vector3(i * ChunkOptimized.Width, 0, (Depth - 1) * ChunkOptimized.Depth), (ushort)Index);
                        //SWAP CHUNKS
                        for (int j = 0; j < Depth - 1; j++)
                        {
                            ChunkOptimized OldChunk = Chunks[i + (j + 1) * Width];
                            OldChunk.ChunkTranslation -= new Vector3(0, 0, ChunkOptimized.Depth);
                            Chunks[i + (j + 1) * Width] = Chunks[i + j * Width];
                            Chunks[i + j * Width] = OldChunk;

                        }
                        IndexStack.Add(i + (Depth - 1) * Width);
                    }
                    break;

                case MoveDirection.East:
                    ChunkOptimized.WorldTranslation += new Vector3(ChunkOptimized.Width, 0, 0);
                    for (int j = 0; j < Depth; j++)
                    {
                        int Index = 0 + j * Width;
                        Chunks[Index].Dispose(); // FREE MEMORY
                        Chunks[Index] = new ChunkOptimized(new Vector3((Width - 1) * ChunkOptimized.Width, 0, j * ChunkOptimized.Depth), (ushort)Index);
                        //SWAP CHUNKS
                        for (int i = 0; i < Width - 1; i++)
                        {
                            ChunkOptimized OldChunk = Chunks[(i + 1) + j * Width];
                            OldChunk.ChunkTranslation -= new Vector3(ChunkOptimized.Width, 0, 0);
                            Chunks[(i + 1) + j * Width] = Chunks[i + j * Width];
                            Chunks[i + j * Width] = OldChunk;
                        }
                        IndexStack.Add((Width - 1) + j * Width);
                    }
                    break;

                case MoveDirection.West:
                    ChunkOptimized.WorldTranslation -= new Vector3(ChunkOptimized.Width, 0, 0);
                    for (int j = 0; j < Depth; j++)
                    {
                        int Index = (Width - 1) + j * Width;
                        Chunks[Index].Dispose(); // FREE MEMORY
                        Chunks[Index] = new ChunkOptimized(new Vector3(0, 0, j * ChunkOptimized.Depth), (ushort)Index);
                        //SWAP CHUNKS
                        for (int i = Width - 1; i > 0; i--)
                        {
                            ChunkOptimized OldChunk = Chunks[(i - 1) + j * Width];
                            OldChunk.ChunkTranslation += new Vector3(ChunkOptimized.Width, 0, 0);
                            Chunks[(i - 1) + j * Width] = Chunks[i + j * Width];
                            Chunks[i + j * Width] = OldChunk;
                        }
                        IndexStack.Add(0 + j * Width);
                    }
                    break;
            }


            if (IndexStack.Count > 0)
            {
                Parallel.For(0, IndexStack.Count, new Action<int>(i =>
                {
                    Chunks[IndexStack[i]].Generate();
                }));
                MovedDirection = MoveDirection.NULL;
            }
        }
    }
}
