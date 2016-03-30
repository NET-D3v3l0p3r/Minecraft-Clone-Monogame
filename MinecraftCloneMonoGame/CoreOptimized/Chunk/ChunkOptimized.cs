using Core.MapGenerator;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Earlz.BareMetal;
namespace MinecraftClone.CoreII.Chunk
{
    public sealed class ChunkOptimized
    {
        private static int GeneratedChunks;
        private HardwareInstancedRenderer Instancing;

        public ChunkOptimized[] SurroundingChunks = new ChunkOptimized[9];
        private bool Parsed;

        public string Id { get; private set; }

        public static int Width { get; set; }
        public static int Height { get; set; }
        public static int Depth { get; set; }

        public Vector3 Translation { get; private set; }

        public DefaultCubeClass[] ChunkData { get; private set; }

        public float[,] HeightMap { get; private set; }

        public List<int> IndexRenderer { get; private set; }
        public static int RenderingBufferSize { get; set; }

        public List<int> IndexUpdater { get; private set; }
        public static int UpdatingBufferSize { get; set; }

        public bool Invalidate { get; set; }

        public BoundingBox ChunkArea { get; private set; }

        public Color Color;

        public ChunkOptimized(Vector3 translation)
        {
            Instancing = new HardwareInstancedRenderer();
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\SeamlessStone"), GlobalShares.Stone / 2);
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\GrassTexture"), GlobalShares.Grass / 2);
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\DirtSmooth"), GlobalShares.Dirt / 2);
            Instancing.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\Water"), GlobalShares.Water / 2);

            Translation = translation;

            ChunkArea = new BoundingBox(new Vector3(Translation.X -0.5f, 0, Translation.Z -.5f), new Vector3(Translation.X + Width -0.5f, Height, Translation.Z + Depth - 0.5f));
            ChunkData = new DefaultCubeClass[Width * Height * Depth];

            IndexRenderer = new List<int>();
            IndexUpdater = new List<int>();

            UpdatingBufferSize = 512;

            HeightMap = new float[Width, Depth];

            for (int i = 0; i < HeightMap.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < HeightMap.GetUpperBound(1) + 1; j++)
                {
                    HeightMap[i, j] = 133 + ChunkManager.Generator.GetNoise3D(i + (int)Translation.X, 154, j + (int)Translation.Z);
                }
            }

            ChunkManager.Progress++;
        }

        private void ParseSurroundingChunks()
        {
            if ( !Parsed)
            {
                int X = (int)Translation.X / (int) Width;
                int Y = (int)Translation.Z / (int) Depth;

                if (X + 1 < ChunkManager.Width)
                    SurroundingChunks[0] = ChunkManager.Chunks[(X + 1) + Y * ChunkManager.Width];
                if (X - 1 >= 0)
                    SurroundingChunks[1] = ChunkManager.Chunks[(X - 1) + Y * ChunkManager.Width];

                if (Y + 1 < ChunkManager.Depth)
                    SurroundingChunks[2] = ChunkManager.Chunks[X + (Y + 1) * ChunkManager.Width];
                if (Y - 1 >= 0)
                    SurroundingChunks[3] = ChunkManager.Chunks[X + (Y - 1) * ChunkManager.Width];

                if (X + 1 < ChunkManager.Width && Y + 1 < ChunkManager.Depth)
                    SurroundingChunks[4] = ChunkManager.Chunks[(X + 1) + (Y + 1) * ChunkManager.Width];
                if (X + 1 < ChunkManager.Width && Y - 1 >= 0)
                    SurroundingChunks[5] = ChunkManager.Chunks[(X + 1) + (Y - 1) * ChunkManager.Width];

                if (X - 1 >= 0 && Y + 1 < ChunkManager.Depth)
                    SurroundingChunks[6] = ChunkManager.Chunks[(X - 1) + (Y + 1) * ChunkManager.Width];
                if (X - 1 >= 0 && Y - 1 >= 0)
                    SurroundingChunks[7] = ChunkManager.Chunks[(X - 1) + (Y - 1) * ChunkManager.Width];

                SurroundingChunks[8] = this;

                Parsed = true;

            }
        }


        public void Generate()
        {
            ParseSurroundingChunks();

            int left = 0;
            int right = 0;
            int up = 0;
            int down = 0;

            int X = (int)Translation.X;
            int Z = (int)Translation.Z;

            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    if ((x - 1) + (int)0 > 0)
                        left =
                           (int)HeightMap[(x - 1) + (int)0, z + (int)0];
                    else if(SurroundingChunks[1] == null) left =
                             Height + 1;
                    else left = (int)SurroundingChunks[1].HeightMap[Width - 1, z];

                    if ((x + 1) + (int)0 < Width  + (int)0)
                        right =
                           (int)HeightMap[(x + 1) + (int)0, z + (int)0];
                    else if(SurroundingChunks[0] == null) right =
                             Height + 1;
                    else right = (int)SurroundingChunks[0].HeightMap[0, z];

                    if ((z - 1) + (int)0 > 0)
                        up =
                           (int)HeightMap[x + (int)0, (z - 1) + (int)0];
                    else if(SurroundingChunks[3] == null) up =
                          Height + 1;
                    else up = (int)SurroundingChunks[3].HeightMap[x, Depth - 1];

                    if ((z + 1) + (int)0 < Depth + (int)0)
                        down =
                           (int)HeightMap[x + (int)0, (z + 1) + (int)0];
                    else if (SurroundingChunks[2] == null) down =
                                    Height + 1;
                    else down = (int)SurroundingChunks[2].HeightMap[x, 0];

                    bool SetEdge = false;

                    for (int y = 0; y < HeightMap[x + (int)0, z + (int)0]; y++)
                    {
                        if ((y >= up || y >= down || y >= left || y >= right) && y <= HeightMap[x + (int)0, z + (int)0] - 1)
                        {
                            if (!SetEdge)
                            {
                                if (y < Height && y > 0)
                                    ChunkData[ChunkManager.Indices[x, y - 1, z]] = new DefaultCubeClass((int)Global.GlobalShares.Identification.Air, Vector3.Zero, Vector3.Zero, ChunkManager.Indices[x, y - 1, z]);
                                SetEdge = true;
                            }
                            Push(x, y, z, (int)Global.GlobalShares.Identification.Grass);
                        }
                        else if (y == (int)HeightMap[x + (int)0, z + (int)0] - 1)
                        {
                            if(y < Height && y > 0)
                                ChunkData[ChunkManager.Indices[x, y - 1, z]] = new DefaultCubeClass((int)Global.GlobalShares.Identification.Air, Vector3.Zero, Vector3.Zero, ChunkManager.Indices[x, y - 1, z]);
                            Push(x, y, z, (int)Global.GlobalShares.Identification.Stone);
                        }
                    }
                }
            }
            if (GeneratedChunks++ >= ChunkManager.Width * (ChunkManager.Depth - 1))
                ChunkManager.Generated = true;
            else ChunkManager.Generated = false;
            Invalidate = true;
        }

        private void Push(int x, int y, int z, int id)
        {
            if (y < Height)
            {
                int Index = ChunkManager.Indices[x, y, z];
                var Chunk = new DefaultCubeClass(-1, Vector3.Zero, Vector3.Zero, -1);
                Chunk.Id = id;

                Chunk.Index = Index;
                Chunk.Position = new Microsoft.Xna.Framework.Vector3(x, y, z);
                Chunk.ChunkTranslation = Translation;
                Chunk.Initialize();

                Push(Index, Chunk);

                UploadIndexRenderer(Index);
                ChunkManager.MaximumRender++;
            }
            else Push(x, y - 1, z, id);
        }

        public void Push(int index, DefaultCubeClass data)
        {
            ChunkData[index] = data;
        }

        public void Update(GameTime gTime)
        {
            if (Invalidate)
            {
                ChunkManager.UpdatingChunks++;
                Invalidate = false;
                Parallel.For(0, IndexUpdater.Count, new Action<int>((i) =>
                {
                    ChunkManager.TotalUpdate++;
                    ChunkData[IndexUpdater[i]].Update(gTime);
                }));

                PullShaderData();
            }

            //~~~Priorized tasks~~~


        }

        public void Render()
        {
            Instancing.Render();
        }

        public void UploadIndexRenderer(int index)
        {
            if (IndexRenderer.Count + 1 < RenderingBufferSize)
                IndexRenderer.Add(index);
        }
        public void UploadIndexUpdater(int index)
        {

        }

        public DefaultCubeClass Pop(int index)
        {
            var Cube = ChunkData[index];
            IndexRenderer.Remove(index);
            Cube.Id = (int)Global.GlobalShares.Identification.Air;
            Push(index, Cube);

            var X = (int)Cube.Position.X;
            var Y = (int)Cube.Position.Y - 1;
            var Z = (int)Cube.Position.Z;




            //TODO: GENERATE NEW CUBES 


            FlushChunk();
            return Cube;
        }

        public void FlushChunk()
        {
            if (!this.Invalidate)
                this.Invalidate = true;
        }

        public void PullShaderData()
        {
            Instancing.ResizeInstancing(IndexRenderer.Count);
            int Index = 0;
            for (int i = 0; i < IndexRenderer.Count; i++)
            {
                Instancing.TextureBufferArray[Index] = (ChunkData[IndexRenderer[i]].TextureVector2);
                Instancing.MatrixBufferArray[Index] = (ChunkData[IndexRenderer[i]].Transformation);
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
