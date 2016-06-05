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
using Earlz.BareMetal;
using System.Threading;
using MinecraftCloneMonoGame.CoreOptimized.Misc;
using MinecraftCloneMonoGame.Multiplayer;
using MinecraftCloneMonoGame.Multiplayer.Global;
namespace MinecraftClone
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MinecraftCloneGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager _GraphicsDeviceManager;
        SpriteBatch spriteBatch;

        ChunkManager _ChunkManager;
        Input _InputManager;

        Camera3D _Camera;

        bool _RenderDebug;
        Texture2D _Crosshair;
        SpriteFont _DebugFont;

        FpsCounter _FpsCounter;
        Profile? _Profile;

        bool _StartGame;

        //Test
        RenderTarget2D _RenderTarget;
        Effect _Blur;

        GravitationController _GravitationController;

        public MinecraftCloneGame()
        {
            _GraphicsDeviceManager = new GraphicsDeviceManager(this);
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
            _GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            _GraphicsDeviceManager.PreferredBackBufferWidth = 1920;
            _GraphicsDeviceManager.PreferredBackBufferHeight = 1080;
            _GraphicsDeviceManager.ApplyChanges();

            
            base.Initialize();
        }
        GlobalOnlinePlayer _GlobalPlayer;
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures

            spriteBatch = new SpriteBatch(GraphicsDevice);


            CoreII.Global.GlobalShares.GlobalContent = Content;
            CoreII.Global.GlobalShares.GlobalDevice = GraphicsDevice;
            CoreII.Global.GlobalShares.GlobalDeviceManager = _GraphicsDeviceManager;

            _ChunkManager = new ChunkManager();

            ChunkManager.Algorithm = ChunkManager.GeneratorAlgorithm.SimplexNoise;
            ChunkManager.Width = 44;
            ChunkManager.Depth = 44;

            Camera3D.Game = this;

            _Camera = new Camera3D(0.0035f, 0.25f);

            ChunkOptimized.Width = ChunkOptimized.Depth = 16;
            ChunkOptimized.Height = 256;
            ChunkOptimized.RenderingBufferSize = 4096;

            _Blur = Content.Load<Effect>(@"PostProcessing\PostProcessingEffect");

            float ReservedMB = (((BareMetal.SizeOf<DefaultCubeClass>() * (ChunkOptimized.Width * ChunkOptimized.Height * ChunkOptimized.Depth)) / 1024f) / 1024f);

            Console.WriteLine("::RESERVED " + ReservedMB + " MB" + Environment.NewLine +
                              "FOR DEFAULTCUBESTRUCTURE::");
            Console.WriteLine("::IN TOTAL: " + ReservedMB * (ChunkManager.Width * ChunkManager.Depth) + " MB!");

            _InputManager = new Input();
            _FpsCounter = new CoreII.Profiler.FpsCounter();

            _GravitationController = new GravitationController(5.972E24, new Vector3(0, -6000000, 0));
            _GravitationController.DeltaTime = 0.025f;
            _GravitationController.Damping = 0.549f;
            _GravitationController.Friction = 0.05f;

            Camera3D.CameraPosition = new Vector3((ChunkManager.Width / 2) * 16, 135, (ChunkManager.Depth / 2) * 16);
            _Crosshair = Content.Load<Texture2D>(@"Textures\cross_cross");
            _DebugFont = Content.Load<SpriteFont>(@"DebugFont");

            _RenderTarget = new RenderTarget2D(
                            GraphicsDevice,
                            GraphicsDevice.PresentationParameters.BackBufferWidth,
                            GraphicsDevice.PresentationParameters.BackBufferHeight,
                            false,
                            GraphicsDevice.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);

            for (int i = 0; i < 16; i++)
            {
                GraphicsDevice.SamplerStates[i] = SamplerState.PointWrap;
            }

            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.F11, new Action(() => { }), new Action(() => { _RenderDebug = !_RenderDebug; }), true));

            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.S, new Action(() => { }), new Action(() =>
            {
                Camera3D.Move(new Vector3(0, 0, 1));

            }), false));
            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.W, new Action(() => { }), new Action(() =>
            {
                Camera3D.Move(new Vector3(0, 0, -1));
            })
                , false));
            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.D, new Action(() => { }), new Action(() =>
            {
                Camera3D.Move(new Vector3(1, 0, 0));
            })
                , false));
            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.A, new Action(() => { }), new Action(() =>
            {
                Camera3D.Move(new Vector3(-1, 0, 0));
            })
                , false));

            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.Escape, new Action(() => { }), new Action(() => { Exit(); }), true));
            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.F5, new Action(() => { }), new Action(() =>
            {
                ChunkManager.Generated = false;
                _ChunkManager.RunGeneration();

            }), true));


            _InputManager.KeyList.Add(new Core.Camera.Key.KeyData(Keys.F12, new Action(() => { }), new Action(() =>
            {
                var _IPeP = Microsoft.VisualBasic.Interaction.InputBox("CONNECT TO HOST[PATTERN: IP:PORT]:").Split(':');

                LocalOnlinePlayer _Player = new LocalOnlinePlayer(8215, _IPeP[0], int.Parse(_IPeP[1]));
                _Player.BindOnKeyboard(_InputManager);
                _Player.BindOnCamera();

            }), true));

            //GUI
            this.IsMouseVisible = true;
            var frm = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            frm.Focus();
            frm.KeyDown += new System.Windows.Forms.KeyEventHandler((object sender, System.Windows.Forms.KeyEventArgs e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Escape)
                    this.Exit();
            });

            frm.Location = new System.Drawing.Point(0, 0);
            frm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //frm.TopMost = true;
            frm.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            System.Windows.Forms.TextBox _SeedTB = new System.Windows.Forms.TextBox();
            _SeedTB.Font = new System.Drawing.Font("Serial", 11);
            _SeedTB.Multiline = true;
            _SeedTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _SeedTB.BackColor = System.Drawing.Color.SlateGray;
            _SeedTB.Size = new System.Drawing.Size(150, 25);
            _SeedTB.Location = new System.Drawing.Point(MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 60,
                MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2);
            _SeedTB.KeyDown += new System.Windows.Forms.KeyEventHandler((object sender, System.Windows.Forms.KeyEventArgs e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Enter)
                {
                    _ChunkManager.Start(99, _SeedTB.Text);
                    _ChunkManager.RunGeneration();
                    frm.Controls.Clear();
                    _StartGame = true;
                    IsMouseVisible = false;
                    e.SuppressKeyPress = true;
                }
            });
            frm.Controls.Add(_SeedTB);

            System.Windows.Forms.Label _SeedDescrp = new System.Windows.Forms.Label();
            _SeedDescrp.Font = new System.Drawing.Font("Serial", 11);
            _SeedDescrp.Size = new System.Drawing.Size(45, 22);
            _SeedDescrp.BackColor = System.Drawing.Color.CornflowerBlue;
            _SeedDescrp.Location = new System.Drawing.Point(MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 90 - 15,
                MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2 + 2);
            _SeedDescrp.Text = "Seed:";

            frm.Controls.Add(_SeedDescrp);


            System.Windows.Forms.RadioButton _GeneratorSimplexCB = new System.Windows.Forms.RadioButton();
            _GeneratorSimplexCB.Size = new System.Drawing.Size(130, 17);
            _GeneratorSimplexCB.Font = new System.Drawing.Font("Serial", 11);
            _GeneratorSimplexCB.Text = "SimplexNoise";
            _GeneratorSimplexCB.Location = new System.Drawing.Point(MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 55,
                MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2 + 35);
            _GeneratorSimplexCB.BackColor = System.Drawing.Color.CornflowerBlue;
            _GeneratorSimplexCB.CheckedChanged += new EventHandler((object sender, EventArgs e) =>
            {
                ChunkManager.Algorithm = ChunkManager.GeneratorAlgorithm.SimplexNoise;
            });
            _GeneratorSimplexCB.Checked = true;
            frm.Controls.Add(_GeneratorSimplexCB);


            System.Windows.Forms.RadioButton _GeneratorDiamondCB = new System.Windows.Forms.RadioButton();
            _GeneratorDiamondCB.Size = new System.Drawing.Size(140, 17);
            _GeneratorDiamondCB.Font = new System.Drawing.Font("Serial", 11);
            _GeneratorDiamondCB.Text = "DiamondSquare";
            _GeneratorDiamondCB.Location = new System.Drawing.Point(MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 55,
                MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2 + 70);
            _GeneratorDiamondCB.BackColor = System.Drawing.Color.CornflowerBlue;
            _GeneratorDiamondCB.CheckedChanged += new EventHandler((object sender, EventArgs e) =>
            {
                ChunkManager.Algorithm = ChunkManager.GeneratorAlgorithm.DiamondSquare;
            });
            frm.Controls.Add(_GeneratorDiamondCB);


            System.Windows.Forms.Label _GeneratorDescp = new System.Windows.Forms.Label();
            _GeneratorDescp.Font = new System.Drawing.Font("Serial", 11);
            _GeneratorDescp.Size = new System.Drawing.Size(75, 22);
            _GeneratorDescp.BackColor = System.Drawing.Color.CornflowerBlue;
            _GeneratorDescp.Location = new System.Drawing.Point(MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 90 - 43,
                MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2 + 35);
            _GeneratorDescp.Text = "Algorithm:";

            frm.Controls.Add(_GeneratorDescp);

            System.Windows.Forms.TrackBar _RenderDistance = new System.Windows.Forms.TrackBar();

            _GlobalPlayer = new GlobalOnlinePlayer("77.21.164.133", 8000);


        }
        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (!_StartGame)
                return;
            _FpsCounter.Start(gameTime);

            ChunkManager.PullingShaderData = ChunkManager.UploadingShaderData = false;

            Camera3D.Update(gameTime);
            _InputManager.Update(gameTime);
            _ChunkManager.Update(gameTime);

            if(Keyboard.GetState().IsKeyDown(Keys.F3))
                _GlobalPlayer.SendEcho(); 

            //_GravitationController.Update(gameTime);

            if (Camera3D.IsChangigView || Camera3D.isMoving)
                _Profile = ChunkManager.GetFocusedCube(4.0f);

            if (Camera3D.IsUnderWater)
                Camera3D.MovementSpeed = 0.15f;
            else Camera3D.MovementSpeed = 0.25f;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                if (_Profile.HasValue)
                {
                    _Profile.Value.Chunk.Pop((int)_Profile.Value.Index);
                    _Profile = null;
                }
            _Blur.Parameters["_Value"].SetValue((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 0.015f);
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {


            var ClearColor = Color.CornflowerBlue;
            if (Camera3D.IsUnderWater)
                ClearColor = new Color(0, 162, 232, 255);
            GraphicsDevice.SetRenderTarget(_RenderTarget);
            GraphicsDevice.Clear(ClearColor);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            //if (ChunkManager.CurrentChunk != null)
            //    foreach (var chunk in ChunkManager.CurrentChunk.SurroundingChunks)
            //        if (chunk != null)
            //            BoundingBoxRenderer.Render(chunk.ChunkArea, MinecraftClone.CoreII.Global.GlobalShares.GlobalDevice, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Black);

            if (!_StartGame)
            {
                GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin( );
                spriteBatch.Draw(_RenderTarget, new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                spriteBatch.End();
                return;
            }
            if (_Profile.HasValue)
                BoundingBoxRenderer.Render(_Profile.Value.AABB, GraphicsDevice, Camera3D.ViewMatrix, Camera3D.ProjectionMatrix, Color.Yellow);

            _ChunkManager.RenderChunks();
            GraphicsDevice.SetRenderTarget(null);

            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            if (Camera3D.IsUnderWater)
                _Blur.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(_RenderTarget, new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
            spriteBatch.End();

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
            var FPS = _FpsCounter.End();
            spriteBatch.Begin();

            if (!ChunkManager.Generated)
            {
                spriteBatch.DrawString(_DebugFont, "GENERATING WORLD...: ",
                    new Vector2(MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 90, MinecraftClone.CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2), Color.Black);
                spriteBatch.End();
                return;
            }

            if (_RenderDebug)
                spriteBatch.DrawString(_DebugFont,
                    "PROGRAMMED _ BY PhiConst" + Environment.NewLine +
                    "FPS: " + FPS + Environment.NewLine +
                    "ALLOCATED_MEMORY: " + ((GC.GetTotalMemory(false)/ 1024f) / 1024f) + "MB" + Environment.NewLine +
                    "MAP_IS_GENERATED: " + ChunkManager.Generated + Environment.NewLine +
                    "MAP_WIDTH: " + (ChunkManager.Width * 16) + Environment.NewLine +
                    "MAP_DEPTH: " + (ChunkManager.Depth * 16) + Environment.NewLine +
                    "RENDER_BUFFER_SIZE: " + ChunkOptimized.RenderingBufferSize + Environment.NewLine +
                    "SEED: " + ChunkManager.Seed + Environment.NewLine + 
                    "RENDERING_CHUNKS: " + ChunkManager.RenderingChunks + Environment.NewLine + 
                    "UPDATING_CHUNKS: " + ChunkManager.UpdatingChunks + Environment.NewLine +
                    "TOTAL_UPDATE: " + ChunkManager.TotalUpdate + Environment.NewLine +
                    "PULLING_SHADER_DATA: " + ChunkManager.PullingShaderData + Environment.NewLine +
                    "UPLOAD_SHADER_DATA: " + ChunkManager.UploadingShaderData + Environment.NewLine +
                    "DATA_RECEIVED: " + ChunkManager.TotalRender + " per chunk" + Environment.NewLine +
                    "LOOKING_AT_CUBE: " + (_Profile.HasValue ?  Enum.GetName(typeof(MinecraftClone.CoreII.Global.GlobalShares.Face), _Profile.Value.Face )+ "" : "FALSE") + Environment.NewLine +
                    "CURRENT_HEIGHT: " + Camera3D.CurrentHeight + Environment.NewLine +
                    "POSITION {X:" + (int)(Camera3D.CameraPosition.X / 1.0f) + " Y:" + (int)(Camera3D.CameraPosition.Y / 1.0f) + " Z:" + (int)(Camera3D.CameraPosition.Z / 1.0f) + "}" + Environment.NewLine + 
                    "MOVED_TO: "  + ChunkManager.MovedDirection + Environment.NewLine +
                    "DIRECTION(STATIONARY): " + (Camera3D.CameraDirectionStationary) + Environment.NewLine +
                    "QUARTER:" + Camera3D.GetQuarter(Camera3D.CameraDirectionStationary) + Environment.NewLine +
                    "UNDER_WATER: " + Camera3D.IsUnderWater
                    , new Vector2(0, 0), Color.Yellow);


            spriteBatch.Draw(_Crosshair, new Rectangle((int)(CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2 - 7.5), (int)(CoreII.Global.GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2 -  1), 15, 15), Color.DarkGray);
            spriteBatch.End();
        }
    }
}
