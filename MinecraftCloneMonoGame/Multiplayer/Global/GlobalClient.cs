using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.IO;

namespace MinecraftCloneMonoGame.Multiplayer.Global
{
    public class GlobalClient
    {
        private TcpClient _Client;

        private StreamWriter _Writer;
        private StreamReader _Reader;

        private BinaryWriter _BinWriter;
        private BinaryReader _BinReader;

        private IPEndPoint _RemoteIPeP;

        private GlobalNetworkCard _NetworkCard;

        public enum Option
        {
            NetworkCard,
            Buffer,
            String
        }

        public GlobalClient(TcpClient _client, GlobalNetworkCard _card)
        {
            _Client = _client;
            _NetworkCard = _card;

            _RemoteIPeP = (IPEndPoint)_Client.Client.RemoteEndPoint;

            _Writer = new StreamWriter(_Client.GetStream());
            _Reader = new StreamReader(_client.GetStream());

            _BinWriter = new BinaryWriter(_Client.GetStream());
            _BinReader = new BinaryReader(_Client.GetStream());

        }

        public  void Send(string _dgram, string param = "STRING")
        {
            _BinWriter.Write(param);
            _BinWriter.Write(_dgram);
        }

        public void Send(byte[] _dgram, Option _option)
        {
            switch (_option)
            {
                case Option.NetworkCard:
                    _BinWriter.Write("NETWORK_CARD");
                    _BinWriter.Write(_dgram.Length);
                    _BinWriter.Write(_dgram);
                    break;
                case Option.Buffer:
                    _BinWriter.Write("BUFFER");
                    _BinWriter.Write(_dgram.Length);
                    _BinWriter.Write(_dgram);
                    break;
            }
        }

        public string Receive()
        {
            var _Message = _BinReader.ReadString();
            return _Message; 
        }
    }
}
