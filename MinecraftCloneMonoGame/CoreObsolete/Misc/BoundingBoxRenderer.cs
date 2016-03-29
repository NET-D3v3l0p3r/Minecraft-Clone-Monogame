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

        public static bool Intersects(BoundingBox aabb, Ray ray)
        {
            var DirFrac = new Vector3(1.0f / ray.Direction.X, 1.0f / ray.Direction.Y, 1.0f / ray.Direction.Z);

            float t1 = (aabb.Min.X - ray.Position.X) * DirFrac.X;
            float t2 = (aabb.Max.X - ray.Position.X) * DirFrac.X;
            float t3 = (aabb.Min.Y - ray.Position.Y) * DirFrac.Y;
            float t4 = (aabb.Max.Y - ray.Position.Y) * DirFrac.Y;
            float t5 = (aabb.Min.Z - ray.Position.Z) * DirFrac.Z;
            float t6 = (aabb.Max.Z - ray.Position.Z) * DirFrac.Z;

            float tmin = MathHelper.Max(MathHelper.Max(MathHelper.Min(t1, t2), MathHelper.Min(t3, t4)), MathHelper.Min(t5, t6));
            float tmax = MathHelper.Min(MathHelper.Min(MathHelper.Max(t1, t2), MathHelper.Max(t3, t4)), MathHelper.Max(t5, t6));

            if (tmax < 0)
                return false;
            if (tmin > tmax)
                return false;

            return true;
        }

        public static bool IntersectRayVsBox(BoundingBox a_kBox,
                         Ray a_kRay,
                         out float a_fDist,
                         out int a_nFace)
        {
            a_nFace = -1;
            a_fDist = float.MaxValue;

            // Preform the collision query  
            float? fParam = a_kRay.Intersects(a_kBox);

            // No collision, return false.  
            if (!fParam.HasValue)
                return false;

            // Asign the distance along the ray our intersection point is  
            a_fDist = fParam.Value;

            // Compute the intersection point  
            Vector3 vIntersection = a_kRay.Position + a_kRay.Direction * a_fDist;

            // Determine the side of the box the ray hit, this is slower than  
            // more obvious methods but it's extremely tolerant of numerical  
            // drift (aka rounding errors)  
            Vector3 vDistMin = vIntersection - a_kBox.Min;
            Vector3 vDistMax = vIntersection - a_kBox.Max;

            vDistMin.X = (float)Math.Abs(vDistMin.X);
            vDistMin.Y = (float)Math.Abs(vDistMin.Y);
            vDistMin.Z = (float)Math.Abs(vDistMin.Z);

            vDistMax.X = (float)Math.Abs(vDistMax.X);
            vDistMax.Y = (float)Math.Abs(vDistMax.Y);
            vDistMax.Z = (float)Math.Abs(vDistMax.Z);
           

            // Start off assuming that our intersection point is on the  
            // negative x face of the bounding box.  
            a_nFace = 0;
            float fMinDist = vDistMin.X;

            // +X  
            if (vDistMax.X < fMinDist)
            {
                a_nFace = 1;
                fMinDist = vDistMax.X;
            }

            // -Y  
            if (vDistMin.Y < fMinDist)
            {
                a_nFace = 2;
                fMinDist = vDistMin.Y;
            }

            // +Y  
            if (vDistMax.Y < fMinDist)
            {
                a_nFace = 3;
                fMinDist = vDistMax.Y;
            }

            // -Z  
            if (vDistMin.Z < fMinDist)
            {
                a_nFace = 4;
                fMinDist = vDistMin.Z;
            }

            // +Z  
            if (vDistMax.Z < fMinDist)
            {
                a_nFace = 5;
                fMinDist = vDistMin.Z;
            }

            return true;
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
