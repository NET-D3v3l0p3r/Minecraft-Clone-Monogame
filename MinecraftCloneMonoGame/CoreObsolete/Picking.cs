using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core;
using MinecraftClone.Core.Model.Types;
using MinecraftClone.Core.MapGenerator;
namespace MinecraftClone.Core
{
    public class Picking
    {
        private float Lambda;
        private Microsoft.Xna.Framework.Graphics.Model Model;
        private Matrix World;
        private Matrix View;
        private bool End;
        private string CurrentAction = "";

        public GraphicsDeviceManager Manager { get; set; }
        public GraphicsDevice Device { get; set; }
        public ContentManager Content { get; set; }

        public BoundingBox BoundingBox;
        public Vector3 Position;

        public Dictionary<string, Action> Actions { get; private set; }
        public int MaxLambda { get; set; }

        public Picking(GraphicsDeviceManager manager, GraphicsDevice device, ContentManager content)
        {
            Manager = manager;
            Device = device;
            Content = content;

            Model = Content.Load<Microsoft.Xna.Framework.Graphics.Model>(@"Cube\Cube1x1");
            Actions = new Dictionary<string, Action>();

            Lambda = 1;
            MaxLambda = 30;
            CurrentAction = "NOTHING";
            Actions.Add("NOTHING", new Action(() => { }));
        }

        public void SetUp(Matrix view)
        {
            if (End)
            {
                Lambda = 0;
                View = view;
                End = false;
            }
        }

        public void Exit()
        {
            End = true;
            Lambda = 0;
        }

        public void Update(GameTime gTime)
        {
            if (!End && Lambda < MaxLambda)
            {
                Lambda += 0.1f;
                World = Matrix.CreateScale(0.055f) * Matrix.CreateTranslation(new Vector3(0, 0, -Lambda)) * Matrix.Invert(View);
                Position = World.Translation;
                BoundingBox = BoundingBoxRenderer.UpdateBoundingBox(Model, World);
                if (Lambda < MaxLambda)
                {
                    Actions[CurrentAction].Invoke();
                }
                else End = true;
            }
        }
        public void SetCurrentAction(string action)
        {
            CurrentAction = action;
        }

        public void Render()
        {
            BoundingBoxRenderer.Render(BoundingBox, Device, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Red);
        }
    }
}
