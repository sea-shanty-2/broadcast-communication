using BroadcastCommunication.Packet;

namespace BroadcastCommunication
{
    public interface IWebSocketServer
    {
        void Start();
        void Broadcast(string channel, IPacket packet);
    }
}