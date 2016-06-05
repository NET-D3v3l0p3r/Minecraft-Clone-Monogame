using MinecraftCloneMonoGame.Multiplayer;
using MinecraftCloneMonoGame.Multiplayer.Global;
using System;
using System.Net;
using System.Net.Sockets;

namespace MinecraftCloneMonoGame
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("HOST?");

            var _Answer = Console.ReadLine();
            if (_Answer.ToUpper().Equals("YES"))
            {
                Console.WriteLine("GLOBAL/LOCAL?");
                var _Decision = Console.ReadLine().ToUpper();
                if (_Decision == "LOCAL")
                {
                    Console.WriteLine("PORT: ");
                    LocalHost Host = new LocalHost(GetLocalIPAddress(), int.Parse(Console.ReadLine()));
                }
                else if (_Decision == "GLOBAL")
                {
                    Console.WriteLine("PORT: ");
                    GlobalHost _Host = new GlobalHost(GetLocalIPAddress(), int.Parse(Console.ReadLine()));

                    while (true)
                    {
                        _Host.SendToAllClients("[ADMIN]: " + Console.ReadLine());
                    }
                }
            }
            else

                using (var game = new MinecraftClone.MinecraftCloneGame())
                    game.Run();

        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
#endif
}
