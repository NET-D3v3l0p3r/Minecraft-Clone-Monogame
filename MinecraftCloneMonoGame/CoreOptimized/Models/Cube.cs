using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
namespace MinecraftClone.CoreII.Models
{
    public class Cube
    {
        private VertexPositionNormalTexture[] Vertices = new VertexPositionNormalTexture[8];
        private ushort[] Indices = new ushort[36];

        public static VertexBuffer VertexBuffer { get; private set; }
        public static IndexBuffer IndexBuffer { get; private set; }

        public static void Initialize()
        {
            new Cube().SetUpVertices().SetUpIndices();
        }
        private Cube SetUpVertices()
        {
            //Vector2 textureTopLeft = new Vector2(1.0f * Size.X, 0.0f * Size.Y);
            //Vector2 textureTopRight = new Vector2(0.0f * Size.X, 0.0f * Size.Y);
            //Vector2 textureBottomLeft = new Vector2(1.0f * Size.X, 1.0f * Size.Y);
            //Vector2 textureBottomRight = new Vector2(0.0f * Size.X, 1.0f * Size.Y);

            Vertices[0] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector2(0, 0));
            Vertices[1] = new VertexPositionNormalTexture(new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector2(0, 0));
            Vertices[2] = new VertexPositionNormalTexture(new Vector3(1, 1, 0), new Vector3(0, 0, 1), new Vector2(0, 0));

            Vertices[3] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector2(0, 0));
            Vertices[4] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(0, 0, 0), new Vector2(0, 0));
            Vertices[5] = new VertexPositionNormalTexture(new Vector3(0, 1, -1), new Vector3(0, 0, 0), new Vector2(0, 0));

            Vertices[6] = new VertexPositionNormalTexture(new Vector3(1, 1, -1), new Vector3(0, 1, 0), new Vector2(0, 0));
            Vertices[7] = new VertexPositionNormalTexture(new Vector3(1, 0, -1), new Vector3(0, 0, 0), new Vector2(0, 0));

            VertexBuffer = new VertexBuffer(Global.GlobalShares.GlobalDevice, typeof(VertexPositionNormalTexture), 8, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionNormalTexture>(Vertices);

            return this;
        }
        private Cube SetUpIndices()
        {

            //Front face
            //bottom right triangle
            Indices[0] = 0;
            Indices[1] = 3;
            Indices[2] = 2;
            //top left triangle
            Indices[3] = 2;
            Indices[4] = 1;
            Indices[5] = 0;
            //back face
            //bottom right triangle
            Indices[6] = 4;
            Indices[7] = 7;
            Indices[8] = 6;
            //top left triangle
            Indices[9] = 6;
            Indices[10] = 5;
            Indices[11] = 4;
            //Top face
            //bottom right triangle
            Indices[12] = 1;
            Indices[13] = 2;
            Indices[14] = 6;
            //top left triangle
            Indices[15] = 6;
            Indices[16] = 5;
            Indices[17] = 1;
            //bottom face
            //bottom right triangle
            Indices[18] = 4;
            Indices[19] = 7;
            Indices[20] = 3;
            //top left triangle
            Indices[21] = 3;
            Indices[22] = 0;
            Indices[23] = 4;
            //left face
            //bottom right triangle
            Indices[24] = 4;
            Indices[25] = 0;
            Indices[26] = 1;
            //top left triangle
            Indices[27] = 1;
            Indices[28] = 5;
            Indices[29] = 4;
            //right face
            //bottom right triangle
            Indices[30] = 3;
            Indices[31] = 7;
            Indices[32] = 6;
            //top left triangle
            Indices[33] = 6;
            Indices[34] = 2;
            Indices[35] = 3;

            IndexBuffer = new IndexBuffer(Global.GlobalShares.GlobalDevice, IndexElementSize.SixteenBits, sizeof(ushort) * Indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData<ushort>(Indices);
            return this;
        }

    }
}
