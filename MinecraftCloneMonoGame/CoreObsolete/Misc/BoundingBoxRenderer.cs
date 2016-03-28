using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftClone.Core.Misc
{
    public static class BoundingBoxRenderer
    {
        static VertexPositionColor[] verts = new VertexPositionColor[8];
        static short[] indices = new short[]
	    {
		0, 1,
		1, 2,
		2, 3,
		3, 0,
		0, 4,
		1, 5,
		2, 6,
		3, 7,
		4, 5,
		5, 6,
		6, 7,
		7, 4,
    	};

        static BasicEffect effect;

        /// <summary>
        /// Renders the bounding box for debugging purposes.
        /// </summary>
        /// <param name="box">The box to render.</param>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="projection">The current projection matrix.</param>
        /// <param name="color">The color to use drawing the lines of the box.</param>
        public static void Render(
            BoundingBox box,
            GraphicsDevice graphicsDevice,
            Matrix view,
            Matrix projection,
            Color color)
        {
            if (effect == null)
            {
                effect = new BasicEffect(graphicsDevice);
                effect.VertexColorEnabled = true;
                effect.LightingEnabled = false;
            }

            Vector3[] corners = box.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                verts[i].Position = corners[i];
                verts[i].Color = color;
            }

            effect.View = view;
            effect.Projection = projection;
            effect.CurrentTechnique.Passes[0].Apply();

            graphicsDevice.DrawUserIndexedPrimitives(
            PrimitiveType.LineList,
            verts,
            0,
            8,
            indices,
            0,
            indices.Length / 2);
        }


        public static BoundingBox UpdateBoundingBox(Microsoft.Xna.Framework.Graphics.Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    try
                    {
                        meshPart.VertexBuffer.GetData<float>(vertexData);
                    }
                    catch { }
                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            // Create and return bounding box
            return new BoundingBox(min, max);
        }

        public static void RenderRay(GraphicsDevice device, Ray ray, Matrix projection, Matrix view)
        {
            Func<float, Vector3> RayLambda = (float lambda) =>
            {
                var xCoord = ray.Direction.X * lambda + ray.Position.X;
                var yCoord = ray.Direction.Y * lambda + ray.Position.Y;
                var zCoord = ray.Direction.Z * lambda + ray.Position.Z;
                return new Vector3(xCoord, yCoord, zCoord);
            };

            var pos1 = RayLambda(0);
            var pos2 = RayLambda(500);

            BasicEffect effect = new BasicEffect(device);
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;
            effect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, new VertexPositionColor[] { new VertexPositionColor(pos1, Color.Red), new VertexPositionColor(pos2, Color.Red) }, 0, 1);
        }

        public static float GetLambda(Ray ray, float y)
        {
            return (y - ray.Position.Y) / ray.Direction.Y;
        }


    }
}
