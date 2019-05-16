using System.Collections.Generic;
using BroadcastCommunication.Packet;
using BroadcastCommunication.Sockets;

namespace BroadcastCommunication.Channel
{
    public interface IChannel
    {
        string Id { get; }
        int AddClient(IWebSocketClient client);
        void RemoveClient(IWebSocketClient client);
        void Broadcast(IPacket packet, ISet<IWebSocketClient> excludedClients);
        void Rate(string emoji, IWebSocketClient user);
        void SendMessage(string message, IWebSocketClient user);
        void SendRecentMessages(IWebSocketClient user);
        
        bool ChatEnabled { get; set; }
        
        int PositiveRatings { get; }
        int NegativeRatings { get; }
        
        IWebSocketClient Owner { get; }
    }
}