using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.Atmosphere
{
    public class Moon : CelestialBody
    {

        public GraphicsDevice GraphicsDevice { get; set; }
        public ContentManager ContentManager { get; set; }

        public Model Model { get; set; }
        public Dictionary<string, Texture2D> Textures { get; set; }

        public float MetaData { get; set; }
        public float Time { get; set; }

        public Vector3 Postion { get; set; }

        public Moon(GraphicsDevice _device, ContentManager _content)
        {
            GraphicsDevice = _device;
            ContentManager = _content;

            Textures = new Dictionary<string, Texture2D>();

            Model = ContentManager.Load<Model>(@"Atmosphere\Moon");
            Textures.Add("Standard", ContentManager.Load<Texture2D>(@"Atmosphere\Textures\Moon")); // ADD PHASES

            Postion = new Vector3(0, 500, 0);
        }

        public void Update(GameTime _gTime)
        {
            Time = (float)_gTime.TotalGameTime.TotalSeconds;
            MetaData += 0.001f * (float)_gTime.ElapsedGameTime.TotalMilliseconds;
            float X = (float)(-15 + (Camera3D.RenderDistance * 7) * Math.Cos(MetaData));
            float Y = (float)(150 + (Camera3D.RenderDistance * 5) * Math.Sin(MetaData));

            Postion = new Vector3( X, Y, 0);
        }

        public void Render()
        {
            Matrix[] _Bones = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(_Bones);

            foreach (var _Mesh in Model.Meshes)
            {
                foreach (BasicEffect _Effect in _Mesh.Effects)
                {
                    _Effect.View = Camera3D.ViewMatrix;
                    _Effect.Projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 20000) ;

                    _Effect.World = _Bones[_Mesh.ParentBone.Index] * Matrix.CreateScale(50) * Matrix.Identity * Matrix.CreateTranslation(Postion);

                    _Effect.TextureEnabled = true;
                    _Effect.Texture = Textures["Standard"];

                    _Effect.CurrentTechnique.Passes[0].Apply();
                }

                _Mesh.Draw();
            }
        }
    }
}
