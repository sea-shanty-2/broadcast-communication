using Fleck;

namespace BroadcastCommunication
{
    class WebSocketClient : IWebSocketClient
    {
        public string Name { get; }
        public string Channel { get; }
        public string Avatar { get; }
        
        private readonly IWebSocketServer _server;

        public WebSocketClient(IWebSocketServer webSocketServer)
        {
            _server = webSocketServer;
        }

        public void HandleMessage(string message)
        {
            // this._server.Broadcast("whatever", message);
        }
    }
}