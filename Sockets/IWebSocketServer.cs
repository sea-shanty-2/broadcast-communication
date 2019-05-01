using System.Collections.Generic;
using BroadcastCommunication.Packet;

namespace BroadcastCommunication.Sockets
{
    public interface IWebSocketServer
    {
        void Start();
        void Broadcast(string channel, IPacket packet, ISet<IWebSocketClient> excludedClients);
        bool IsEmojiAllowed(string emoji);
        Polarity GetEmojiPolarity(string emoji);
    }
}