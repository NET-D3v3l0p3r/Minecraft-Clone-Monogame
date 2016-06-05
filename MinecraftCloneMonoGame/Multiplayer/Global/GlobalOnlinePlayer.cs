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

namespace MinecraftCloneMonoGame.Multiplayer.Global
{
    public class GlobalOnlinePlayer
    {

        private TcpClient _Client;
        
        private StreamReader _Reader;
        private StreamWriter _Writer;

        private BinaryWriter _BinWriter;
        private BinaryReader _BinReader;

        public IPEndPoint IPeP_Client { get; set; }
        public IPEndPoint IPeP_Server { get; set; }

        public GlobalNetworkCard NetworkCard { get; set; }


        public GlobalOnlinePlayer(string _wan, int _port)
        {
            IPeP_Client = new IPEndPoint(IPAddress.Parse(Program.GetLocalIPAddress()), 8000);
            IPeP_Server = new IPEndPoint(IPAddress.Parse(_wan), _port);

            _Client = new TcpClient();
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("...Connecting");
                _Client.Connect(IPeP_Server);
            }
            catch { }
            if(! _Client.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(".:ERROR:.");
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
            Console.WriteLine("SUCCESS");

            _Reader = new StreamReader(_Client.GetStream());
            _Writer = new StreamWriter(_Client.GetStream());

            _BinWriter = new BinaryWriter(_Client.GetStream());
            _BinReader = new  BinaryReader(_Client.GetStream());

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
                        Console.WriteLine("["+ NetworkCard.ID + "]" +  _BinReader.ReadString());
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
        }

        public bool SendEcho()
        {
            if (_BinWriter == null)
                return false;
            _BinWriter.Write("ECHO");
            _BinWriter.Flush();

            return _BinReader.ReadByte() == 255;
        }





    }
}
