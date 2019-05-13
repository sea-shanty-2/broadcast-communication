using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BroadcastCommunication.Packet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fleck;

namespace BroadcastCommunication.Sockets
{
    class WebSocketClient : IWebSocketClient
    {
        public string Name { get; private set; }
        public IChannel Channel { get; private set; }
        public int SequenceId { get; private set; }
        private bool Identified => Name != null && Channel != null;
        public IWebSocketConnection Socket { get; }

        private readonly ConcurrentDictionary<IChannel, DateTime> _lastReaction;
        private readonly ConcurrentDictionary<IChannel, DateTime> _lastChat;
        private readonly IWebSocketServer _server;

        public WebSocketClient(IWebSocketServer webSocketServer, IWebSocketConnection socket)
        {
            Socket = socket;

            _server = webSocketServer;
            _lastReaction = new ConcurrentDictionary<IChannel, DateTime>();
            _lastChat = new ConcurrentDictionary<IChannel, DateTime>();
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
                    case PacketType.ChatState:
                        HandleChatStatePacket(jsonObject);
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

        private void HandleChatStatePacket(JObject jsonObject)
        {
            if (!Identified || Channel.Owner != this || Channel == null) return;
            Channel.ChatEnabled = jsonObject["Enabled"].Value<bool>();
        }

        private void HandleReactionPacket(JObject jsonObject)
        {
            if (!Identified) return;
            _lastReaction.TryGetValue(Channel, out var time);

            var emoji = jsonObject["Reaction"].Value<string>();
            if (!_server.IsEmojiAllowed(emoji) || DateTime.Now - time <= TimeSpan.FromMilliseconds(200)) return;
            Channel?.Broadcast(new ReactionPacket(emoji), new HashSet<IWebSocketClient> { this });
            Channel?.Rate(_server.GetEmojiPolarity(emoji), this);
            _lastReaction[Channel] = DateTime.Now;
        }

        private void HandleChatPacket(JObject jsonObject)
        {
            if (!Identified) return;
            _lastChat.TryGetValue(Channel, out var time);

            if (DateTime.Now - time <= TimeSpan.FromMilliseconds(200)) return;
            Channel?.Broadcast(new MessagePacket(jsonObject["Message"].Value<string>(), this), new HashSet<IWebSocketClient> { this });
            _lastChat[Channel] = DateTime.Now;
        }

        private void HandleIdentityPacket(JObject jsonObject)
        {
            Channel?.RemoveClient(this);

            Name = jsonObject["Name"].Value<string>();
            Channel = _server.GetOrJoinChannel(jsonObject["Channel"].Value<string>(), this);
            SequenceId = Channel.AddClient(this);
            
            // Confirm client's identity
            Socket.Send(JsonConvert.SerializeObject(new IdentityPacket()
            {
                SequenceId = SequenceId,
                Name = Name
            }));
            
            // If channel's chat is disabled, send to this chatter
            if (!Channel.ChatEnabled)
            {
                Socket.Send(JsonConvert.SerializeObject(new ChatStatePacket {Enabled = Channel.ChatEnabled}));
            }
        }
    }
}