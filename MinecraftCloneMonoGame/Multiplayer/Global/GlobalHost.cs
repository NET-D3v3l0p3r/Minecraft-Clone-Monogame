using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Threading;

using System.Net;
using System.Net.Sockets;

using System.IO;
using System.Windows.Forms;

namespace MinecraftCloneMonoGame.Multiplayer.Global
{
    public class GlobalHost
    {
        private TcpListener _Server;
        private static int _MaxClients = 0;

        private List<GlobalClient> _Clients = new List<GlobalClient>();

        public IPEndPoint IPeP { get; private set; }

        public GlobalHost(string _ip, int _globalport)
        {
            IPeP = new IPEndPoint(IPAddress.Parse(_ip), _globalport);
            _Server = new TcpListener(IPeP);

            //TEST NAT 

            Thread t = new Thread(new ThreadStart(__Listen));
            t.Start();

            WebBrowser _WebEngine = new WebBrowser();
            _WebEngine.Navigate("http://icanhazip.com/");

            while (_WebEngine.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }
            string _WAN = _WebEngine.DocumentText.Split(new string[] { "<PRE>" }, StringSplitOptions.None)[1];
            var _CHARS = _WAN.ToCharArray();
            _WAN = "";
            for (int i = 0; i < _CHARS.Length; i++)
            {
                if (Char.IsDigit(_CHARS[i]) || _CHARS[i] == '.')
                    _WAN += _CHARS[i];
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("TESTING HOST");
            GlobalOnlinePlayer _TestPlayer = new GlobalOnlinePlayer(_WAN, _globalport);
            
            if (!_TestPlayer.SendEcho())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("YOU ARE NOT PERMITTED TO OFFER HOSTING");
                t.Abort();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("YOU ARE THE HOST");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WAN: " + _WAN);
            Console.WriteLine("PORT: " + _globalport);            
        }

        private void __Listen()
        {
            _Server.Start();
            int _ThreadCount = 0; 
            while (true)
            {
                var _Client = _Server.AcceptTcpClient();

                _ThreadCount++;
                
                if (_ThreadCount > _MaxClients)
                    _Client.GetStream().WriteByte(5);
                else
                {
                    // SEND CODE 255 : SUCCESS
                    _Client.GetStream().WriteByte(255);

                    BinaryReader _BinReader = new BinaryReader(_Client.GetStream());

                    var _dgramsize = _BinReader.ReadInt32();
                    var _dgram = _BinReader.ReadBytes(_dgramsize);

                    GlobalNetworkCard _Card = GlobalNetworkCard.FromBytes(_dgram);
                    GlobalClient _GClient = new GlobalClient(_Client, _Card);

                    _Client.GetStream().WriteByte(255);

                    SendToAllClients("---NEW_LOGIN---");
                    SendToAllClients(_Card.ToBytes(), GlobalClient.Option.NetworkCard);

                    _Clients.Add(_GClient);

                    Thread t = new Thread(new ParameterizedThreadStart((object _client) =>
                    {
                        Console.WriteLine("_THREAD:" + _ThreadCount);
                        GlobalClient __Client = (GlobalClient)_client;
                        while (true)
                        {
                            SendToAllClients(__Client.Receive());
                        }
                    }));
                    t.Start(_GClient);
                }
            }
        }

        public void SendToAllClients(string _message)
        {
            //TODO: CLIENTS SENDS PARAMATER
            foreach (var _c in _Clients)
                if (_message.Contains("POS"))
                    _c.Send(_message, "POSITION");
                else _c.Send(_message);
        }

        public void SendToAllClients(byte[] _dgram, GlobalClient.Option _option)
        {
            foreach (var _c in _Clients)
                _c.Send(_dgram, _option);
        }

        public static void _SetMaxClients(int _size)
        {
            _MaxClients = _size;
        }
    }
}
