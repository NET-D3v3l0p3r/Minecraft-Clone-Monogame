using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.Multiplayer.Global
{
    public struct GlobalNetworkCard
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 215)]
        public string Username;
        public int ID;

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
        public static GlobalNetworkCard FromBytes(byte[] _datagram)
        {
            GlobalNetworkCard str = new GlobalNetworkCard();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(_datagram, 0, ptr, size);

            str = (GlobalNetworkCard)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }
    }
}
