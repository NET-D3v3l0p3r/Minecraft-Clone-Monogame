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
using MinecraftCloneMonoGame.Multiplayer;
using MinecraftCloneMonoGame.Multiplayer.Global;
namespace MinecraftClone.Core.Camera
{
    public class Input
    {
        private LocalOnlinePlayer _IPeP_Local;
        private GlobalOnlinePlayer _IPeP_Global;

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
                    {
                        keys.KeyDown.Invoke();
                        if (_IPeP_Local != null)
                            _IPeP_Local.RaiseEvent(LocalOnlinePlayer.Event.PushPosition);

                        if (_IPeP_Global != null)
                            _IPeP_Global.RaiseEvent(GlobalOnlinePlayer.Event.PushPosition);

                    }
                    else if (Keyboard.GetState().IsKeyUp(keys.LocalKey))
                        keys.KeyUp.Invoke();
                    
                }
            }
        }

        public void BindIPeP_Player(LocalOnlinePlayer _player)
        {
            _IPeP_Local = _player;
        }

        public void BindIPeP_Player(GlobalOnlinePlayer _player)
        {
            _IPeP_Global = _player;
        }

    }
}
