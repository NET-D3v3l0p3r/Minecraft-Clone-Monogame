using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MinecraftClone.Core.Camera.Key
{
    public class KeyData
    {
        public Keys LocalKey { get; set; }

        public Action KeyUp { get; set; }
        public Action KeyDown { get; set; }

        public bool EnableLock { get; set; }
        public bool IsLocking { get; set; }

        public KeyData(Keys key, Action up, Action down, bool enable_lock)
        {
            LocalKey = key;

            KeyUp = up;
            KeyDown = down;

            EnableLock = enable_lock;
        }

        public override string ToString()
        {
            return Enum.GetName(LocalKey.GetType(), LocalKey);
        }
    }
}
