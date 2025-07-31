using MPPacket;
using System;
using System.Net;
using System.Net.Sockets;

namespace MPServer
{
    internal class Program
    {
        public static void Main()
        {
            TcpListener server = new TcpListener(IPEndPoint.Parse("127.0.0.1:12345"));
            server.Start();

            Console.WriteLine("Server started. Waiting for clients to connect...");

            Thread ta, tb;

            ta = new Thread(() => Handler(server));
            tb = new Thread(() => Handler(server));

            ta.Start();
            tb.Start();

            ta.Join();
            tb.Join();
        }

        public static void Handler(TcpListener server)
        {
            string name = "TEMP";

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

                        break;
                    default:
                        Console.WriteLine($"{name} sent an unknown packet type: {packet.Type}");
                        break;
                }
            }
        }
    }
}