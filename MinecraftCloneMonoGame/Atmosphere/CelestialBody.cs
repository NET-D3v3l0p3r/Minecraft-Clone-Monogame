using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MinecraftCloneMonoGame.Atmosphere
{
    public interface CelestialBody
    {
        GraphicsDevice GraphicsDevice { get; set; }
        ContentManager ContentManager { get; set; }

        Model Model { get; set; }
        Dictionary<string, Texture2D> Textures { get; set; }

        float MetaData { get; set; }
        float Time { get; set; }

        Vector3 Postion { get; set; }


        
    }
}
