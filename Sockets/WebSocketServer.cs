using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BroadcastCommunication.Channel;
using BroadcastCommunication.Packet;
using Fleck;
using J3QQ4;
using Newtonsoft.Json;

namespace BroadcastCommunication.Sockets
{
    public class WebSocketServer : Fleck.WebSocketServer, IWebSocketServer
    {
        private readonly IDictionary<string, Polarity> _emojiPolarityMap;
        private readonly IDictionary<string, IChannel> _channels = new ConcurrentDictionary<string, IChannel>();

        public ICollection<IChannel> Channels => _channels.Values;
        
        public WebSocketServer(string location, bool supportDualStack = true) : base(location, supportDualStack)
        {
            _emojiPolarityMap = new Dictionary<string, Polarity>
            {
                { Emoji.Fire, Polarity.Positive},
                { Emoji.Joy, Polarity.Positive},
                { Emoji.Thumbsup, Polarity.Positive},
                { Emoji.Heart, Polarity.Positive},
                { Emoji.Eggplant, Polarity.Positive},
                { Emoji.Angry, Polarity.Negative},
                { Emoji.Thumbsdown, Polarity.Negative},
            };
        }


        public IChannel GetOrJoinChannel(string channelId, IWebSocketClient requester)
        {
            if (!_channels.ContainsKey(channelId))
            {
                _channels[channelId] = new Channel.Channel(channelId, requester);
            }

            return _channels[channelId];
        }

        public void Start()
        {
            base.Start(socket =>
            {
                socket.OnClose = () => ConnectionClosed(socket);
                socket.OnOpen = () => ConnectionOpened(socket);
            });
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
            var client = new WebSocketClient(this, socket);
            socket.OnMessage = client.HandleMessage;
        }

        private void ConnectionClosed(IWebSocketConnection socket)
        {
            // TODO: Delete from channel?
        }
    }
}