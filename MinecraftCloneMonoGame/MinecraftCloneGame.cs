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
using MinecraftClone.Core.Model;
using MinecraftClone.Core;
using MinecraftClone.Core.Model.Types;
using MinecraftClone.Core.MapGenerator;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MinecraftClone.CoreII.Chunk;
using MinecraftClone.CoreII.Profiler;
using MinecraftClone.CoreII;
using Core.MapGenerator;
using MinecraftCloneMonoGame.CoreOptimized.Global;
namespace MinecraftClone
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MinecraftCloneGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public ChunkManager ChunkManager;
        Input InputManager;

        Camera3D Camera;

        bool RenderDebug;
        Texture2D Crosshair;
        SpriteFont DebugFont;

        private FpsCounter FpsCounter;
        private Profile? Profile;

        public MinecraftCloneGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //MONOGAME BUG
            this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferredBackBufferWidth = 1080;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            CoreII.Global.GlobalShares.GlobalContent = Content;
            CoreII.Global.GlobalShares.GlobalDevice = GraphicsDevice;
            CoreII.Global.GlobalShares.GlobalDeviceManager = graphics;

            ChunkManager = new ChunkManager();
            ChunkManager.Width = 20;
            ChunkManager.Depth = 20;


            Camera = new Camera3D(0.0035f, 0.35f);

            ChunkOptimized.RenderingBufferSize = 2048;


            ChunkManager.Start(154, GlobalShares.GetRandomWord(15));
   
            InputManager = new Input();
            FpsCounter = new CoreII.Profiler.FpsCounter();


            Camera3D.CameraPosition = new Vector3((ChunkManager.Width / 2) * 16, 200, (ChunkManager.Depth / 2) * 16);
            Crosshair = Content.Load<Texture2D>(@"Textures\cross_cross");
            DebugFont = Content.Load<SpriteFont>(@"DebugFont");

            for (int i = 0; i < 16; i++)
            {
                GraphicsDevice.SamplerStates[i] = SamplerState.PointWrap;
            }

            InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.F11, new Action(() => { }), new Action(() => { RenderDebug = !RenderDebug; }), true));

            InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.S, new Action(() => { }), new Action(() => { Camera3D.Move(new Vector3(0, 0, 1)); }), false));
            InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.W, new Action(() => { }), new Action(() => { Camera3D.Move(new Vector3(0, 0, -1)); }), false));
            InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.D, new Action(() => { }), new Action(() => { Camera3D.Move(new Vector3(1, 0, 0)); }), false));
            InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.A, new Action(() => { }), new Action(() => { Camera3D.Move(new Vector3(-1, 0, 0)); }), false));

            InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.Escape, new Action(() => { }), new Action(() => { Exit(); }), true));

            Camera3D.Game = this;
        }
        protected override void UnloadContent()
        {

        }


        protected override void Update(GameTime gameTime)
        {
            FpsCounter.Start(gameTime);

            ChunkManager.PullingShaderData = ChunkManager.UploadingShaderData = false;

            Camera3D.Update(gameTime);
            InputManager.Update(gameTime);
            ChunkManager.Update(gameTime);

            Profile = ChunkManager.GetFocusedCube(13.0f);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            ChunkManager.RenderChunks();
            DrawDebug();



            base.Draw(gameTime);
        }

        public Texture2D GetTextureFromColor(GraphicsDevice device, Color color)
        {
            Texture2D tex = new Texture2D(device, 1, 1);
            tex.SetData<Color>(new Color[] { color });
            return tex;
        }

        public void DrawDebug()
        {
            var FPS = FpsCounter.End();
            spriteBatch.Begin();

            if (!ChunkManager.Generated)
            {
                spriteBatch.DrawString(DebugFont, "LOADING...", new Vector2(MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 25, MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2), Color.Black);
                spriteBatch.End();
                return;
            }

            if (Profile.HasValue)
                BoundingBoxRenderer.Render(Profile.Value.AABB, GraphicsDevice, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Yellow);
            
            
            if (RenderDebug)
                spriteBatch.DrawString(DebugFont,
                    "PROGRAMMED _ BY PhiConst" + Environment.NewLine +
                    "FPS: " + FPS + Environment.NewLine +
                    "ALLOCATED_MEMORY: " + ((GC.GetTotalMemory(false)/ 1024f) / 1024f) + "MB" + Environment.NewLine +
                    "MAP_IS_GENERATED: " + ChunkManager.Generated + Environment.NewLine +
                    "MAP_WIDTH: " + (ChunkManager.Width * 16) + Environment.NewLine +
                    "MAP_DEPTH: " + (ChunkManager.Depth * 16) + Environment.NewLine +
                    "RENDER_BUFFER_SIZE: " + ChunkOptimized.RenderingBufferSize + Environment.NewLine +
                    "SEED: " + ChunkManager.Seed + Environment.NewLine + 
                    "UPDATING_CHUNKS: " + ChunkManager.UpdatingChunks + Environment.NewLine +
                    "TOTAL_UPDATE: " + ChunkManager.TotalUpdate + Environment.NewLine +
                    "PULLING_SHADER_DATA: " + ChunkManager.PullingShaderData + Environment.NewLine +
                    "UPLOAD_SHADER_DATA: " + ChunkManager.UploadingShaderData + Environment.NewLine +
                    "DATA_RECEIVED: " + ChunkManager.TotalRender + " per chunk" + Environment.NewLine +
                    "LOOKING_AT_CUBE: " + (Profile.HasValue ?  Enum.GetName(typeof(MinecraftClone.CoreII.Global.GlobalShares.Face), Profile.Value.Face )+ "" : "FALSE") + Environment.NewLine +
                    "POSITION {X:" + (int)(Camera3D.CameraPosition.X / 1.0f) + " Y:" + (int)(Camera3D.CameraPosition.Y / 1.0f) + " Z:" + (int)(Camera3D.CameraPosition.Z / 1.0f) + "}" + Environment.NewLine + 
                    "DIRECTION(STATIONARY): " + (Camera3D.CameraDirectionStationary) + Environment.NewLine +
                    "QUARTER:" + Camera3D.GetQuarter(Camera3D.CameraDirectionStationary) + Environment.NewLine
                    , new Vector2(0, 0), Color.Yellow);


            spriteBatch.Draw(Crosshair, new Rectangle((int)(CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 7.5), (int)(CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2), 15, 15), Color.DarkGray);
            spriteBatch.End();
        }
    }
}
