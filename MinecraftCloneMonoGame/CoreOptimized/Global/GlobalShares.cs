using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
namespace MinecraftClone.CoreII.Global
{
    public static class GlobalShares
    {
        public static ContentManager GlobalContent { get; set; }
        public static GraphicsDevice GlobalDevice { get; set; }
        public static GraphicsDeviceManager GlobalDeviceManager { get; set; }

        public static Random GlobalRandom = new Random();

        public enum Identification
        {
            DefautCubeStructure = 0,
            Dirt = 1,
            Grass = 7,  
            Cobble = 2,
            Stone = 3,
            CoalOre = 4,
            GoldOre = 5,
            IronOre = 6,
            Water = 8
        };
        //215
    }
}
