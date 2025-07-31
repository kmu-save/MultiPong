namespace MPPacket
{
    public enum PacketType
    {
        EnterRequest,    // Player
        EnterResponse,   // Server
        LeaveRequest,    // Player
        LeaveResponse,   // Server

        BallLocationUpdate,    // Server
        PlayerLocationUpdate,  // Server

        BallCollisionRequest,  // Player
        PlayerLocationRequest, // Player
    }

    public class Packet
    {
        public PacketType Type { get; set; }

        public string[] Data { get; set; } 

        public Packet(PacketType type, params string[] data) 
        {
            Type = type;
            Data = data;
        }

        public byte[] Serialize()
        {
            // Convert the packet to a byte array for transmission
            throw new NotImplementedException("Serialization logic needs to be implemented.");
        }

        public static Packet Deserialize(byte[] data)
        {
            // Convert the byte array back to a Packet object
            // This will depend on how you choose to serialize the packet
            throw new NotImplementedException("Deserialization logic needs to be implemented.");
        }
    }
}
