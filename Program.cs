using System.Threading;
using WebSocketServer = BroadcastCommunication.Sockets.WebSocketServer;

namespace BroadcastCommunication
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:3333")
            {
                RestartAfterListenError = true
            };
            server.Start();
            
            // Continuously send ratings to gateway
            while (true)
            {
                foreach (var channel in server.Channels)
                {
                    // channel.Id
                    // channel.PositiveRatings
                    // channel.NegativeRatings
                    // Enjoy, Thomas!
                }
                
                Thread.Sleep(10000);
            }
        }
    }
}
