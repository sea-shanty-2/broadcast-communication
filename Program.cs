using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Fleck;
using WebSocketServer = BroadcastCommunication.Sockets.WebSocketServer;

namespace BroadcastCommunication
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("wss://0.0.0.0:4040")
            {
                RestartAfterListenError = true,
                Certificate = new X509Certificate2("/certs/wss.pfx")
            };
            server.Start();
            
            // TODO: Run some background processes/cleaning
            while (true) Thread.Sleep(1000);
        }
    }
}
