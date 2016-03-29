using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.MapGenerator
{
    public static class GlobalShares
    {
        public const int GoldOre = 0;
        public const int Dirt = 2;
        public const int Stone = 4;
        public const int Water = 6;
        public const int Wood = 8;
        public const int Cobble = 10;
        public const int CoalOre = 12;
        public const int Grass = 14;
        public const int Sand = 16;

        //public static ICube GetNearest(ICube[] cubes, Picking picking)
        //{
        //    for (int i = 0; i < cubes.Length; i++)
        //    {
        //        if (cubes[i] != null)
        //        {
        //            if (cubes[i].BoundingBoxTransformed.Contains(picking.BoundingBox) == ContainmentType.Intersects)
        //                return cubes[i];
        //        }
        //    }

        //    return null;
        //}

        public static string GetRandomWord(int length)
        {
            string[] Letters = new string[] { "a", "e", "i", "o", "u", "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string Result = "";
            for (int i = 0; i < length; i++)
            {
                Result += Letters[MinecraftClone.CoreII.Global.GlobalShares.GlobalRandom.Next(0, Letters.Length)];
            }
            return Result;

        }

    }
}
