using BroadcastCommunication.Sockets;

namespace BroadcastCommunication.Packet
{
    public class MessagePacket : IPacket
    {
        public string Type => PacketType.Message;
        public string Message { get; }
        public string Author { get; }
        public string Avatar { get; }
        
        public MessagePacket(string message, IWebSocketClient sender)
        {
            Message = message;
            Author = sender.Name;
            Avatar = sender.Avatar;
        }
    }
}