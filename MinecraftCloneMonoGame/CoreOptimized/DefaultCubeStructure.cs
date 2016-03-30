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
    //UPDATED TO CLASS
    //SIGNFICANT PERFOMANCE INCREASES
    //32x32 CHUNK-MAP ~ 512 MB!
    public class DefaultCubeClass
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public Action<DefaultCubeClass> Task { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 ChunkTranslation { get; set; }
        public Matrix Transformation { get; set; }

        public Vector2 TextureVector2;
        public float MetaData { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public DefaultCubeClass(int id, Vector3 position, Vector3 translation, int index) 
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

            GlobalModels.IndexTextureTuple.TryGetValue(Id, out TextureVector2);
            Transformation = Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(Position + ChunkTranslation);
            //THIS LINE IS CAUSING PERFOMANCE ISSUES
            //CALCULATE BOUNDINGBOX BY OWN
            BoundingBox = new BoundingBox(Position + ChunkTranslation - new Vector3(0.5f), Position + ChunkTranslation + new Vector3(0.5f));
        }

        public override string ToString()
        {
            return Enum.GetName(typeof(Global.GlobalShares.Identification), Id);
        }

        

    }
}
