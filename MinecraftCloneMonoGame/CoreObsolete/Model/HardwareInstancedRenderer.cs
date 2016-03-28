using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core.Model;
using System.Threading.Tasks;
using MinecraftClone.CoreII.Models;
using MinecraftClone.CoreII.Chunk;
using MinecraftClone.CoreII.Global;

namespace MinecraftClone.Core.Model
{
    public class HardwareInstancedRenderer
    {
        public Vector2[] TextureBufferArray;
        public Matrix[] MatrixBufferArray;

        public List<Vector2> TextureBuffer { get; set; }
        public List<Matrix> MatrixBuffer { get; set; }

        public Matrix WorldMatrix { get; set; }


        private DynamicVertexBuffer InstancedVertexBuffer;
        private DynamicVertexBuffer InstancedTextureBuffer;

        private static Effect InstancingShader;
        private bool Initialized;

        static VertexDeclaration InstancedVertexDeclaration = new VertexDeclaration
        (
          new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
          new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
          new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
          new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );
        static VertexDeclaration InstancedTextureIDs = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        private Texture2D[] Texture2Ds;

        public HardwareInstancedRenderer()
        {
            WorldMatrix = Matrix.Identity;

            if (!Initialized)
            {
                while (true)
                {
                    try
                    {
                        InstancingShader = GlobalShares.GlobalContent.Load<Effect>("MainShader");
                        break;
                    }
                    catch { }
                }
                Initialized = true;
            }


            Texture2Ds = new Texture2D[16];

            TextureBufferArray = new Vector2[0];
            MatrixBufferArray = new Matrix[0];

            TextureBuffer = new List<Vector2>();
            MatrixBuffer = new List<Matrix>();

        }

     

        public void BindTexture(Texture2D texture, int index)
        {
            if (index > 15)
                throw new Exception("Maximum index: 16");

            Texture2Ds[index] = texture;
        }

        public void ResizeInstancing(int size)
        {
            Array.Resize(ref TextureBufferArray, size);
            Array.Resize(ref MatrixBufferArray, size);
        }

        public void Apply()
        {
            MatrixBufferArray = MatrixBuffer.ToArray();
            TextureBufferArray = TextureBuffer.ToArray();
            ChunkManager.PullingShaderData = true;
        }

        public bool Render()
        {
            if (MatrixBufferArray.Length == 0)
                return false;
            if ((InstancedVertexBuffer == null) || (MatrixBufferArray.Length > InstancedVertexBuffer.VertexCount))
            {
                if (InstancedVertexBuffer != null)
                {
                    InstancedVertexBuffer.Dispose();
                    InstancedTextureBuffer.Dispose();

                }

                InstancedVertexBuffer = new DynamicVertexBuffer(GlobalShares.GlobalDevice, InstancedVertexDeclaration, MatrixBufferArray.Length, BufferUsage.WriteOnly);
                InstancedTextureBuffer = new DynamicVertexBuffer(GlobalShares.GlobalDevice, InstancedTextureIDs, TextureBufferArray.Length, BufferUsage.WriteOnly);
            }


            InstancedVertexBuffer.SetData<Matrix>(MatrixBufferArray, 0, MatrixBufferArray.Length, SetDataOptions.Discard);
            InstancedTextureBuffer.SetData<Vector2>(TextureBufferArray, 0, TextureBufferArray.Length, SetDataOptions.Discard);

            GlobalShares.GlobalDevice.SetVertexBuffers(
                new VertexBufferBinding(Cube.VertexBuffer, 0, 0),
                new VertexBufferBinding(InstancedVertexBuffer, 0, 1),
                new VertexBufferBinding(InstancedTextureBuffer, 0, 1));

            GlobalShares.GlobalDevice.Indices = Cube.IndexBuffer;

            InstancingShader.CurrentTechnique = InstancingShader.Techniques["HardwareInstancing"];

            InstancingShader.Parameters["World"].SetValue(WorldMatrix);
            InstancingShader.Parameters["View"].SetValue(Camera3D.ViewMatrix);
            InstancingShader.Parameters["Projection"].SetValue(Camera3D.ProjectionMatrix);
            InstancingShader.Parameters["EyePosition"].SetValue(Camera3D.CameraPosition);

            for (int i = 0; i < Texture2Ds.Length; i++)
            {
                if(Texture2Ds[i] != null)
                    InstancingShader.Parameters["Texture" + i].SetValue(Texture2Ds[i]);
            }

            InstancingShader.Parameters["FogEnabled"].SetValue(1.0f);
            InstancingShader.Parameters["FogColor"].SetValue(Color.CornflowerBlue.ToVector3());
            InstancingShader.Parameters["FogStart"].SetValue(0.0f);
            InstancingShader.Parameters["FogEnd"].SetValue(208.0f );


            foreach (EffectPass pass in InstancingShader.CurrentTechnique.Passes)
            {
                pass.Apply();
                GlobalShares.GlobalDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 1, 0, 12, MatrixBufferArray.Length);

            }

            return true;


        }
    }
}
