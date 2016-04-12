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
using MinecraftCloneMonoGame.CoreObsolete.Misc;
namespace MinecraftClone.CoreII
{
    //UPDATED TO CLASS
    //SIGNFICANT PERFOMANCE INCREASES
    //32x32 CHUNK-MAP ~ 512 MB!
    public class DefaultCubeClass : IDisposable
    {
        public int Owner;

        public short Id;
        public int Index;
        public Action<DefaultCubeClass> Task;

        public Vector3 Position;
        public Vector3 ChunkTranslation;

        public Vector3 IndexPosition;

        public Matrix Transformation;

        public short TextureId;
        public float MetaData;

        public BoundingBox CollisionBox { get; set; }
        public BoundingBox PickingBox { get; set; }

        /// <summary>
        /// Notice: Position = Position + WorldTranslation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="translation"></param>
        /// <param name="index"></param>
        public DefaultCubeClass(short id, Vector3 position, Vector3 translation, Vector3 index_pos, int index, int owner) 
        {
            Id = id;
            Owner = owner;
            Index = index;
            Position = position;
            ChunkTranslation = translation;
            IndexPosition = index_pos;

            Initialize();
        }
         
        public void Update(GameTime gTime) { if (Task != null) Task.DynamicInvoke(this); }
        public void Initialize()
        {

            GlobalModels.IndexTextureTuple.TryGetValue(Id, out TextureId);
            Transformation = Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(Position + ChunkTranslation);
            //THIS LINE IS CAUSING PERFOMANCE ISSUES
            //CALCULATE BOUNDINGBOX BY OWN
            PickingBox = new BoundingBox(Position + ChunkTranslation - new Vector3(0.5f), Position + ChunkTranslation + new Vector3(0.5f));
            CollisionBox = new BoundingBox(Position + ChunkTranslation - new Vector3(0.6f), Position + ChunkTranslation + new Vector3(0.6f));
        }

        public override string ToString()
        {
            return Enum.GetName(typeof(Global.GlobalShares.Identification), Id);
        }

        public void Dispose()
        {
            Id = -1;
            Task = null;
            Position = Vector3.Zero;
            ChunkTranslation = Vector3.Zero;
            Transformation = Matrix.Identity;
            TextureId = -1;
            MetaData = -1;
            PickingBox = new BoundingBox();
            CollisionBox = new BoundingBox();
            GC.SuppressFinalize(this);
            
        }
    }
}
