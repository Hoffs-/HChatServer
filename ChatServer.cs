using System;
using ChatProtos.Networking;
using CoreServer;


namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HServer server = new HServer(4000);
            server.RegisterDefaultCommands();
            server.Run();
        }
    }
}