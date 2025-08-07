using MPPacket;
using System.Net;
using System.Net.Sockets;

namespace MultiPong
{
    public class MainForm : Form
    {
        private GameManager _manager;
        private TcpClient _client;
        private int playerNumber;
        private Packet? leavePacket = null;
        private bool alreadySend = false;

        public MainForm()   
        {
            _manager = new GameManager();
            _client = new TcpClient();

            KeyPreview = true;
            KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Down)
                {
                    Packet packet = new Packet(PacketType.PlayerLocationRequest, "down");
                    _client.GetStream().Write(packet.Serialize());
                }
                else if (e.KeyCode == Keys.Up)
                {
                    Packet packet = new Packet(PacketType.PlayerLocationRequest, "up");
                    _client.GetStream().Write(packet.Serialize());
                }
                else if (e.KeyCode == Keys.Space)
                {

                }
            };

            _client.Connect(IPEndPoint.Parse("127.0.0.1:12345"));

            playerNumber = new Random().Next(1000, 10000);

            Packet packet = new Packet(PacketType.EnterRequest, $"Player-{playerNumber}");

            try
            {
                _client.GetStream().Write(packet.Serialize());
            }
            catch (IOException)
            {
                Console.WriteLine("서버와의 연결이 끊겼습니다.");
                if (_client.Connected)
                {
                    _client.Close();
                }
            }

            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            bytesRead =_client.GetStream().Read(buffer, 0, buffer.Length);

            Packet response = Packet.Deserialize(buffer, bytesRead);

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
                
                Thread t1, t2;
                t1 = new Thread(Draw);
                t2 = new Thread(Handler);

                t1.Start();
                t2.Start();
            }
            else
            {
                MessageBox.Show("Failed to enter the game.");
            }
        }

        private void Draw()
        {
            leavePacket = new Packet(PacketType.LeaveRequest, $"Player-{playerNumber}");

            while (true)
            {
                Thread.Sleep(1000 / 60);

                //if (this.IsDisposed || !this.IsHandleCreated) break;

                try
                {
                    this.Invoke(() => _manager.Redraw(this));
                }
                catch (ObjectDisposedException)
                {
                    if (_client.Connected && !alreadySend)
                    {
                        _client.GetStream().Write(leavePacket.Serialize());
                        alreadySend = true;
                    }

                    break;
                }
                catch (InvalidOperationException)
                {
                    if (_client.Connected && !alreadySend)
                    {
                        _client.GetStream().Write(leavePacket.Serialize());
                        alreadySend = true;
                    }

                    break;
                }
            }
        }

        private void Handler()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            bool shouldClose = false;
            leavePacket = new Packet(PacketType.LeaveRequest, $"Player-{playerNumber}");

            while (!shouldClose)
            {
                try
                { 
                    bytesRead = _client.GetStream().Read(buffer, 0, buffer.Length);
                }
                catch (IOException)
                { 
                    if (_client.Connected && !alreadySend)
                    {
                        _client.GetStream().Write(leavePacket.Serialize());
                        alreadySend = true;
                    }
                    // LeaveResponse 패킷을 받기 전에 break되면 안되니까 break문 제거
                    //break;
                }
                catch (ObjectDisposedException)
                {
                    if (_client.Connected && !alreadySend)
                    {
                        _client.GetStream().Write(leavePacket.Serialize());
                        alreadySend = true;
                    }
                    //break;
                }
                

                Packet packet = Packet.Deserialize(buffer, bytesRead);

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

                    case PacketType.LeaveResponse:
                        _client.Close();
                        shouldClose = true;

                        break;
                }
            }
        }
    }
}
