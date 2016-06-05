using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.Multiplayer
{
    public struct NetworkCard
    {
        // IPeP OF CLIENT
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 215)]
        public string Ip;
        public int Port;

        public int ID;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 215)]
        public string Username;

        public NetworkCard(string ip, int port, int id, string user)
            : this()
        {
            Ip = ip;
            Port = port;

            ID = id;
            Username = user;
        }

        // SOURCE : http://stackoverflow.com/a/3278956
        public byte[] ToBytes()
        {
            int size = Marshal.SizeOf(this);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
        // SOURCE : http://stackoverflow.com/a/3278956
        public static NetworkCard FromBytes(byte[] _datagram)
        {
            NetworkCard str = new NetworkCard();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(_datagram, 0, ptr, size);

            str = (NetworkCard)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }

    }
}
