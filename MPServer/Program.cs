using MPPacket;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace MPServer
{
    internal class Program
    {
        private static GameManager _manager = new GameManager();

        public static void Main()
        {
            TcpListener server = new TcpListener(IPEndPoint.Parse("127.0.0.1:12345"));
            server.Start();

            Console.WriteLine("Server started. Waiting for clients to connect...");

            Thread ta, tb;

            ta = new Thread(() => Handler(server, 1));
            tb = new Thread(() => Handler(server, 2));

            ta.Start();
            tb.Start();

            ta.Join();
            tb.Join();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static void Handler(TcpListener server, int p)
        {
            string name = "Client";

            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine($"{name} connected from {client.Client.RemoteEndPoint}.");

            byte[] buffer = new byte[1024];

            while (true)
            {
                client.GetStream().Read(buffer);

                Packet packet = Packet.Deserialize(buffer);

                switch (packet.Type)
                {
                    case PacketType.EnterRequest:
                        name = packet.Data[0];
                        Console.WriteLine($"{name} has entered the game.");

                        if (p == 1)
                        {
                            lock (_manager)
                            {
                                _manager.EnterPlayer1();
                            }
                            client.GetStream().Write(new Packet(PacketType.EnterResponse, _manager.SyncInit()).Serialize());
                        }
                        else if (p == 2)
                        {
                            lock (_manager)
                            {
                                _manager.EnterPlayer2();
                            }
                            client.GetStream().Write(new Packet(PacketType.EnterResponse, _manager.SyncInit()).Serialize());
                        }

                        new Thread(() =>
                        {
                            while (true)
                            {
                                client.GetStream().Write(new Packet(PacketType.Update, _manager.SyncUpdate()).Serialize());
                                Debug.WriteLine(JsonSerializer.Serialize(_manager));
                                Thread.Sleep(1000 / 60);
                            }

                        }).Start();

                        break;
                    default:
                        Console.WriteLine($"{name} sent an unknown packet type: {packet.Type}");
                        break;
                }
            }
        }
    }
}