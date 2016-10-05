using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks;
using System.Threading;

using System.Net;
using System.Net.Sockets;

using System.IO;
using MinecraftClone.CoreII.Global;
using MinecraftClone.Core.Camera;

namespace MinecraftCloneMonoGame.Multiplayer.Global
{
    public class GlobalOnlinePlayer
    {

        private TcpClient _Client;

        private bool _BindOnCamera;
        private bool _IsConnected;

        private StreamReader _Reader;
        private StreamWriter _Writer;

        private BinaryWriter _BinWriter;
        private BinaryReader _BinReader;

        public IPEndPoint IPeP_Client { get; set; }
        public IPEndPoint IPeP_Server { get; set; }

        public GlobalNetworkCard NetworkCard { get; set; }

        public enum Event
        {
            PushPosition,
            PushOthers
        }


        public GlobalOnlinePlayer(string _wan, int _port)
        {
            IPeP_Client = new IPEndPoint(IPAddress.Parse(Program.GetLocalIPAddress()), 8000);
            IPeP_Server = new IPEndPoint(IPAddress.Parse(_wan), _port);

            _Client = new TcpClient();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("...Connecting");

            var _Async = _Client.BeginConnect(IPeP_Server.Address.MapToIPv4(), IPeP_Server.Port, null, null);
            var _Result = _Async.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(60));

            if (!_Result)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(".:ERROR:.");
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
            _Client.EndConnect(_Async);
            Console.WriteLine("SUCCESS");

            _Reader = new StreamReader(_Client.GetStream());
            _Writer = new StreamWriter(_Client.GetStream());

            _BinWriter = new BinaryWriter(_Client.GetStream());
            _BinReader = new BinaryReader(_Client.GetStream());

            NetworkCard = new GlobalNetworkCard()
            {
                ID = GlobalShares.GlobalRandom.Next(0, 1000000),
                Username = "USER_" + GlobalShares.GlobalRandom.Next(0, 1000)
            };

            __LogIn();
        }

        private void __Listen()
        {
            while (_Client.Connected)
            {
                var _Input = _BinReader.ReadString();
                //EVALUATE INPUT...

                switch (_Input)
                {
                    case "STRING":
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("[" + NetworkCard.ID + "]" + _BinReader.ReadString());
                        break;

                    case "NETWORK_CARD":
                        int _Length = _BinReader.ReadInt32();
                        byte[] _Buffer = _BinReader.ReadBytes(_Length);

                        GlobalNetworkCard _NewPlayer = GlobalNetworkCard.FromBytes(_Buffer);
                        Console.WriteLine(_NewPlayer);

                        break;

                    case "BUFFER":
                        int __Length = _BinReader.ReadInt32();
                        byte[] __Buffer = _BinReader.ReadBytes(__Length);

                        break;

                    case "POSITION":
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("[POSITION]" + _BinReader.ReadString());
                        break;
                }


            }
        }

        private void __LogIn()
        {
            byte _Code = _BinReader.ReadByte();

            if (_Code == 255)
            {
                Console.WriteLine("SERVER_RESPONSES: OK");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("SENDING NETWORKCARD...");

                var _dgram = NetworkCard.ToBytes();

                _BinWriter.Write(_dgram.Length);
                _BinWriter.Write(_dgram);

                _Code = _BinReader.ReadByte();
                if (_Code == 255)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("SERVER_RESPONSES: OK");
                    Console.WriteLine("CLIENT CONNECTED SUCCESFULLY TO SERVER");
                    Thread t = new Thread(new ThreadStart(__Listen));
                    t.Start();
                }
            }
            else if (_Code == 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("MAX_CLIENT_REACHED");
            }
        }

        public bool SendEcho()
        {
            if (_BinWriter == null)
                return false;
            _BinWriter.Write("ECHO");

            return true;
        }


        public void BindOnCamera()
        {
            _BindOnCamera = true;
        }

        public void BindOnKeyboard(Input _keyboard)
        {
            _keyboard.BindIPeP_Player(this);
        }

        public void RaiseEvent(Event _event)
        {
            switch (_event)
            {
                case Event.PushPosition:
                    if (_Client.Connected && _BindOnCamera)
                    {
                        string _Message = "[POS]|" + NetworkCard.ID + "|" + Camera3D.CameraPosition.X + "|" + Camera3D.CameraPosition.Y + "|" + Camera3D.CameraPosition.Z;
                        _BinWriter.Write(_Message);
                    }
                    break;
            }
        }





    }
}
