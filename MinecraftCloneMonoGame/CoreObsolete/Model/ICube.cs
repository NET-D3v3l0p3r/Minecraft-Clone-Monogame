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
//namespace MinecraftClone.Core.Model
//{
//    public interface ICube
//    {
//        Matrix WorldMatrix { get; set; }
//        Chunk Parent { get; set; }

//        Vector3 Position { get; set; }
//        Vector3 Translation { get; set; }
//        Vector3 ArrayPosition { get; set; }

//        Vector3 Size { get; set; }

//        Vector2 TextureID { get; set; }
//        string ID { get; set; }
//        int Index { get; set; }
//        bool Draw { get; set; }

//        bool RenderBoundingBox { get; set; }
//        BoundingBox BoundingBoxRaw { get; set; }
//        BoundingBox BoundingBoxTransformed { get; set; }

//        SoundEffect SoundEffect { get; set; }

//        void Update(GameTime gTime);
//        void Render();

//        void Debug();

//        ICube GetNearestCubeToObserver(Picking picking);

//    }
//}
