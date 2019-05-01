namespace BroadcastCommunication.Packet
{
    public class ReactionPacket : IPacket
    {
        public string Type => PacketType.Reaction;
        public string Reaction { get; }
        
        public ReactionPacket(string reaction)
        {
            Reaction = reaction;
        }
    }
}