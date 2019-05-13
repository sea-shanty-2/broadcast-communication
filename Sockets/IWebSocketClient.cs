using Fleck;

namespace BroadcastCommunication.Sockets
{
    public interface IWebSocketClient
    {
        string Name { get; }
        IChannel Channel { get; }
        IWebSocketConnection Socket { get; }
        int SequenceId { get; }
        void HandleMessage(string message);
    }
}