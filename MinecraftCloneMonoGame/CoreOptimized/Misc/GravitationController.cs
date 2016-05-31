using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using MinecraftClone.CoreII.Chunk;
using MinecraftClone.CoreII.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftCloneMonoGame.CoreOptimized.Misc
{
    public class GravitationController
    {

        public double PlanetWeight { get; set; }
        public Vector3 MassMainEmphasis { get; set; }

        public float Acceleration { get; private set; } //Y
        public float Velocity { get; private set; } //Y
         
        public float Friction { get; set; }
        public float Damping { get; set; }
        public float DeltaTime { get; set; }

        public double Distance { get; private set; }

        public GravitationController(double weight, Vector3 center)
        {
            PlanetWeight = weight;
            MassMainEmphasis = center;
        }

        public void Update(GameTime gTime)
        {
            if (ChunkManager.CurrentChunk != null)
            {
                foreach (var cube in ChunkManager.CurrentChunk.ChunkData)
                {
                    if (cube != null 
                        && cube.Id != (int) GlobalShares.Identification.Water 
                        && cube.CollisionBox.Intersects(Camera3D.CollisionBox))
                        return;
                }
                Distance = Camera3D.CameraPosition.Y - MassMainEmphasis.Y;
                Acceleration = (float)(-6.67408E-11 * PlanetWeight * (Camera3D.CameraPosition.Y - MassMainEmphasis.Y)) / (float)Math.Pow(Distance, 3);
                Velocity += (float)(Acceleration * DeltaTime);
                Camera3D.CameraPosition = new Vector3(Camera3D.CameraPosition.X, Camera3D.CameraPosition.Y + (float)(Velocity * DeltaTime), Camera3D.CameraPosition.Z);
            }
        }



    }
}
