using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BroadcastCommunication.Packet;
using Fleck;
using Newtonsoft.Json;

namespace BroadcastCommunication
{
    public class WebSocketServer : Fleck.WebSocketServer, IWebSocketServer
    {
        private readonly IDictionary<IWebSocketConnection, WebSocketClient> _clientMap;
        
        public WebSocketServer(string location, bool supportDualStack = true) : base(location, supportDualStack)
        {
            _clientMap = new ConcurrentDictionary<IWebSocketConnection, WebSocketClient>();
        }

        public void Start()
        {
            base.Start(socket =>
            {
                socket.OnClose = () => ConnectionClosed(socket);
                socket.OnOpen = () => ConnectionOpened(socket);
            });
        }

        public void Broadcast(string channel, IPacket packet)
        {
            var serialized = JsonConvert.SerializeObject(packet);
            
            foreach (var socket in _clientMap.Keys)
                socket.Send(serialized);
        }

        private void ConnectionOpened(IWebSocketConnection socket)
        {
            var client = new WebSocketClient(this);
            socket.OnMessage = client.HandleMessage;
            _clientMap[socket] = client;
            
            Console.WriteLine("Connection opened");
        }

        private void ConnectionClosed(IWebSocketConnection socket)
        {
            if (_clientMap.ContainsKey(socket))
                _clientMap.Remove(socket);
            
            Console.WriteLine("Connection closed");
        }
    }
}