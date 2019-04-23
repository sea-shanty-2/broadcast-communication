using System;
using System.Threading;
using Fleck;

namespace BroadcastCommunication
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:5004") { RestartAfterListenError = true };
            server.Start();
            
            // TODO: Run some background processes/cleaning
            Console.ReadLine();
        }
    }
}
