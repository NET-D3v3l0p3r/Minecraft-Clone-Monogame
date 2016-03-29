using Microsoft.Xna.Framework;
using MinecraftClone.CoreII.Chunk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.CoreOptimized.Global
{
    public struct Profile
    {
        public BoundingBox AABB { get; set; }
        public ChunkOptimized Chunk { get; set; }
        public int Index { get; set; }
        public int Face { get; set; }
        public float? Distance { get; set; }

        public Profile(ChunkOptimized chunk, int index, int face, float? distance)
            : this()
        {
            Chunk = chunk;
            AABB = Chunk.ChunkData[index].BoundingBox;
            Index = index;
            Face = face;
            Distance = distance;
        }
    }
}
