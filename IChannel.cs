using BroadcastCommunication.Sockets;
using BroadcastCommunication.Packet;
using System.Collections.Generic;

namespace BroadcastCommunication
{
    public interface IChannel
    {
        string Id { get; }
        int AddClient(IWebSocketClient client);
        void RemoveClient(IWebSocketClient client);
        void Broadcast(IPacket packet, ISet<IWebSocketClient> excludedClients);
        void Rate(Polarity rating, IWebSocketClient rater);

        bool ChatEnabled { get; set; }
        
        int PositiveRatings { get; }
        int NegativeRatings { get; }
        
        IWebSocketClient Owner { get; }
    }
}