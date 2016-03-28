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
//using System.Runtime.InteropServices;
//namespace MinecraftClone.Core.Model.Types
//{
//    [StructLayout(LayoutKind.Sequential)]
//    public struct Air : ICube
//    {
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
//        public float Alpha { get; set; }
//        public string ID { get; set; }

//        public bool RenderBoundingBox { get; set; }
//        public BoundingBox BoundingBoxRaw { get; set; }
//        public BoundingBox BoundingBoxTransformed { get; set; }

//        public SoundEffect SoundEffect { get; set; }

//        public Air(Chunk chunk, Vector3 position, Vector3 translation, Vector3 size, Vector2 tex2d, int index) : this()
//        {
//            ID = this.GetType().Name;
//            Position = position;
//            Parent = chunk;
//            ArrayPosition = new Vector3(Position.X / 1.0f, Position.Y / 1.0f, Position.Z / 1.0f);
//            Translation = translation;
//            Size = size;
//            TextureID = tex2d;

//            InitBoundingBox();

//            Index = index;

//        }

//        public void InitBoundingBox()
//        {
//            WorldMatrix = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position + Translation);
//            BoundingBoxTransformed = BoundingBoxRenderer.UpdateBoundingBox(ModelCube, WorldMatrix);
//        }

//        public void Update(GameTime gTime) {  }
//        public void Render() { }

//        public void Debug()
//        {
//            Console.WriteLine("...");
//        }

//        public ICube GetNearestCubeToObserver(Picking picking) { return null; }

//        public override string ToString()
//        {
//            return ID;
//        }
//    }
//}
