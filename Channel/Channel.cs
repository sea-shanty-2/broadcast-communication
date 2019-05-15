using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using BroadcastCommunication.Packet;
using BroadcastCommunication.Sockets;
using Newtonsoft.Json;

namespace BroadcastCommunication.Channel
{
    public class Channel : IChannel
    {
        private readonly ISet<IWebSocketClient> _clients = new HashSet<IWebSocketClient>();
        private readonly IDictionary<string, int> _sequenceIds = new ConcurrentDictionary<string, int>();
        private readonly IDictionary<string, Polarity> _ratings = new ConcurrentDictionary<string, Polarity>();
        private readonly FixedSizedQueue<MessagePacket> _messages = new FixedSizedQueue<MessagePacket>(5);
        
        public string Id { get; }
        public IWebSocketClient Owner { get; }
        private int Sequence { get; set; }
        public int NegativeRatings => _ratings.Values.Count(v => v.Equals(Polarity.Negative));
        public int PositiveRatings => _ratings.Values.Count(v => v.Equals(Polarity.Positive));
        
        private bool _chatEnabled = true;

        public bool ChatEnabled
        {
            get => _chatEnabled;
            set
            {
                if (value != _chatEnabled)
                {
                    Broadcast(new ChatStatePacket() { Enabled =  value });
                }
                
                _chatEnabled = value;
            }
        }
        
        public Channel(string id, IWebSocketClient owner)
        {
            Id = id;
            Owner = owner;
        }

        
        public void SendMessage(string message, IWebSocketClient user)
        {
            var messagePacket = new MessagePacket(message, user);
            Broadcast(messagePacket, new HashSet<IWebSocketClient> { user });
            _messages.Enqueue(messagePacket);
        }
        
        public void SendRecentMessages(IWebSocketClient user)
        {
            foreach (var message in _messages)
            {
                user.Socket.Send(JsonConvert.SerializeObject(message));
            }
        }
        
        public void Rate(Polarity rating, IWebSocketClient user)
        {
            _ratings[user.UniqueId] = rating;
        }

        public void Broadcast(IPacket packet, ISet<IWebSocketClient> excludedClients)
        {
            var serialized = JsonConvert.SerializeObject(packet);
            
            foreach (var client in _clients.Where(item => !excludedClients.Contains(item)))
                client.Socket.Send(serialized);
        }

        private void Broadcast(IPacket packet)
        {
            Broadcast(packet, ImmutableHashSet<IWebSocketClient>.Empty);
        }

        public void RemoveClient(IWebSocketClient client)
        {
            if (_clients.Contains(client))
            {
                _clients.Remove(client);
            }
        }

        public int AddClient(IWebSocketClient client)
        {
            if (!_clients.Contains(client))
            {
                _clients.Add(client);
            }
            
            if (!_sequenceIds.ContainsKey(client.UniqueId))
            {
                _sequenceIds[client.UniqueId] = Sequence++;
            }

            return _sequenceIds[client.UniqueId];
        }
    }
}
