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
    public class HardwareInstancedRenderer : IDisposable
    {
        public Vector2[] TextureBufferArray;
        public Matrix[] MatrixBufferArray;

        public bool Invalidate { get; set; }

        public List<Vector2> TextureBuffer { get; set; }
        public List<Matrix> MatrixBuffer { get; set; }

        public Matrix WorldMatrix { get; set; }

        private DynamicVertexBuffer InstancedVertexBuffer;
        private DynamicVertexBuffer InstancedTextureBuffer;

        private static Effect InstancingShader;
        private static bool Initialized;

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
        private Microsoft.Xna.Framework.Graphics.Model Model;

        public HardwareInstancedRenderer()
        {
            WorldMatrix = Matrix.Identity;
            //MONOGAME BUG
            while (!Initialized)
            {
                try
                {
                    InstancingShader = GlobalShares.GlobalContent.Load<Effect>("MainShader");
                    Initialized = true;
                }
                catch { }
            }
            Model = GlobalModels.IndexModelTuple[0];
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
            if ((InstancedVertexBuffer == null) || (MatrixBufferArray.Length > InstancedVertexBuffer.VertexCount) || Invalidate )
            {
                if (InstancedVertexBuffer != null)
                {
                    InstancedVertexBuffer.Dispose();
                    InstancedTextureBuffer.Dispose();

                }

                InstancedVertexBuffer = new DynamicVertexBuffer(GlobalShares.GlobalDevice, InstancedVertexDeclaration, MatrixBufferArray.Length, BufferUsage.WriteOnly);
                InstancedTextureBuffer = new DynamicVertexBuffer(GlobalShares.GlobalDevice, InstancedTextureIDs, TextureBufferArray.Length, BufferUsage.WriteOnly);

                InstancedVertexBuffer.SetData<Matrix>(MatrixBufferArray, 0, MatrixBufferArray.Length, SetDataOptions.Discard);
                InstancedTextureBuffer.SetData<Vector2>(TextureBufferArray, 0, TextureBufferArray.Length, SetDataOptions.Discard);
                Invalidate = false;
            }

            for (int i = 0; i < Texture2Ds.Length; i++)
            {
                if (Texture2Ds[i] != null)
                    InstancingShader.Parameters["Texture" + i].SetValue(Texture2Ds[i]);
            }

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    GlobalShares.GlobalDevice.SetVertexBuffers(
                    new VertexBufferBinding(meshPart.VertexBuffer, 0, 0),
                    new VertexBufferBinding(InstancedVertexBuffer, 0, 1),
                    new VertexBufferBinding(InstancedTextureBuffer, 0, 1));

                    GlobalShares.GlobalDevice.Indices = meshPart.IndexBuffer;

                    InstancingShader.CurrentTechnique = InstancingShader.Techniques["HardwareInstancing"];

                    InstancingShader.Parameters["World"].SetValue(WorldMatrix);
                    InstancingShader.Parameters["View"].SetValue(Camera3D.ViewMatrix);
                    InstancingShader.Parameters["Projection"].SetValue(Camera3D.ProjectionMatrix);
                    InstancingShader.Parameters["EyePosition"].SetValue(Camera3D.CameraPosition);

                    InstancingShader.Parameters["FogEnabled"].SetValue(1.0f);
                    InstancingShader.Parameters["FogColor"].SetValue(Color.CornflowerBlue.ToVector3());
                    InstancingShader.Parameters["FogStart"].SetValue(0.0f);
                    if (!Camera3D.IsUnderWater) //TODO
                        InstancingShader.Parameters["FogEnd"].SetValue((float)Camera3D.RenderDistance);
                    else
                    {
                        InstancingShader.Parameters["FogEnd"].SetValue(55.0f);
                        InstancingShader.Parameters["FogColor"].SetValue(new Color(0, 162, 232, 255).ToVector3());
                    }
                    InstancingShader.CurrentTechnique.Passes[0].Apply();
                    GlobalShares.GlobalDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 1, 0, 12, MatrixBufferArray.Length);



                }
            }

            return true;
        }

        public void Dispose()
        {
            TextureBuffer.Clear();
            TextureBuffer = null;
            MatrixBuffer.Clear();
            MatrixBuffer = null;

            TextureBufferArray = new Vector2[0];
            MatrixBufferArray = new Matrix[0];

            Texture2Ds = new Texture2D[0];

            if (InstancedVertexBuffer != null)
                InstancedVertexBuffer.Dispose();
            if (InstancedTextureBuffer != null)
                InstancedTextureBuffer.Dispose();

            //InstancingShader.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
