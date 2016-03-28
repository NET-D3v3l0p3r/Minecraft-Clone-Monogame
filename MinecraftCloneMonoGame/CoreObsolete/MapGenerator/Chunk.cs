////---1---
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
//using MinecraftClone.Core.Camera;
//using MinecraftClone.Core.Misc;
//using MinecraftClone.Core.Model;
//using MinecraftClone.Core.Model.Types;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MinecraftClone.Core.MapGenerator
//{
//    public class Chunk
//    {
//        public ICube[] ChunkData { get; private set; }
//        public int[, ,] ChunkIndices { get; private set; }

//        public List<int> RenderIndex { get; private set; }
//        public List<int> UpdateIndex { get; private set; }

//        public int X { get; private set; }
//        public int Y { get; private set; }
//        public Vector3 Translation { get; private set; }

//        public Chunk[] AssociatedChunks { get; private set; }

//        public BoundingBox Area { get; private set; }
//        public bool ChangedChunk { get; set; }

//        public string ID { get; private set; }

//        public int UpdatingCubes { get; private set; }
//        public int RenderingCubes { get; private set; }

//        private List<Possiblity> Possiblities = new List<Possiblity>();
//        private HardwareInstancedRenderer HardwareInstancingRenderer;
       
//        public static int TotalUpdadingCubes { get; set; }
//        public static int TotalRenderingCubes { get; set; }

//        public Chunk(ContentManager content, int x, int y)
//        {
//            ChunkData = new ICube[Map3D.LocalWidth * Map3D.LocalDepth * Map3D.LocalHeight];
//            ChunkIndices = new int[Map3D.LocalWidth, Map3D.LocalHeight, Map3D.LocalDepth];

//            RenderIndex = new List<int>();
//            UpdateIndex = new List<int>();

//            HardwareInstancingRenderer = new HardwareInstancedRenderer(Camera3D.Device, content);

//            HardwareInstancingRenderer.BindTexture(content.Load<Texture2D>(@"Textures\DirtSmooth"), GlobalShares.Dirt / 2);
//            HardwareInstancingRenderer.BindTexture(content.Load<Texture2D>(@"Textures\SeamlessStone"), GlobalShares.Stone / 2);
//            HardwareInstancingRenderer.BindTexture(content.Load<Texture2D>(@"Textures\GoldOre"), GlobalShares.GoldOre / 2);
//            HardwareInstancingRenderer.BindTexture(content.Load<Texture2D>(@"Textures\GrassTexture"), GlobalShares.Grass / 2);
//            HardwareInstancingRenderer.BindTexture(content.Load<Texture2D>(@"Textures\Sand"), GlobalShares.Sand / 2);

//            HardwareInstancingRenderer.Textures = new Vector2[ChunkData.Length];
//            HardwareInstancingRenderer.Transformations = new Matrix[ChunkData.Length];

//            X = x;
//            Y = y;

//            AssociatedChunks = new Chunk[4];
//        }

//        public void ParseAssociatingChunks()
//        {
//            if (Y + 1 < Map3D.SIZE)
//                AssociatedChunks[GlobalShares.Right] = Map3D.Chunks2D[X, Y + 1];
//            if (Y - 1 >= 0)
//                AssociatedChunks[GlobalShares.Left] = Map3D.Chunks2D[X, Y - 1];
//            if (X - 1 >= 0)
//                AssociatedChunks[GlobalShares.Down] = Map3D.Chunks2D[X - 1, Y];
//            if (X + 1 < Map3D.SIZE)
//                AssociatedChunks[GlobalShares.Top] = Map3D.Chunks2D[X + 1, Y];
//        }

//        public void LoadChunk(Vector3 translation)
//        {
//            ChangedChunk = true;

//            Translation = translation;
//            Area = new BoundingBox(Translation , new Vector3(Map3D.LocalWidth, Map3D.LocalHeight, Map3D.LocalDepth) + Translation);
//            ID = GlobalShares.GetRandomWord(Random.Next(5, 7));

//            Possiblities.Add(new Possiblity("Dirt", 0.2f));
//            Possiblities.Add(new Possiblity("Stone", 0.9f));
//            Possiblities.Add(new Possiblity("Gold", 0.005f));
//            Possiblities.Add(new Possiblity("Sand", 0.3f));
//            Stopwatch sw = new Stopwatch();

//            sw.Start();

//            for (int z = 0; z < Map3D.LocalDepth; z++)
//            {
//                for (int y = 0; y < Map3D.LocalHeight; y++)
//                {
//                    for (int x = 0; x < Map3D.LocalWidth; x++)
//                    {
//                        string Resource = GetResource();
//                        int Index = (z * Map3D.LocalWidth * Map3D.LocalHeight) + (y * Map3D.LocalWidth) + x;
//                        ICube Cube = new Air(this, new Microsoft.Xna.Framework.Vector3(x, y, z), Translation, new Microsoft.Xna.Framework.Vector3(0.5f), new Microsoft.Xna.Framework.Vector2(-1, -1), Index);
//                        if (y < Map3D.LocalHeight - Map3D.AirHeight)
//                            Cube = new Dirt(this, new Microsoft.Xna.Framework.Vector3(x, y, z), Translation, new Microsoft.Xna.Framework.Vector3(0.5f), Index);
//                        switch (Resource)
//                        {
//                            case "Dirt":
//                            entry_dirt:
//                                if (y < Map3D.LocalHeight - Map3D.AirHeight)
//                                    Cube = new Dirt(this, new Microsoft.Xna.Framework.Vector3(x, y, z), Translation, new Microsoft.Xna.Framework.Vector3(0.5f), Index);
//                                break;
//                            case "Sand":
//                                if (y > 0 && y < Map3D.LocalHeight - Map3D.AirHeight)
//                                    Cube = new Sand(this, new Microsoft.Xna.Framework.Vector3(x, y, z), Translation, new Microsoft.Xna.Framework.Vector3(0.5f), Index);
//                                break;
//                            case "Stone":
//                                if (y > 0 && y < Map3D.LocalHeight - (Map3D.AirHeight + 5))
//                                    Cube = new Stone(this, new Microsoft.Xna.Framework.Vector3(x, y, z), Translation, new Microsoft.Xna.Framework.Vector3(0.5f), Index);
//                                else goto entry_dirt;
//                                break;
//                            case "Gold":
//                                if (y > 0 && y < 45)
//                                    Cube = new GoldOre(this, new Microsoft.Xna.Framework.Vector3(x, y, z), Translation, new Microsoft.Xna.Framework.Vector3(0.5f), Index);
//                                else goto entry_dirt;
//                                break;
//                        }
//                        ChunkIndices[x, y, z] = Index;
//                        ChunkData[Index] = Cube;

//                        //215
//                    }
//                }
//            }

//            sw.Stop();

//            for (int i = 0; i < ChunkData.Length; i++)
//            {
//                if (ChunkData[i] == null)
//                    Console.WriteLine("Found null at index {0}", i);
//            }

//            Console.WriteLine("Generated Chunk in " + sw.Elapsed.TotalMilliseconds + " ms.");
 
//        }
//        public void UpdateChunk(GameTime gTime)
//        {
//            if (ChangedChunk)
//            {
//                RenderIndex.Clear();
//                Map3D.UpdatingChunks++;

//                for (int i = 0; i < ChunkData.Length; i++)
//                    ChunkData[i].Update(gTime);
//                ChangedChunk = false;
//            }

//            if (UpdateIndex.Count > 0)
//                Map3D.UpdatingChunks++;

//            for (int i = 0; i < UpdateIndex.Count; i++)
//            {
//                TotalUpdadingCubes++;
//                ChunkData[UpdateIndex[i]].Update(gTime);
//            }

//        }
//        public void RenderChunk()
//        {
//            HardwareInstancingRenderer.ResizeInstancing(RenderIndex.Count);
//            RenderingCubes = 0;
//            for (int i = 0; i < RenderIndex.Count; i++)
//            {
//                if (ChunkData[RenderIndex[i]].BoundingBoxRaw.Contains(Camera3D.ViewFrustum) != ContainmentType.Disjoint || this.Equals(Map3D.LocalChunk0))
//                {
//                    HardwareInstancingRenderer.Transformations[RenderingCubes] = ChunkData[RenderIndex[i]].WorldMatrix;
//                    HardwareInstancingRenderer.Textures[RenderingCubes] = ChunkData[RenderIndex[i]].TextureID;
//                    if (ChunkData[RenderIndex[i]].BoundingBoxTransformed.Intersects(Camera3D.Ray) > 1)
//                        Map3D.LengthToCube = (float)Camera3D.Ray.Intersects(ChunkData[RenderIndex[i]].BoundingBoxTransformed) * 3;
//                    RenderingCubes++;
//                    TotalRenderingCubes++;
//                }
//            }

//            HardwareInstancingRenderer.Render();
//            //BoundingBoxRenderer.Render(Area, Camera3D.Device, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Red);
//        }

//        public void ReadChunk(string destination)
//        {
//            if (!File.Exists(destination))
//                return;
//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            string[] Lines = File.ReadAllLines(destination);
//            Microsoft.Xna.Framework.Vector3 Position = new Microsoft.Xna.Framework.Vector3(0);
//            Microsoft.Xna.Framework.Vector3 Translation = new Microsoft.Xna.Framework.Vector3(0);
//            Microsoft.Xna.Framework.Vector3 Size = new Microsoft.Xna.Framework.Vector3(0.5f);
//            for (int i = 0; i < Lines.Length; i++)
//            {
//                string[] Items = Lines[i].Split(';');
//                string Name = Items[0];

//                Position.X = float.Parse(Items[1].Split(',')[0]);
//                Position.Y = float.Parse(Items[1].Split(',')[1]);
//                Position.Z = float.Parse(Items[1].Split(',')[2]);

//                Translation.X = int.Parse(Items[2].Split(',')[0]);
//                Translation.Y = int.Parse(Items[2].Split(',')[1]);
//                Translation.Z = int.Parse(Items[2].Split(',')[2]);

//                int Index = int.Parse(Items[3]);

//                switch (Name)
//                {
//                    case "Dirt":
//                        ChunkData[Index] = new Dirt(this, Position, Translation, Size, Index);
//                        break;
//                    case "GoldOre":
//                        ChunkData[Index] = new GoldOre(this, Position, Translation, Size, Index);
//                        break;
//                    case "Air":
//                        ChunkData[Index] = new Air(this, Position, Translation, Size, new Microsoft.Xna.Framework.Vector2(-1), Index);
//                        break;
//                    case "Stone":
//                        ChunkData[Index] = new Stone(this, Position, Translation, Size, Index);
//                        break;
//                    case "Sand":
//                        ChunkData[Index] = new Sand(this, Position, Translation, Size, Index);
//                        break;
//                }
//            }
//            sw.Stop();

//            Console.WriteLine("Chunk read successfully: {0} ms.", sw.Elapsed.Milliseconds);

            
//        }

//        public void SaveChunk(string destination)
//        {
//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            string DATA = "";
//            StringBuilder sb = new StringBuilder(DATA);
//            for (int i = 0; i < ChunkData.Length; i++)
//            {
//                var Item = ChunkData[i];
//                if (Item != null)
//                    sb.Append(Item.ID + ";" + Item.Position.X + "," + Item.Position.Y + "," + Item.Position.Z + ";" + Item.Translation.X + "," + Item.Translation.Y + "," + Item.Translation.Z + ";" + Item.Index + Environment.NewLine);
//            }

//            File.WriteAllText(destination, sb.ToString(0, sb.ToString().Length - 2));

//            sw.Stop();
//            Console.WriteLine("Saving chunks... :{0}.", sw.Elapsed.Milliseconds);
//        }
//        public static Random Random = new Random();
//        public string GetResource()
//        {
//            var Integer = Random.Next(Possiblity.LastBound + 1);   
//            foreach (var possiblity in Possiblities)
//                if (possiblity.Minimum <= Integer && Integer <= possiblity.Maximum)
//                    return possiblity.Resource;


//            return "null";
//        }
//    }
//}
