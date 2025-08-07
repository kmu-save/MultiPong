using System.Text;
using System.Text.Json;

namespace MPPacket
{
    public enum PacketType
    {
        EnterRequest,    // Player
        EnterResponse,   // Server

        LeaveRequest,    // Player
        LeaveResponse,   // Server

        Update, // Server

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
            string json = JsonSerializer.Serialize(this);
            byte[] data = Encoding.UTF8.GetBytes(json);

            byte[] packet = new byte[1024];

            for (int i = 0; i < data.Length && i < packet.Length; i++)
            {
                packet[i] = data[i];
            }

            return packet;
        }

        public static Packet Deserialize(byte[] data, int length)
        {
            string json = Encoding.UTF8.GetString(data, 0, length).TrimEnd('\0');
            Packet packet = JsonSerializer.Deserialize<Packet>(json)
                ?? throw new InvalidOperationException("Deserialization failed. Packet is null.");

            return packet;
        }
    }
}
