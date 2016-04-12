using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.CoreObsolete.Misc
{
    public class uInt3
    {
        int x, y, z;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int Z
        {
            get { return z; }
            set { z = value; }
        }

        public uInt3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

    }
}
