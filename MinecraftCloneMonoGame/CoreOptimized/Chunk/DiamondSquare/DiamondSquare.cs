//https://github.com/eogas/DiamondSquare/tree/master/DiamondSquare
﻿using Core.MapGenerator;
using Earlz.BareMetal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MinecraftClone.Core;
using MinecraftClone.Core.Camera;
using MinecraftClone.Core.MapGenerator;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using MinecraftClone.Core.Model.Types;
using MinecraftClone.CoreII;
using MinecraftClone.CoreII.Chunk;
using MinecraftClone.CoreII.Profiler;
using MinecraftCloneMonoGame.CoreOptimized.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace MinecraftCloneMonoGame.CoreOptimized.Chunk.DiamondSquare
{
    public class DiamondSquare
    {

        public int Size { get; set; }
        public int Seed { get; set; }

        public float MinTerrainHeight { get; set; }
        public float MaxTerrainHeight { get; set; }

        public float Roughness { get; set; }

        private Random Rand;
        private float[,] Map;

        public DiamondSquare(int size = 32 + 1, int seed = -1, float rMin = 0, float rMax = 0, float roughness = 0.0f)
        {
            Size = size;
            Seed = seed;
            MinTerrainHeight = rMin;
            MaxTerrainHeight = rMax;
            Roughness = roughness;

            Rand = new Random(Seed);
            Map = Calculate();
        }

        private float[,] Calculate()
        {
            int s = Size - 1;
            if (!pow2(s) || MinTerrainHeight >= MaxTerrainHeight)
                return null;

            float modNoise = .0f;

            float[,] _grid = new float[Size, Size];


            _grid[0, 0] = RandRange(Rand, MinTerrainHeight, MaxTerrainHeight);
            _grid[s, 0] = RandRange(Rand, MinTerrainHeight, MaxTerrainHeight);
            _grid[0, s] = RandRange(Rand, MinTerrainHeight, MaxTerrainHeight); ;
            _grid[s, s] = RandRange(Rand, MinTerrainHeight, MaxTerrainHeight);

            float s0, s1, s2, s3, d0, d1, d2, d3, cn;

            for (int i = s; i > 1; i /= 2)
            {
                // reduce the random range at each step
                modNoise = (MaxTerrainHeight - MinTerrainHeight) * Roughness * ((float)i / s);

                // diamonds
                for (int y = 0; y < s; y += i)
                {
                    for (int x = 0; x < s; x += i)
                    {
                        s0 = _grid[x, y];
                        s1 = _grid[x + i, y];
                        s2 = _grid[x, y + i];
                        s3 = _grid[x + i, y + i];

                        // cn
                        _grid[x + (i / 2), y + (i / 2)] = ((s0 + s1 + s2 + s3) / 4.0f)
                            + RandRange(Rand, -modNoise, modNoise);
                    }
                }

                for (int y = 0; y < s; y += i)
                {
                    for (int x = 0; x < s; x += i)
                    {
                        s0 = _grid[x, y];
                        s1 = _grid[x + i, y];
                        s2 = _grid[x, y + i];
                        s3 = _grid[x + i, y + i];
                        cn = _grid[x + (i / 2), y + (i / 2)];

                        d0 = y <= 0 ? (s0 + s1 + cn) / 3.0f : (s0 + s1 + cn + _grid[x + (i / 2), y - (i / 2)]) / 4.0f;
                        d1 = x <= 0 ? (s0 + cn + s2) / 3.0f : (s0 + cn + s2 + _grid[x - (i / 2), y + (i / 2)]) / 4.0f;
                        d2 = x >= s - i ? (s1 + cn + s3) / 3.0f :
                            (s1 + cn + s3 + _grid[x + i + (i / 2), y + (i / 2)]) / 4.0f;
                        d3 = y >= s - i ? (cn + s2 + s3) / 3.0f :
                            (cn + s2 + s3 + _grid[x + (i / 2), y + i + (i / 2)]) / 4.0f;

                        _grid[x + (i / 2), y] = d0 + RandRange(Rand, -modNoise, modNoise);
                        _grid[x, y + (i / 2)] = d1 + RandRange(Rand, -modNoise, modNoise);
                        _grid[x + i, y + (i / 2)] = d2 + RandRange(Rand, -modNoise, modNoise);
                        _grid[x + (i / 2), y + i] = d3 + RandRange(Rand, -modNoise, modNoise);
                    }
                }
            }




            return _grid;
        }
        public float[,] GetTile(int x, int y, int width, int height)
        {
            float[,] copy = new float[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    copy[i, j] = Map[i + x, j + y];
                }
            }
            return copy;
        }

        public static int RandRange(Random r, int rMin, int rMax)
        {
            return rMin + r.Next() * (rMax - rMin);
        }

        public static double RandRange(Random r, double rMin, double rMax)
        {
            return rMin + r.NextDouble() * (rMax - rMin);
        }

        public static float RandRange(Random r, float rMin, float rMax)
        {
            return rMin + (float)r.NextDouble() * (rMax - rMin);
        }

        public static bool pow2(int a)
        {
            return (a & (a - 1)) == 0;
        }

    }
}
