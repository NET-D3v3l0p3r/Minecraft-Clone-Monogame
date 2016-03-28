using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using MinecraftClone.CoreII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftClone.CoreII.Chunk
{
    public sealed class ChunkOptimized
    {
        private static int GeneratedChunks;
        private HardwareInstancedRenderer Instancing;

        public string Id { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        public Vector3 Translation { get; private set; }

        public DefaultCubeStructure[] ChunkData { get; private set; }

        public static int[, ,] Indices { get; set; }
        public float[,] HeightMap { get; private set; }

        public List<int> RenderingCubes { get; private set; }
        public static int RenderingBufferSize { get; set; }

        public List<int> UpdatingCube { get; private set; }
        public bool Invalidate { get; set; }

        public BoundingBox ChunkArea { get; private set; }

        public ChunkOptimized(int width, int height, int depth, Vector3 translation)
        {
            Instancing = new HardwareInstancedRenderer();
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\SeamlessStone"), Core.MapGenerator.GlobalShares.Stone / 2);
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\GrassTexture"), Core.MapGenerator.GlobalShares.Grass / 2);
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\DirtSmooth"), Core.MapGenerator.GlobalShares.Dirt / 2);
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\Water"), Core.MapGenerator.GlobalShares.Water / 2);

            Width = width;
            Height = height;
            Depth = depth;

            Translation = translation;

            ChunkArea = new BoundingBox(new Vector3(Translation.X, 0, Translation.Z), new Vector3(Translation.X + Width, Translation.Y + Height, Translation.Z + Depth));
            ChunkData = new DefaultCubeStructure[Width * Height * Depth];
            
            RenderingCubes = new List<int>();
            UpdatingCube = new List<int>();
        }

        public void Generate()
        {
            HeightMap = new float[Width, Depth];

            for (int i = 0; i < HeightMap.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < HeightMap.GetUpperBound(1) + 1; j++)
                {
                    var h = ChunkManager.Generator.GetNoise3D(i + (int)Translation.X, 154, j + (int)Translation.Z); ;
                    HeightMap[i, j] = 154 + h;
                }
            }
            

            int left = 0;
            int right = 0;
            int up = 0;
            int down = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    if ((x - 1) + (int)0 >= 0)
                        left =
                           (int)HeightMap[(x - 1) + (int)0, z + (int)0];
                    else left =
                            Height + 1;

                    if ((x + 1) + (int)0 < Width + (int)0)
                        right =
                           (int)HeightMap[(x + 1) + (int)0, z + (int)0];
                    else right =
                             Height + 1;

                    if ((z - 1) + (int)0 >= 0)
                        up =
                           (int)HeightMap[x + (int)0, (z - 1) + (int)0];
                    else up =
                          Height + 1;

                    if ((z + 1) + (int)0 < Depth + (int)0)
                        down =
                           (int)HeightMap[x + (int)0, (z + 1) + (int)0];
                    else down =
                              Height + 1;

                    for (int y = 0; y < HeightMap[x + (int)0, z + (int)0]; y++)
                        if ((y >= up || y >= down || y >= left || y >= right) && y <= HeightMap[x + (int)0, z + (int)0] - 1)
                            Push(x, y, z);
                        else if (y == (int)HeightMap[x + (int)0, z + (int)0] - 1)
                            Push(x, y, z);
                }
            }
            if (GeneratedChunks++ >= ChunkManager.Width * (ChunkManager.Depth - 1))
                ChunkManager.Generated = true;
            else ChunkManager.Generated = false;
            Invalidate = true;
        }

        private void Push(int x, int y, int z)
        {
            if (y < Height)
            {
                int Index = Indices[x, y, z];
                var Chunk = ChunkData[Index];

                if (y > 60 && y < 130)
                    Chunk.Id = (int)Global.GlobalShares.Identification.Stone;
                else if (y > 130)
                    Chunk.Id = (int)Global.GlobalShares.Identification.Grass;
                else if (y <= 60)
                    Chunk.Id = (int)Global.GlobalShares.Identification.Water;
                else Chunk.Id = (int)Global.GlobalShares.Identification.Dirt;
                
                Chunk.Index = Index;
                Chunk.Position = new Microsoft.Xna.Framework.Vector3(x, y, z);
                Chunk.ChunkTranslation = Translation;
                Chunk.Initialize();

                Push(Index, Chunk);

                UploadIndexToRenderer(Index);
                ChunkManager.MaximumRender++;
            }
            else Push(x, y - 1, z);
        }

        public void Push(int index, DefaultCubeStructure data)
        {
            ChunkData[index] = data;
        }

        public void Update(GameTime gTime)
        {
            if (Invalidate)
            {
                ChunkManager.UpdatingChunks++;
                Invalidate = false;
                Parallel.For(0, UpdatingCube.Count, new Action<int>((i) =>
                {
                    ChunkManager.TotalUpdate++;
                    ChunkData[UpdatingCube[i]].Update(gTime);
                }));

                PullShaderData();
            }


            //~~~Priorized tasks~~~


        }

        public void Render()
        {
            Instancing.Render();
        }

        public void UploadIndexToRenderer(int index)
        {

            if (RenderingCubes.Count + 1 < RenderingBufferSize)
                RenderingCubes.Add(index);

        }
        public void PullShaderData()
        {
            Instancing.ResizeInstancing(RenderingCubes.Count);
            int Index = 0;
            for (int i = 0; i < RenderingCubes.Count; i++)
            {
                Instancing.TextureBufferArray[Index] = (ChunkData[RenderingCubes[i]].TextureVector2);
                Instancing.MatrixBufferArray[Index] = (ChunkData[RenderingCubes[i]].Transformation);
                Index++;
                ChunkManager.TotalRender++;
            }
            ChunkManager.UploadingShaderData = true;
            //Instancing.Apply();
        }

        public void DeleteShaderData(int index)
        {
            Instancing.TextureBuffer.Remove(ChunkData[index].TextureVector2);
            Instancing.MatrixBuffer.Remove(ChunkData[index].Transformation);
            Instancing.Apply();
        }

        //public IEnumerable<int> Filter()
        //{
        //    for (int i = 0; i < RenderingCubes.Count; i++)
        //    {
        //        if (ChunkData[RenderingCubes[i]].BoundingBox.Contains(Camera3D.ViewFrustum) != ContainmentType.Disjoint)
        //            yield return RenderingCubes[i];
        //    }
        //}

        //public void PullShaderData()
        //{
        //    var Filtered = Enumerable.ToArray<int>(Filter()).AsParallel(); //AsParallel .
        //    Instancing.ResizeInstancing(Filtered.Count());
        //    int Index = 0;

        //    foreach (var index in RenderingCubes)
        //    {
        //        Instancing.Transformations[Index] = ChunkData[index].Transformation;
        //        Instancing.Textures[Index] = ChunkData[index].TextureVector2;
        //        Index++;
        //        ChunkManager.TotalRender++;
        //    }

        //    ChunkManager.MaximumRender += Filtered.Count();
        //}
    }
}
