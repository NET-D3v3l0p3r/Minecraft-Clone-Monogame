using Core.MapGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftCloneMonoGame.CoreObsolete.Misc;
 
namespace MinecraftClone.CoreII.Chunk
{
    /*
     * TODO: Generate chunk as model*/
    public sealed class ChunkOptimized : IDisposable
    {
        private static int GeneratedChunks;
        
        private HardwareInstancedRenderer InstancingDefaultModel;
        private HardwareInstancedRenderer InstancingSpecialModel;

        private int ChunkDataCounter = 0;

        public ChunkOptimized[] SurroundingChunks = new ChunkOptimized[9];

        public ushort Index { get; private set; }

        public static int Width { get; set; }
        public static int Height { get; set; }
        public static int Depth { get; set; }

        public static Vector3 WorldTranslation { get; set; }
        public Vector3 ChunkTranslation { get; set; }

        public DefaultCubeClass[] ChunkData { get; private set; } // INSTEAD OF CLASS USE PRIMITIVES
        public float[,] HeightMap { get; private set; }

        public List<int> IndexRenderer { get; private set; }
        public static int RenderingBufferSize { get; set; }

        public List<int> IndexUpdater { get; private set; }
        public static int UpdatingBufferSize { get; set; }

        public bool Invalidate { get; set; }
        public BoundingBox ChunkArea { get; set; }

        //~~~ GENERATEX DECLARATIONS ~~~

        private List<VertexPositionColorTexture> ChunkVertices = new List<VertexPositionColorTexture>();
        private VertexBuffer ChunkVertexBuffer;

        private short[] Indices;
        private IndexBuffer ChunkIndexBuffer;
        

        public ChunkOptimized(Vector3 translation, ushort index)
        {
            InstancingDefaultModel = new HardwareInstancedRenderer();
            InstancingDefaultModel.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\SeamlessStone"), GlobalShares.Stone / 2);
            InstancingDefaultModel.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\GrassTexture"), GlobalShares.Grass / 2);
            InstancingDefaultModel.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\DirtSmooth"), GlobalShares.Dirt / 2);
            InstancingDefaultModel.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\Water"), GlobalShares.Water / 2);
            InstancingDefaultModel.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\Sand"), GlobalShares.Sand / 2);

            //InstancingSpecialModel = new HardwareInstancedRenderer(CoreII.Global.GlobalShares.GlobalContent.Load<Model>(@"Model\Flat"));
            //InstancingSpecialModel.BindTexture(Global.GlobalShares.GlobalContent.Load<Texture2D>(@"Textures\Water"), GlobalShares.Water / 2);

            Index = index;
            ChunkTranslation = translation;


            ChunkArea = new BoundingBox(new Vector3(ChunkTranslation.X + WorldTranslation.X - 0.5f, 0, ChunkTranslation.Z + WorldTranslation.Z - .5f), new Vector3(ChunkTranslation.X + WorldTranslation.X + Width - 0.5f, Height * 2, ChunkTranslation.Z + WorldTranslation.Z + Depth - 0.5f));

            ChunkData = new DefaultCubeClass[Width * Height * Depth];

            IndexRenderer = new List<int>();
            IndexUpdater = new List<int>();

            UpdatingBufferSize = 512;

            HeightMap = ChunkManager.
                TerrainGeneratorSimplex.GetNoiseMap2D(
                (int)ChunkTranslation.X + (int)WorldTranslation.X,
                (int)ChunkTranslation.Z + (int)WorldTranslation.Z,
                Width, Depth);

            ChunkManager.Progress++;
        }

        public void ParseSurroundingChunks()
        {
            int X = (int)ChunkTranslation.X / (int)Width;
            int Y = (int)ChunkTranslation.Z / (int)Depth;

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

        }


        public void Generate()
        {
            ParseSurroundingChunks();

            int left = 0;
            int right = 0;
            int up = 0;
            int down = 0;

            int UnusedIndexCounter = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Depth; z++)
                {

                    if (HeightMap[x + (int)0, z + (int)0] < ChunkManager.SeaLevel + 1)
                        Push(x, ChunkManager.SeaLevel , z, (int)Global.GlobalShares.Identification.Water);

                    if (HeightMap[x, z] <= 1)
                        Push(x, 0, z, (int)Global.GlobalShares.Identification.Grass);

                    if ((x - 1) + (int)0 >= 0)
                        left =
                           (int)HeightMap[(x - 1) + (int)0, z + (int)0];
                    else if (SurroundingChunks[1] == null) left =
                           Height + 1;
                    else left = (int)SurroundingChunks[1].HeightMap[Width - 1, z];

                    if ((x + 1) + (int)0 < Width + (int)0)
                        right =
                           (int)HeightMap[(x + 1) + (int)0, z + (int)0];
                    else if (SurroundingChunks[0] == null) right =
                           Height + 1;
                    else right = (int)SurroundingChunks[0].HeightMap[0, z];

                    if ((z - 1) + (int)0 >= 0)
                        up =
                           (int)HeightMap[x + (int)0, (z - 1) + (int)0];
                    else if (SurroundingChunks[3] == null) up =
                           Height + 1;
                    else up = (int)SurroundingChunks[3].HeightMap[x, Depth - 1];

                    if ((z + 1) + (int)0 < Depth + (int)0)
                        down =
                           (int)HeightMap[x + (int)0, (z + 1) + (int)0];
                    else if (SurroundingChunks[2] == null) down =
                           Height + 1;
                    else down = (int)SurroundingChunks[2].HeightMap[x, 0];

                    for (int y = 0; y < HeightMap[x + (int)0, z + (int)0]; y++)
                    {
                        if ((y >= up ||
                            y >= down ||
                            y >= left ||
                            y >= right) &&
                            y <= HeightMap[x + (int)0, z + (int)0] - 1)

                            if (y == (int)HeightMap[x + (int)0, z + (int)0] - 1)
                                Push(x, y, z, (int)Global.GlobalShares.Identification.Grass);
                            else Push(x, y, z, (int) Global.GlobalShares.Identification.Dirt);

                        else if (y == (int)HeightMap[x + (int)0, z + (int)0] - 1)
                            Push(x, y, z, (int)Global.GlobalShares.Identification.Stone);
                    }
                }
            }
            if (GeneratedChunks++ >= ChunkManager.Width * (ChunkManager.Depth - 1))
                ChunkManager.Generated = true;
            else ChunkManager.Generated = false;
            Invalidate = true;
        }

        private void GenerateX()
        {

        }

        private void Push(int x, int y, int z, short id, bool flush = false)
        {
            if (y < Height)
            {
                var Chunk = new DefaultCubeClass(-1, Vector3.Zero, Vector3.Zero, 0,0,0, 0);
                Chunk.Id = id;
                Chunk.Index = (int)((z * ChunkOptimized.Width * ChunkOptimized.Height) + ((y) * ChunkOptimized.Depth) + x);
                Chunk.IndX = (short)x;
                Chunk.IndY = (short)y;
                Chunk.IndZ = (short)z;
                Chunk.Position = new Vector3(x, y, z) + WorldTranslation;
                Chunk.ChunkTranslation = ChunkTranslation;
                Chunk.Initialize();
                if (ChunkData[Chunk.Index] != null)
                    ChunkData[Chunk.Index].Dispose();
                Push((int)((z * ChunkOptimized.Width * ChunkOptimized.Height) + ((y) * ChunkOptimized.Depth) + x), Chunk, x,y,z);

                UploadIndexRenderer((z * ChunkOptimized.Width * ChunkOptimized.Height) + ((y) * ChunkOptimized.Depth) + x);
                ChunkManager.MaximumRender++;

                if (flush)
                    FlushChunk();
            }
            else Push(x, y - 1, z, id);
        }

        public void Push(int index, DefaultCubeClass data, int x, int y, int z)
        {
            int X = x;
            int Y = y;
            int Z = z;
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
            InstancingDefaultModel.Render();
        }

        public void UploadIndexRenderer(int index)
        {
            if (IndexRenderer.Count + 1 < RenderingBufferSize)
                IndexRenderer.Add(index);
        }
        public void UploadIndexUpdater(int index)
        {

        }


        public void AddToQueue(int index, int id)
        {
         
        }

        public void Pop(int index)
        {
            short X = ChunkData[index].IndX;
            short Y = ChunkData[index].IndY;
            short Z = ChunkData[index].IndZ;

            int HeightForward = 0;
            int HeightBackward = 0;
            int HeightLeft = 0;
            int HeightRight = 0;

            int HeightUp = 0;
            int HeightDown = 0;

            int HeightMid = 0;

            //1: left
            //2: forward
            //3: backward
            //0: right

            /*
             * RULE:
             * if y + v < height && y + v == null || y + v != null && y + v = air
             */

            if (X + 1 < Width)
                HeightRight = (int)HeightMap[X + 1, Z];
            else HeightRight = (int)SurroundingChunks[0].HeightMap[0, Z];

            if (X - 1 >= 0)
                HeightLeft = (int)HeightMap[X - 1, Z];
            else HeightLeft = (int)SurroundingChunks[1].HeightMap[Width - 1, Z];

            if (Z + 1 < Depth)
                HeightForward = (int)HeightMap[X, Z + 1];
            else HeightForward = (int)SurroundingChunks[2].HeightMap[X, 0];

            if (Z - 1 >= 0)
                HeightBackward = (int)HeightMap[X, Z - 1];
            else HeightBackward = (int)SurroundingChunks[3].HeightMap[X, Depth - 1];

            if (Y + 1 < Height)
                HeightUp = Y + 1;
            else HeightUp = Height - 1;

            if (Y - 1 >= 0)
                HeightDown = Y - 1;
            else HeightDown = 0;

            HeightMid = (int)HeightMap[X, Z];

            if (ChunkData[(Z * ChunkOptimized.Width * ChunkOptimized.Height) + ((HeightDown) * ChunkOptimized.Depth) + X] == null)
                Push(X, HeightDown, Z, (int)Global.GlobalShares.Identification.Grass);

            if (ChunkData[(Z * ChunkOptimized.Width * ChunkOptimized.Height) + ((HeightUp) * ChunkOptimized.Depth) + X] == null && HeightUp < HeightMid - 1)
                Push(X, HeightUp, Z, (int)Global.GlobalShares.Identification.Grass);


            if (Z + 1 < Depth)
            {
                if ((ChunkData[((Z + 1) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] == null ||
                ChunkData[((Z + 1) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] != null &&
                ChunkData[((Z + 1) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X].Id != -1)
                && Y < HeightForward - 1)
                    Push(X, Y, Z + 1, (int)Global.GlobalShares.Identification.Grass);

            }
            else if (
                   SurroundingChunks[2].ChunkData[((0) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] == null ||
                   SurroundingChunks[2].ChunkData[((0) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] != null &&
                   SurroundingChunks[2].ChunkData[((0) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X].Id != -1
                   && Y < HeightForward - 1)
            {
                SurroundingChunks[2].Push(X, Y, 0, (int)Global.GlobalShares.Identification.Stone, true);
            }


            if (Z - 1 >= 0)
            {
                if (
               (ChunkData[((Z - 1) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] == null ||
                ChunkData[((Z - 1) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] != null &&
                ChunkData[((Z - 1) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X].Id != -1)
                && Y < HeightBackward - 1)
                    Push(X, Y, Z - 1, (int)Global.GlobalShares.Identification.Grass);
            }
            else if (
             SurroundingChunks[3].ChunkData[((15) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] == null ||
             SurroundingChunks[3].ChunkData[((15) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X] != null &&
             SurroundingChunks[3].ChunkData[((15) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + X].Id != -1
             && Y < HeightForward - 1)
            {
                SurroundingChunks[3].Push(X, Y, 15, (int)Global.GlobalShares.Identification.Stone, true);
            }

            if (X + 1 < Width)
            {
                if (
               (ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + (X + 1)] == null ||
                ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + (X + 1)] != null &&
                ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + (X + 1)].Id != -1)
                && Y < HeightRight - 1)
                    Push(X + 1, Y, Z, (int)Global.GlobalShares.Identification.Grass);
            }
            else if (
             SurroundingChunks[0].ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + 0] == null ||
             SurroundingChunks[0].ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + 0] != null &&
             SurroundingChunks[0].ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + 0].Id != -1
             && Y < HeightForward - 1)
            {
                SurroundingChunks[0].Push(0, Y, Z, (int)Global.GlobalShares.Identification.Stone, true);
            }

            if (X - 1 >= 0)
            {
                if (
               (ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + (X - 1)] == null ||
                ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + (X - 1)] != null &&
                ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + (X - 1)].Id != -1)
                && Y < HeightLeft - 1)
                    Push(X - 1, Y, Z, (int)Global.GlobalShares.Identification.Grass);
            }
            else if (
             SurroundingChunks[1].ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + 15] == null ||
             SurroundingChunks[1].ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + 15] != null &&
             SurroundingChunks[1].ChunkData[((Z) * ChunkOptimized.Width * ChunkOptimized.Height) + ((Y) * ChunkOptimized.Depth) + 15].Id != -1
             && Y < HeightForward - 1)
            {
                SurroundingChunks[1].Push(15, Y, Z, (int)Global.GlobalShares.Identification.Stone, true);
            }

            ChunkData[index].Dispose();
            IndexRenderer.Remove(index);
            FlushChunk();
        }

        public void FlushChunk()
        {
            if (!this.Invalidate)
                this.Invalidate = true;
            
        }

        public void PullShaderData()
        {
            InstancingDefaultModel.Invalidate = true;
            InstancingDefaultModel.ResizeInstancing(IndexRenderer.Count);
            int Index = 0;
            for (int i = 0; i < IndexRenderer.Count; i++)
            {
                InstancingDefaultModel.TextureBufferArray[Index] = new Vector2(ChunkData[IndexRenderer[i]].TextureId, 0);
                InstancingDefaultModel.MatrixBufferArray[Index] = (ChunkData[IndexRenderer[i]].Transformation);
                Index++;
                ChunkManager.TotalRender++;
            }
            ChunkManager.UploadingShaderData = true;
        }
        /// <summary>
        /// IMPORTANT
        /// </summary>
        public void Dispose()
        {
            if (IndexRenderer != null)
                IndexRenderer.Clear();
            if (IndexUpdater != null)
                IndexUpdater.Clear();
            IndexRenderer = null;
            IndexUpdater = null;
            HeightMap = new float[0, 0];
            ChunkData = new DefaultCubeClass[0];
            SurroundingChunks = new ChunkOptimized[0];
            if (InstancingDefaultModel != null)
                InstancingDefaultModel.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
