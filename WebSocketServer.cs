using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BroadcastCommunication.Packet;
using Fleck;
using J3QQ4;
using Newtonsoft.Json;

namespace BroadcastCommunication
{
    public class WebSocketServer : Fleck.WebSocketServer, IWebSocketServer
    {
        private readonly IDictionary<string, Polarity> _emojiPolarityMap;
        private readonly IDictionary<IWebSocketConnection, WebSocketClient> _clientMap;

        public WebSocketServer(string location, bool supportDualStack = true) : base(location, supportDualStack)
        {
            _clientMap = new ConcurrentDictionary<IWebSocketConnection, WebSocketClient>();
            _emojiPolarityMap = new Dictionary<string, Polarity>
            {
                { Emoji.Fire, Polarity.Positive},
                { Emoji.Joy, Polarity.Positive},
                { Emoji.Thumbsup, Polarity.Positive},
                { Emoji.B, Polarity.Positive},
                { Emoji.Eggplant, Polarity.Positive},
                { Emoji.Angry, Polarity.Negative},
                { Emoji.Thumbsdown, Polarity.Negative},
            };
        }

        public void Start()
        {
            base.Start(socket =>
            {
                socket.OnClose = () => ConnectionClosed(socket);
                socket.OnOpen = () => ConnectionOpened(socket);
            });
        }

        public void Broadcast(string channel, IPacket packet, ISet<IWebSocketClient> excludedClients)
        {
            var serialized = JsonConvert.SerializeObject(packet);
            
            foreach (var (socket, _) in _clientMap.Where(item => !excludedClients.Contains(item.Value)))
                socket.Send(serialized);
        }

        public bool IsEmojiAllowed(string emoji)
        {
            return _emojiPolarityMap.ContainsKey(emoji);
        }

        public Polarity GetEmojiPolarity(string emoji)
        {
            if (!IsEmojiAllowed(emoji))
                throw new EmojiNotAllowedException(emoji);

            return _emojiPolarityMap[emoji];
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