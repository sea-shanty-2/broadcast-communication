namespace BroadcastCommunication.Packet
{
    public class ChatStatePacket : IPacket
    {
        public string Type => PacketType.ChatState;
        
        public bool Enabled { get; set; }
    }
}