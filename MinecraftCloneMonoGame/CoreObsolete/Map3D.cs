////-1-
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;
//using MinecraftClone.Core.Misc;
//using MinecraftClone.Core.Camera;
//using MinecraftClone.Core.Model;
//using System.Threading.Tasks;
//using MinecraftClone.Core.Model.Types;
//using MinecraftClone.Core.MapGenerator;
//using System.Threading;
//using System.Diagnostics;
//namespace MinecraftClone.Core
//{
//    public class Map3D
//    {
//        public static Chunk LocalChunk0 { get; private set; }

//        public static int LocalWidth { get; set; }
//        public static int LocalHeight { get; set; }
//        public static int LocalDepth { get; set; }
//        public static int AirHeight { get; set; }

//        public static Chunk[] Chunks { get; private set; }
//        public static Chunk[,] Chunks2D { get; private set; }

//        public static float LengthToCube;

//        public static int UpdatingChunks;
//        public static int RenderingChunks;

//        public static Air Air;

//        public static int GetIndex(int x, int y, int z)
//        {
//            return LocalChunk0.ChunkIndices[x, y, z];
//        }

//        public static int GetIndex(Chunk chunk, int x, int y, int z)
//        {
//            return chunk.ChunkIndices[x, y, z];
//        }

//        public static readonly int SIZE = 2;

//        public static void GenerateChunk(ContentManager content)
//        {
//            Console.WriteLine("Loading...");
            
//            Chunks = new Chunk[SIZE * SIZE];
//            Chunks2D = new Chunk[SIZE, SIZE];

//            for (int j = 0; j < SIZE; j++)
//            {
//                for (int i = 0; i < SIZE; i++)
//                {
//                    var Chunk = new Chunk(content, i, j);
//                    Chunk.LoadChunk(new Vector3(i * 15, 0, j * 15));
//                    Chunks[i + j * SIZE] = Chunk;
//                    Chunks2D[i, j] = Chunk;
                    
//                }
//            }

//            for (int i = 0; i < Chunks.Length; i++)
//            {
//                Chunks[i].ParseAssociatingChunks();
//            }

//            Air = new Air(LocalChunk0, new Vector3(0), new Vector3(0), new Vector3(-1), new Vector2(0), -1);
//        }

//        public static void Update(GameTime gTime)
//        {
//            if (Camera3D.isMoving)
//                CalculateCurrentChunk();
//            Chunk.TotalUpdadingCubes = 0;
//            Parallel.For(0, Chunks.Length, new Action<int>((int i) =>
//            {
//                Chunks[i].UpdateChunk(gTime);
//            }));
//        }


//        public static void Render()
//        {
//            Chunk.TotalRenderingCubes = 0;
//            for (int i = 0; i < Chunks.Length; i++)
//            {
//                Chunks[i].RenderChunk();
//            }
//        }

//        public static void CalculateCurrentChunk()
//        {
//            //float Distance = 10000000;
//            for (int i = 0; i < Chunks.Length; i++)
//            {
//                if (Chunks[i].Area.Contains(Camera3D.CameraPosition) != ContainmentType.Disjoint)
//                    LocalChunk0 = Chunks[i];
//                //if (Chunks[i].ID != LocalChunk0.ID)
//                //{
//                //    var BnbBox = Chunks[i].Area;
//                //    var Center = new Vector3((BnbBox.Max.X / 2) + (BnbBox.Min.X), Map3D.LocalHeight - Map3D.AirHeight + 2, (BnbBox.Max.Z / 2) + (BnbBox.Min.Z));
//                //    if ((Center - Camera3D.CameraPosition).Length() < Distance)
//                //    {
//                //        Distance = (Center - Camera3D.CameraPosition).Length();
//                //        LocalChunk1 = Chunks[i];
//                //    }
//                //}
//            }

//        }




//    }
//}