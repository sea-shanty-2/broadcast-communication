namespace BroadcastCommunication.Packet
{
    public class IdentityPacket : IPacket
    {
        public string Type => PacketType.Identity;
        public int SequenceId { get; set; }
        public string Name { get; set; }
    }
}