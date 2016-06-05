using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace MinecraftCloneMonoGame.Multiplayer        
{
    public struct LocalClient
    {
        public IPAddress IPv4 { get; set; }
        public int Port { get; set; }

        public int ID { get; set; }

        public LocalClient(string ip, int port, int id) : this ( )
        {
            IPv4 = IPAddress.Parse(ip);
            Port = port;
            ID = id;
        }

        public async void Send( UdpClient _client, byte[] datagram)
        {
            _client.Connect(new IPEndPoint(IPv4, Port));
            await _client.SendAsync(datagram, datagram.Length);
        }
    }
}
