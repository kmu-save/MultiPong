using MPPacket;
using System.Net;
using System.Net.Sockets;

namespace MultiPong
{
    public class MainForm : Form
    {
        private GameManager _manager;
        private TcpClient _client;

        public MainForm()   
        {
            _manager = new GameManager();
            _client = new TcpClient();

            _client.Connect(IPEndPoint.Parse("127.0.0.1:12345"));

            Packet packet = new Packet(PacketType.EnterRequest, $"Player-{new Random().Next(1000, 10000)}");

            _client.GetStream().Write(packet.Serialize());

            byte[] buffer = new byte[1024];
            _client.GetStream().Read(buffer);

            Packet response = Packet.Deserialize(buffer);

            if (response.Type == PacketType.EnterResponse)
            {
                _manager.MapSize = new Size(int.Parse(response.Data[0]), int.Parse(response.Data[1]));
                _manager.StickSize = new Size(int.Parse(response.Data[2]), int.Parse(response.Data[3]));
                _manager.BallSize = new Size(int.Parse(response.Data[4]), int.Parse(response.Data[5]));
                _manager.GoalMargin = int.Parse(response.Data[6]);

                _manager.Player1 = new Point(_manager.GoalMargin, int.Parse(response.Data[7]));
                _manager.Player2 = new Point(_manager.MapSize.Width - _manager.GoalMargin - _manager.StickSize.Width, int.Parse(response.Data[8]));
                _manager.Ball = new Point(int.Parse(response.Data[9]), int.Parse(response.Data[10]));

                _manager.WithPlayer1 = bool.Parse(response.Data[11]);
                _manager.WithPlayer2 = bool.Parse(response.Data[12]);
                _manager.WithBall = bool.Parse(response.Data[13]);

                new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000 / 60);
                        Invoke(() => _manager.Redraw(this));
                    }
                }).Start();

                new Thread(Handler).Start();
            }
            else
            {
                MessageBox.Show("Failed to enter the game.");
            }
        }

        private void Handler()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                _client.GetStream().Read(buffer);

                Packet packet = Packet.Deserialize(buffer);

                switch (packet.Type)
                {
                    case PacketType.Update:
                        _manager.Player1 = new Point(_manager.Player1.X, int.Parse(packet.Data[0]));
                        _manager.Player2 = new Point(_manager.Player2.X, int.Parse(packet.Data[1]));
                        _manager.Ball = new Point(int.Parse(packet.Data[2]), int.Parse(packet.Data[3]));
                        _manager.WithPlayer1 = bool.Parse(packet.Data[4]);
                        _manager.WithPlayer2 = bool.Parse(packet.Data[5]);
                        _manager.WithBall = bool.Parse(packet.Data[6]);

                        break;
                }
            }
        }
    }
}
