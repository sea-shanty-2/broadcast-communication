using System.Collections.Generic;
using BroadcastCommunication.Channel;
using BroadcastCommunication.Packet;

namespace BroadcastCommunication.Sockets
{
    public interface IWebSocketServer
    {
        void Start();
        bool IsEmojiAllowed(string emoji);
        Polarity GetEmojiPolarity(string emoji);
        IChannel GetOrJoinChannel(string channelId, IWebSocketClient requester);
        ICollection<IChannel> Channels { get; }
    }
}