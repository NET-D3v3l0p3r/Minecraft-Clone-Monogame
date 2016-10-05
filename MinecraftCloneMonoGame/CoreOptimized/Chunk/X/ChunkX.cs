using Core.MapGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinecraftClone.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftCloneMonoGame.CoreObsolete.Misc;

namespace MinecraftCloneMonoGame.CoreOptimized.Chunk.X
{
    public class ChunkX
    {
        private Vector3[] Forward(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X, _pos.Y, _pos.Z + 1), new Vector3(_pos.X + 1, _pos.Y, _pos.Z + 1) };
            return _v;
        }
        private Vector3[] Backward(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X, _pos.Y, _pos.Z - 1), new Vector3(_pos.X + 1, _pos.Y, _pos.Z - 1) };
            return _v;
        }
        private Vector3[] Left(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X - 1, _pos.Y, _pos.Z), new Vector3(_pos.X - 1, _pos.Y, _pos.Z + 1) };
            return _v;
        }
        private Vector3[] Right(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X + 1, _pos.Y, _pos.Z), new Vector3(_pos.X - 1, _pos.Y, _pos.Z + 1) };
            return _v;
        }
        private Vector3[] UpX(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X, _pos.Y + 1, _pos.Z), new Vector3(_pos.X + 1, _pos.Y + 1, _pos.Z) };
            return _v;
        }
        private Vector3[] UpZ(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X, _pos.Y + 1, _pos.Z), new Vector3(_pos.X, _pos.Y + 1, _pos.Z + 1) };
            return _v;
        }
        private Vector3[] DownX(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X, _pos.Y - 1, _pos.Z), new Vector3(_pos.X + 1, _pos.Y - 1, _pos.Z) };
            return _v;
        }
        private Vector3[] DownZ(Vector3 _pos)
        {
            Vector3[] _v = new Vector3[2] { new Vector3(_pos.X, _pos.Y - 1, _pos.Z), new Vector3(_pos.X, _pos.Y - 1, _pos.Z + 1) };
            return _v;
        }
    }
}
