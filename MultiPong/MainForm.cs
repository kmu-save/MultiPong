using MPPacket;
using System.Net;
using System.Net.Sockets;

namespace MultiPong
{
    public class MainForm : Form
    {
        private TcpClient Client { get; set; }

        public MainForm()   
        {
            Client = new TcpClient();

            Client.Connect(IPEndPoint.Parse("127.0.0.1:12345"));

            Packet packet = new Packet(PacketType.EnterRequest, $"Player-{new Random().Next(1000, 10000)}");

            Client.GetStream().Write(packet.Serialize());
        }
    }
}
