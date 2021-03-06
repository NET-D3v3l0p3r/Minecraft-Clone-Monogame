﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using MinecraftClone.CoreII.Chunk;
using MinecraftClone.CoreII.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftClone.Core.Camera
{
    public class Camera3D
    {
        public static Vector3 Up = new Vector3(0, 1, 0);

        public static MinecraftCloneGame Game { get; set; }
        public static int RenderDistance { get; set; }

        private static float oldX, oldY;
        private static Vector3 ReferenceVector3 = new Vector3(0, 0, -1);
        private static Vector3 Position;
        private static Vector2 PitchYaw;

        private static bool Initialized;

        public static bool isMoving { get; set; }
        public static bool IsChangigView { get; set; }

        public static Vector3 CameraPosition;
        public static Vector3 CameraDirection;

        public static Vector3 CameraDirectionStationary;
        public static Ray Ray { get; private set; }

        public static bool IsUnderWater { get; private set; }

        public static float Yaw { get; private set; }
        public static float Pitch { get; private set; }

        public static Matrix ViewMatrix;
        public static Matrix ProjectionMatrix;
        public static BoundingFrustum ViewFrustum;

        public static float MouseDPI { get; set; }
        public static float MovementSpeed { get; set; }

        public static int CurrentHeight = 0;

        public static BoundingBox CollisionBox;

        public enum Quarter
        {
            North,
            East,
            South, 
            West,
            North_East,
            Nort_West,
            South_East,
            South_West,
            Down,
            Up,
            NULL

        }

        public Camera3D(float DPI, float Speed)
        {
            MouseDPI = DPI;
            MovementSpeed = Speed;

            RenderDistance = ChunkManager.Width * 4 * 2;
            CameraPosition = new Vector3(0, 215, 0);

            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,  GlobalShares.GlobalDevice.Viewport.AspectRatio, 0.1f, RenderDistance);

    

        }
        private static void CalculateViewMatrix()
        {
            var dX = Mouse.GetState().X - oldX;
            var dY = Mouse.GetState().Y - oldY;

            Pitch += -MouseDPI * dY;
            Yaw += -MouseDPI * dX;


            Pitch = MathHelper.Clamp(Pitch, -1.5f, 1.5f);

            if (!Initialized)
            {
                Pitch = 0.075f;
                Initialized = true;
            }
            

            Matrix RotationX = Matrix.Identity;
            Matrix RotationY = Matrix.Identity;

            Matrix.CreateRotationX(Pitch, out RotationX);
            Matrix.CreateRotationY(Yaw, out RotationY);

            Matrix Rotation = Matrix.Identity;

            Matrix.Multiply(ref RotationX, ref RotationY, out Rotation);

            Vector3 Transformed = Vector3.Transform(ReferenceVector3, Rotation);
            CameraDirectionStationary = Vector3.Zero + Transformed;
            CameraDirectionStationary = new Vector3((float)Math.Round(CameraDirectionStationary.X, 0), (float)Math.Round(CameraDirectionStationary.Y, 0), (float)Math.Round(CameraDirectionStationary.Z, 0));
            CameraDirection = CameraPosition + Transformed;

            Matrix.CreateLookAt(ref CameraPosition, ref CameraDirection, ref Up, out ViewMatrix);
            var mX = GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2.0f;
            var mY = GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2.0f; 
            Mouse.SetPosition((int)mX, (int)mY);

            oldX = mX;
            oldY = mY;
            //Unnormalize();

        }

        public static Quarter GetQuarter(Vector3 a)
        {
            if (a.X == -1 && a.Z == 0 && a.Y == 0)
                return Camera3D.Quarter.South;
            if (a.X == 1 && a.Z == 0 && a.Y == 0)
                return Camera3D.Quarter.North;
            if (a.X == 0 && a.Z == -1 && a.Y == 0)
                return Camera3D.Quarter.West;
            if (a.X == 0 && a.Z == 1 && a.Y == 0)
                return Camera3D.Quarter.East;

            if (a.X == 1 && a.Z == 1 && a.Y == 0)
                return Camera3D.Quarter.North_East;
            if (a.X == -1 && a.Z == 1 && a.Y == 0)
                return Camera3D.Quarter.South_East;

            if (a.X == 1 && a.Z == -1 && a.Y == 0)
                return Camera3D.Quarter.Nort_West;
            if (a.X == -1 && a.Z == -1 && a.Y == 0)
                return Camera3D.Quarter.South_West;

            if (a.Y == 1 && a.X == 0 && a.Z == 0)
                return Camera3D.Quarter.Up;
            if (a.Y == -1 && a.X == 0 && a.Z == 0)
                return Camera3D.Quarter.Down;

            return Quarter.NULL;

        }

        private static Ray CalculateRay()
        {
            Vector3 nearPlane = GlobalShares.GlobalDevice.Viewport.Unproject(new Vector3(GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2.0f, GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2.0f, 0), Camera3D.ProjectionMatrix, Camera3D.ViewMatrix, Matrix.Identity);
            Vector3 farPlane = GlobalShares.GlobalDevice.Viewport.Unproject(new Vector3(GlobalShares.GlobalDeviceManager.PreferredBackBufferWidth / 2.0f, GlobalShares.GlobalDeviceManager.PreferredBackBufferHeight / 2.0f, 1.0f), Camera3D.ProjectionMatrix, Camera3D.ViewMatrix, Matrix.Identity);

            Vector3 Direction = Vector3.Zero;
            Vector3.Subtract(ref farPlane, ref nearPlane, out Direction);

            Direction.Normalize();

            return new Ray(nearPlane, Direction);
        }

        public static void Move(Vector3 unit)
        {
            Matrix Rotation =  Matrix.CreateRotationY(Yaw);
            Vector3 TransformedVector = Vector3.Transform(unit, Rotation);

            TransformedVector *= MovementSpeed;

            bool RequestArea = false;

            if (RequestArea = !IsColliding(new Vector3(CameraPosition.X + TransformedVector.X, CameraPosition.Y, CameraPosition.Z)))
                CameraPosition.X += TransformedVector.X;
            if (RequestArea = !IsColliding(new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z + TransformedVector.Z)))
                CameraPosition.Z += TransformedVector.Z;
            if (RequestArea = !IsColliding(new Vector3(CameraPosition.X, CameraPosition.Y + TransformedVector.Y, CameraPosition.Z)))
                CameraPosition.Y += TransformedVector.Y;


            if (RequestArea)
                ChunkManager.GetChunkArea(CameraPosition);

            Ray r = new Ray(CameraPosition, new Vector3(0, 1, 0));
            var UpperCube = ChunkManager.GetFocusedCubeSpecified( ChunkManager.CurrentChunk, float.MaxValue, r, (int) GlobalShares.Identification.Water);
            IsUnderWater = false;
            if (UpperCube.HasValue )
                IsUnderWater = true;

        }

        private static bool IsColliding(Vector3 to)
        {
            if (ChunkManager.CurrentChunk != null)
            {
                foreach (var Surroundings in ChunkManager.CurrentChunk.SurroundingChunks)
                {
                    if (Surroundings != null && Surroundings.IndexRenderer != null)
                        for (int i = 0; i < Surroundings.IndexRenderer.Count; i++)
                        {
                            var Object = Surroundings.ChunkData[Surroundings.IndexRenderer[i]];
                            var BoundingBox = Object.CollisionBox;

                            if (BoundingBox.Contains(to) == ContainmentType.Contains && Surroundings.ChunkData[Surroundings.IndexRenderer[i]].Id != (short)GlobalShares.Identification.Water)
                                return true;
                        }
                }
            }
            return false;
        }

        public static void Update(GameTime gTime)
        {
            CalculateViewMatrix();

            ViewFrustum = new BoundingFrustum(ViewMatrix * ProjectionMatrix);

            Ray = CalculateRay();

            if (PitchYaw != new Vector2(Pitch, Yaw))
                IsChangigView = true;
            else IsChangigView = false;

            PitchYaw = new Vector2(Pitch, Yaw);

            if (Position != CameraPosition)
                isMoving = true;
            else isMoving = false;

            Position = CameraPosition;

            CollisionBox = new BoundingBox(new Vector3(-1 + CameraPosition.X, -1 + CameraPosition.Y, -1 + CameraPosition.Z),
                               new Vector3(1 + CameraPosition.X, 1 + CameraPosition.Y, 1 + CameraPosition.Z));

        }
    }
}
