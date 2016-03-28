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
//using MinecraftClone.Core.MapGenerator;
//namespace MinecraftClone.Core.Model.Types
//{
//    public struct GoldOre : ICube
//    {
//        ICube Top ;
//        ICube Bottom ;
//        ICube Left ;
//        ICube Right ;
//        ICube High;
//        ICube Down ;

//        public static int AmountDrawingCubes { get; set; }
//        public Chunk Parent { get; set; }
//        public Vector3 ArrayPosition { get; set; }
//        public int Index { get; set; }
//        public bool Draw { get; set; }

//        public static Microsoft.Xna.Framework.Graphics.Model ModelCube { get; set; }

//        public static void Load(ContentManager Content)
//        {
//            ModelCube = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(@"Cube\Cube1x1");
//        }

//        public Matrix WorldMatrix { get; set; }
//        public Vector3 Position { get; set; }
//        public Vector3 Translation { get; set; }
//        public Vector3 Size { get; set; }

//        public Vector2 TextureID { get; set; }
//        public string ID { get; set; }

//        public bool RenderBoundingBox { get; set; }
//        public BoundingBox BoundingBoxRaw { get; set; }
//        public BoundingBox BoundingBoxTransformed { get; set; }

//        public SoundEffect SoundEffect { get; set; }

//        public GoldOre(Chunk chunk, Vector3 position, Vector3 translation, Vector3 size, int index) : this()
//        {
//            ID = this.GetType().Name;
//            Position = position;
//            Parent = chunk;
//            ArrayPosition = new Vector3(Position.X / 1.0f, Position.Y / 1.0f, Position.Z / 1.0f);
//            Translation = translation;
//            Size = size;
//            TextureID = new Vector2(GlobalShares.GoldOre, 0);

//            WorldMatrix = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position + Translation);
//            BoundingBoxRaw = BoundingBoxRenderer.UpdateBoundingBox(ModelCube, WorldMatrix);
//            WorldMatrix = Matrix.CreateScale(Size + new Vector3(0.25f)) * Matrix.CreateTranslation(Position + Translation);
//            BoundingBoxTransformed = BoundingBoxRenderer.UpdateBoundingBox(ModelCube, WorldMatrix);


//            WorldMatrix = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position + Translation);


//            Index = index;

//        }

//        public void Update(GameTime gTime)
//        {

//            if (true)
//            {
//                var Map = Parent.ChunkData;
//                if (ArrayPosition.Z - 1 >= 0)
//                    Bottom = Map[Map3D.GetIndex((int)ArrayPosition.X, (int)ArrayPosition.Y, (int)ArrayPosition.Z - 1)];
//                if (ArrayPosition.Z + 1 < Map3D.LocalDepth)
//                    Top = Map[Map3D.GetIndex((int)ArrayPosition.X, (int)ArrayPosition.Y, (int)ArrayPosition.Z + 1)];

//                if (ArrayPosition.X - 1 >= 0)
//                    Left = Map[Map3D.GetIndex((int)ArrayPosition.X - 1, (int)ArrayPosition.Y, (int)ArrayPosition.Z)];
//                if (ArrayPosition.X + 1 < Map3D.LocalWidth)
//                    Right = Map[Map3D.GetIndex((int)ArrayPosition.X + 1, (int)ArrayPosition.Y, (int)ArrayPosition.Z)];

//                if (ArrayPosition.Y - 1 >= 0)
//                    Down = Map[Map3D.GetIndex((int)ArrayPosition.X, (int)ArrayPosition.Y - 1, (int)ArrayPosition.Z)];
//                if (ArrayPosition.Y + 1 < Map3D.LocalHeight)
//                    High = Map[Map3D.GetIndex((int)ArrayPosition.X, (int)ArrayPosition.Y + 1, (int)ArrayPosition.Z)];


//                Draw =
//                    ((Top != null && Top.ID == "Air") ||
//                    (Bottom != null && Bottom.ID == "Air") ||
//                    (Left != null && Left.ID == "Air") ||
//                    (Right != null && Right.ID == "Air") ||

//                    (Down != null && Down.ID == "Air") ||
//                    (High != null && High.ID == "Air"));

//                if (Draw)
//                    Parent.RenderIndex.Add(Index);


//            }

//            Chunk.TotalUpdadingCubes++;
//        }

//        public void Render()
//        {
//            if (RenderBoundingBox)
//                BoundingBoxRenderer.Render(BoundingBoxRaw, Camera.Camera3D.Device, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Red);
//        }

//        public void Debug()
//        {
//            Console.WriteLine("NAME: " + ID);
//            Console.WriteLine("POSITION :" + Position);
//            Console.WriteLine("ARRAY_POSITION :" + ArrayPosition);
//            Console.WriteLine("SIZE: " + Size);
//            Console.WriteLine("TEXTURE: " + TextureID);
//        }
//        public ICube GetNearestCubeToObserver(Picking picking)
//        {
//            ICube[] Cubes = new ICube[6];
//            int counter = 0;
//            if (Left != null && Left.ID == "Air")
//                Cubes[counter++] = Left;
//            if (Right != null && Right.ID == "Air")
//                Cubes[counter++] = Right;
//            if (Bottom != null && Bottom.ID == "Air")
//                Cubes[counter++] = Bottom;
//            if (Top != null && Top.ID == "Air")
//                Cubes[counter++] = Top;
//            if (High != null && High.ID == "Air")
//                Cubes[counter++] = High;
//            if (Down != null && Down.ID == "Air")
//                Cubes[counter++] = Down;


//            return GlobalShares.GetNearest(Cubes, picking);
//        }
//    }
//}
