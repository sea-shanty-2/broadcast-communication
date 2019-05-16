using BroadcastCommunication.Channel;
using Fleck;

namespace BroadcastCommunication.Sockets
{
    public interface IWebSocketClient
    {
        string Name { get; }
        string UniqueId { get; }
        IChannel Channel { get; }
        IWebSocketConnection Socket { get; }
        int SequenceId { get; }
        void HandleClose();
    }
}