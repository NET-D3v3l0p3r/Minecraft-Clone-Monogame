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
        public const short GoldOre = 0;
        public const short Dirt = 2;
        public const short Stone = 4;
        public const short Water = 6;
        public const short Wood = 8;
        public const short Cobble = 10;
        public const short CoalOre = 12;
        public const short Grass = 14;
        public const short Sand = 16;

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
