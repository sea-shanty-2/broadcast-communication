using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BroadcastCommunication.Packet;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BroadcastCommunication
{
    class WebSocketClient : IWebSocketClient
    {
        public string Name { get; set; }
        public string Channel { get; set; }
        public string Avatar { get; set; }
        
        private readonly ConcurrentDictionary<string, DateTime> _lastReaction;
        private readonly ConcurrentDictionary<string, DateTime> _lastChat;
        private readonly ConcurrentDictionary<string, Polarity> _channelPolarity;
        private readonly IWebSocketServer _server;

        public WebSocketClient(IWebSocketServer webSocketServer)
        {
            _server = webSocketServer;
            
            _lastReaction = new ConcurrentDictionary<string, DateTime>();
            _lastChat = new ConcurrentDictionary<string, DateTime>();
            _channelPolarity = new ConcurrentDictionary<string, Polarity>();
        }

        public void HandleMessage(string message)
        {
            try
            {
                var jsonObject = JObject.Parse(message);

                if (!jsonObject.ContainsKey("Type")) return;
                
                var packetType = jsonObject["Type"].Value<string>();
                switch (packetType)
                {
                    case PacketType.Identity:
                        HandleIdentityPacket(jsonObject);
                        return;
                    case PacketType.Reaction:
                        HandleReactionPacket(jsonObject);
                        return;
                    case PacketType.Message:
                        HandleChatPacket(jsonObject);
                        return;
                    default:
                        Console.WriteLine($"Unknown packet type {packetType}");
                        return;
                }
            }
            catch (JsonReaderException exception)
            {
                Console.WriteLine($"Unable to parse JSON ({exception.Message})");
            }
        }

        private void HandleReactionPacket(JObject jsonObject)
        {
            _lastReaction.TryGetValue(Channel, out var time);

            var emoji = jsonObject["Reaction"].Value<string>();
            if (!_server.IsEmojiAllowed(emoji) || DateTime.Now - time <= TimeSpan.FromMilliseconds(200)) return;
            _server.Broadcast(Channel, new ReactionPacket(emoji), new HashSet<IWebSocketClient> { this });
            _channelPolarity[Channel] = _server.GetEmojiPolarity(emoji);
            _lastReaction[Channel] = DateTime.Now;
        }

        private void HandleChatPacket(JObject jsonObject)
        {
            _lastChat.TryGetValue(Channel, out var time);

            if (DateTime.Now - time <= TimeSpan.FromMilliseconds(200)) return;
            _server.Broadcast(Channel, new MessagePacket(jsonObject["Message"].Value<string>(), this),
                new HashSet<IWebSocketClient> { this });
            _lastChat[Channel] = DateTime.Now;
        }

        private void HandleIdentityPacket(JObject jsonObject)
        {
            Name = jsonObject["Name"].Value<string>();
            Avatar = jsonObject["Avatar"].Value<string>();
            Channel = jsonObject["Channel"].Value<string>();
        }
    }
}