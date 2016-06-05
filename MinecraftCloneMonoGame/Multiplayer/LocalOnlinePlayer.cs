using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using MinecraftClone.CoreII.Global;
using MinecraftClone.Core.Camera;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace MinecraftCloneMonoGame.Multiplayer
{
    public class LocalOnlinePlayer
    {
        //PATTERN:
        // 1) CREATE NETWORKCARD
        // 2) PUSH TO HOST
        // 3) WAIT FOR CALLBACL
        // 4) BEGIN SENDING RECEIVING

        private UdpClient _Client;
        private IPEndPoint IPeP;

        private bool _BindOnCamera;
        private bool _IsConnected;

        public string IPv4 { get; set; }
        public int Port { get; set; }

        public NetworkCard NetworkCard { get; set; }

        public enum Event
        {
            PushPosition,
            PushOthers
        }

        public LocalOnlinePlayer(int port, string server_ip, int server_port)
        {
            IPv4 = Program.GetLocalIPAddress();
            Port = port;

            IPeP = new IPEndPoint(IPAddress.Parse(IPv4), Port);

            NetworkCard = new NetworkCard(IPv4, Port, GlobalShares.GlobalRandom.Next(0, 1000000), "USER" + GlobalShares.GlobalRandom.Next(0, 1000));

            _Client = new UdpClient(IPeP);
            _Client.Connect(new IPEndPoint(IPAddress.Parse(server_ip), server_port));

            LogIn();

            Thread t = new Thread(new ThreadStart(ListenOnSocket));
            t.Start();

        }

        private async void LogIn()
        {
            await _Client.SendAsync(ASCIIEncoding.ASCII.GetBytes("---LOGIN---").ToArray(), 11);
            var _dgrams = NetworkCard.ToBytes();
            await _Client.SendAsync(_dgrams, _dgrams.Length);
 
        }

        private void ListenOnSocket()
        {
            byte[] _Buffer = new byte[64];

            while (true)
            {
                _Client.Client.Receive(_Buffer);
                //byte[] _Datagram = _Client.Receive(ref IPeP);
                string _Message = ASCIIEncoding.ASCII.GetString(_Buffer);
                _Message = _Message.Replace("\0", "");

                if (_Message.Contains("---PERMITTED---"))
                {
                    Console.Beep();
                    _IsConnected = true;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(_Message);
                
            }
        }

        public void BindOnCamera()
        {
            _BindOnCamera = true;
        }

        public void BindOnKeyboard(Input _keyboard)
        {
            _keyboard.BindIPeP_Player(this);
        }



        public async void RaiseEvent(Event _event)
        {
            switch (_event)
            {
                case Event.PushPosition:
                    if (_IsConnected &&_BindOnCamera)
                    {
                        byte[] _dgram = ASCIIEncoding.ASCII.GetBytes(
                           ( NetworkCard.ID + "|" + Camera3D.CameraPosition.X + "|" + Camera3D.CameraPosition.Y + "|" + Camera3D.CameraPosition.Z).ToCharArray());

                        await _Client.SendAsync(_dgram, _dgram.Length);
                    }
                    break;
            }
        }



    }
}
