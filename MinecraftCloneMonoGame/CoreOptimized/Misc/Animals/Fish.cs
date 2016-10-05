using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.CoreII.Chunk;
using MinecraftClone.CoreII.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.CoreOptimized.Misc.Animals
{
    public class Fish : ArtificallyInteligence
    {
        public Definition.Typus AITypus { get; set; }
        public Stack<Definition.Intention> AIIntentions { get; set; }


        public int HitPoints { get; private set; }
        public bool IsAlive { get; private set; }

        private GraphicsDevice _Device;
        private ContentManager _Content;

        private Vector3 _Position;

        private Model _Model;
        private Texture2D _Texture;

        private Ray _Ray;

        public Fish(GraphicsDevice _device, ContentManager _content)
        {
            _Device = _device;
            _Content = _content;

            _Model = _Content.Load<Model>(@"Animals\Fish\Model\Fish");
            _Texture = _Content.Load<Texture2D>(@"Animals\Fish\Texture\Fish0");

        }

        public void ThinkAI()
        {
            switch (MinecraftClone.CoreII.Global.GlobalShares.GlobalRandom.Next(0, 15))
            {
                case 0:
                    AIIntentions.Push(Definition.Intention.Eat);
                    break;
                default :
                    AIIntentions.Push(Definition.Intention.Idle);
                    break;
            }
        }

        public void ProcessAI()
        {
            bool _Doing = false;
            while (IsAlive)
            {
                // THINK NEXT INTENTION
                bool _Legal = false;
                if (_Doing)
                    ThinkAI();

                if (_Legal = !_IsUnderWater())
                    Hit(5);

                _Legal = !_Legal;

                foreach (var _Intention in AIIntentions)
                {
                    switch (_Intention)
                    {
                        case Definition.Intention.Eat:

                            break;

                        case Definition.Intention.Idle:
                            if (_Legal)
                            {
                                _Doing = true;

                                _Position = new Vector3(_Position.X + GlobalShares.GlobalRandom.Next(0, 1), _Position.Y + (float)GlobalShares.GlobalRandom.NextDouble(), _Position.Z + GlobalShares.GlobalRandom.Next(0, 1));
                            }
                            break;
                    }
                }
            }
        }

        private bool _IsUnderWater()
        {
            _Ray = new Ray(_Position, Vector3.Up);

            ChunkOptimized _Chunk = ChunkManager.GetChunkArea(_Position, true);
            var _Upper = ChunkManager.GetFocusedCubeSpecified(_Chunk, float.MaxValue, _Ray, (int)GlobalShares.Identification.Water);

            return _Upper.HasValue;
        }

        public void Hit(int damage)
        {
            IsAlive = !(HitPoints - damage <= 0);
        }

        public void Render()
        {
            Matrix[] _Bones = new Matrix[_Model.Bones.Count];
            _Model.CopyAbsoluteBoneTransformsTo(_Bones);

            foreach (var _Mesh in _Model.Meshes)
            {
                foreach (BasicEffect _Effect in _Mesh.Effects)
                {

                }

                _Mesh.Draw();
            }
        }
    }
}
