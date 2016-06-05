using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Threading;

using System.Net;
using System.Net.Sockets;

namespace MinecraftCloneMonoGame.Multiplayer
{
    public class LocalHost
    {
        // IPeP : HOST
        public string IPv4 { get; set; }
        public int Port { get; set; }

        public bool KeepAlive { get; set; } 

        private UdpClient _Server = new UdpClient();
        private List<LocalClient> _Clients = new List<LocalClient>();

        private IPEndPoint _IPeP;

        public LocalHost(string ip, int port)
        {
            IPv4 = ip;
            Port = port;

            KeepAlive = true;

            _IPeP = new IPEndPoint(IPAddress.Parse(IPv4), Port);
            _Server = new UdpClient(_IPeP);


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" SERVER IS RUNNING ON IPEP: " + _IPeP.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;

            Thread t = new Thread(new ThreadStart(ListenOnStream));
            t.Start();
        }



        private void ListenOnStream()
        {
 
            while (KeepAlive)
            {
 
                byte[] _Datagram = _Server.Receive(ref _IPeP);
                string _Message = ASCIIEncoding.ASCII.GetString(_Datagram);

                // PATTERN: 
                // 1) ---LOGIN---
                // 2) RECEIVE NETWORKPLAYER
                // 3) ADD TO CLIENTS
                // 4) SEND RECEIVMENT TO ALL CLIENTS
                // 5) STAY CONNECTED AND RECEIVE

                if (_Message.Equals("---LOGIN---"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("---NEW_LOGIN---");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    // CLEAR DATAGRAM
                    _Datagram = new byte[0];
                    LogIn();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("---DATAGRAM_RECEIVED---");
                    Console.WriteLine("---SIZE: " + _Datagram.Length + "---");
                    SendToAllClients(_Datagram);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

            }
        }

        private void LogIn()
        {
            var _NetworkPlayer = NetworkCard.FromBytes(_Server.Receive(ref _IPeP));
            LocalClient _UDPClient = new LocalClient(_NetworkPlayer.Ip, _NetworkPlayer.Port, _NetworkPlayer.ID);

            _UDPClient.Send( _Server, ASCIIEncoding.ASCII.GetBytes("---PERMITTED---".ToCharArray()));
            SendToAllClients(ASCIIEncoding.ASCII.GetBytes("___NEW_LOGIN___".ToCharArray()));
            SendToAllClients( _NetworkPlayer.ToBytes());

            _Clients.Add(_UDPClient);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("USER: " + _NetworkPlayer.Username + " REGISTERED");
            Console.ForegroundColor = ConsoleColor.Gray;

        }

        public void SendToAllClients(byte[] datagram)
        {
            string _String = ASCIIEncoding.ASCII.GetString(datagram);
            for (int i = 0; i < _Clients.Count; i++)
            {
                // if (_Clients[i].ID != int.Parse(_String.Split('|')[0]))  
                _Clients[i].Send(_Server, datagram);
            }
        }

        
        


    }
}
