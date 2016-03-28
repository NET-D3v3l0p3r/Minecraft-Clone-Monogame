using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Misc;
using MinecraftClone.CoreII.Models;
using System.Runtime.InteropServices;
namespace MinecraftClone.CoreII
{
    public struct DefaultCubeStructure
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public Action<DefaultCubeStructure> Task { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 ChunkTranslation { get; set; }
        public Matrix Transformation { get; set; }

        public Vector2 TextureVector2 { get; set; }
        public float MetaData { get; set; }

        public BoundingBox BoundingBox { get; set; }
         
        public bool isAir { get; set; }

        public DefaultCubeStructure(int id, Vector3 position, Vector3 translation, int index) : this()
        {
            Id = id;
            Index = index;
            Position = position;
            ChunkTranslation = translation;

            Initialize();
        }

        public void Update(GameTime gTime) { if (Task != null) Task.DynamicInvoke(this); }
        public void Initialize()
        {
            TextureVector2 = GlobalModels.IndexTextureTuple[Id];
            Transformation = Matrix.CreateScale(1.9f) * Matrix.CreateTranslation(Position + ChunkTranslation);
            BoundingBox = BoundingBoxRenderer.UpdateBoundingBox(GlobalModels.IndexModelTuple[0], Transformation);
            Transformation = Matrix.CreateScale(1) * Matrix.CreateTranslation(Position + ChunkTranslation);
        }

        public override string ToString()
        {
            return Enum.GetName(typeof(Global.GlobalShares.Identification), Id);
        }

        

    }
}
