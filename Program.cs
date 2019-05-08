using System.Threading;
using WebSocketServer = BroadcastCommunication.Sockets.WebSocketServer;

namespace BroadcastCommunication
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("0.0.0.0:4040")
            {
                RestartAfterListenError = true
            };
            server.Start();
            
            // TODO: Run some background processes/cleaning
            while (true) Thread.Sleep(1000);
        }
    }
}
