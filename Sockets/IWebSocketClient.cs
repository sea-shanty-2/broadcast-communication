namespace BroadcastCommunication.Sockets
{
    public interface IWebSocketClient
    {
        string Name { get; }
        string Channel { get; }
        string Avatar { get; }
        void HandleMessage(string message);
    }
}