using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MinecraftClone.Core.Misc;
using MinecraftClone.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinecraftClone.Core.Camera.Key;
namespace MinecraftClone.Core.Camera
{
    public class Input
    {
        public List<KeyData> KeyList { get; set; }

        public Input()
        {
            KeyList = new List<KeyData>();
        }

        public void Update(GameTime gTime)
        {
            foreach (var keys in KeyList)
            {
                if (keys.EnableLock)
                {
                    if ( Keyboard.GetState().IsKeyDown(keys.LocalKey) && !keys.IsLocking)
                    {
                        keys.KeyDown.Invoke();
                        keys.IsLocking = true;
                    }
                    else if (Keyboard.GetState().IsKeyUp(keys.LocalKey) && keys.IsLocking)
                    {
                        keys.KeyUp.Invoke();
                        keys.IsLocking = false;
                    }
                }
                else
                {
                    if (Keyboard.GetState().IsKeyDown(keys.LocalKey))
                        keys.KeyDown.Invoke();
                    else if (Keyboard.GetState().IsKeyUp(keys.LocalKey))
                        keys.KeyUp.Invoke();
                    
                }
            }
        }

    }
}
