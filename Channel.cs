using System.Collections.Concurrent;
using BroadcastCommunication.Sockets;
using BroadcastCommunication.Packet;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace BroadcastCommunication
{
    public class Channel : IChannel
    {
        private readonly ISet<IWebSocketClient> _clients = new HashSet<IWebSocketClient>();
        private readonly IDictionary<IWebSocketClient, int> _sequenceIds = new ConcurrentDictionary<IWebSocketClient, int>();
        private readonly IDictionary<IWebSocketClient, Polarity> _ratings = new ConcurrentDictionary<IWebSocketClient, Polarity>();
        
        public string Id { get; }
        public IWebSocketClient Owner { get; }
        private int Sequence { get; set; }
        public int NegativeRatings => _ratings.Values.Count(v => v.Equals(Polarity.Negative));
        public int PositiveRatings => _ratings.Values.Count(v => v.Equals(Polarity.Positive));
        
        public Channel(string id, IWebSocketClient owner)
        {
            Id = id;
            Owner = owner;
        }
        
        public void Rate(Polarity rating, IWebSocketClient rater)
        {
            _ratings[rater] = rating;
        }

        public void Broadcast(IPacket packet, ISet<IWebSocketClient> excludedClients)
        {
            var serialized = JsonConvert.SerializeObject(packet);
            
            foreach (var client in _clients.Where(item => !excludedClients.Contains(item)))
                client.Socket.Send(serialized);
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
            
            if (!_sequenceIds.ContainsKey(client))
            {
                _sequenceIds[client] = this.Sequence++; // Owner is assigned id = 0
            }

            return _sequenceIds[client];
        }
    }
}
