using MPPacket;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
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

            ta = new Thread(() => WaitAndHandleClient(server, 1));
            tb = new Thread(() => WaitAndHandleClient(server, 2));

            ta.Start();
            tb.Start();
            Console.WriteLine("Thread Start");

            ta.Join();
            tb.Join();
            Console.WriteLine("Thread End");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static void WaitAndHandleClient(TcpListener server, int pid)
        {
            while (true)
            {
                Console.WriteLine($"[Player{pid}] waiting...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("=================================================");
                Console.WriteLine($"[Player{pid}] connected!");

                try
                {
                    Handle(client, pid);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Player{pid}] error: {e.Message}");
                }
                finally
                {
                    client.Close();
                    Console.WriteLine($"[Player{pid}] disconnected. Restarting slot...");
                }
            }
        }
        public static void Handle(TcpClient client, int pid)
        {
            string name = "Client";
            Console.WriteLine($"{name} connected from {client.Client.RemoteEndPoint}.");

            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while (client.Connected)
            {
                try
                {
                    bytesRead = client.GetStream().Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                }
                catch (IOException e)
                {
                    Console.WriteLine($"[IO Exception] 연결 오류 발생: {e.Message}");
                    break;
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("소켓이 이미 닫힘");
                    break;
                }

                Packet packet = Packet.Deserialize(buffer, bytesRead);

                switch (packet.Type)
                {
                    case PacketType.EnterRequest:
                        name = packet.Data[0];
                        Console.WriteLine($"{name} has entered the game.");

                        if (pid == 1)
                        {
                            lock (_manager)
                            {
                                _manager.EnterPlayer1();
                            }
                        }
                        else if (pid == 2)
                        {
                            lock (_manager)
                            {
                                _manager.EnterPlayer2();
                            }
                        }
                        client.GetStream().Write(new Packet(PacketType.EnterResponse, _manager.SyncInit(pid)).Serialize());

                        break;

                    case PacketType.Update:
                        break;

                    case PacketType.LeaveRequest:
                        name = packet.Data[0];
                        Packet leaveResponse = new Packet(PacketType.LeaveResponse, $"{name}");
                        if (pid == 1)
                        {
                            lock (_manager)
                            {
                                _manager.LeavePlayer1();
                            }
                            //client.Close();
                        }
                        else if (pid == 2)
                        {
                            lock (_manager)
                            {
                                _manager.LeavePlayer2();
                            }
                            //client.Close();
                        }

                        client.GetStream().Write(leaveResponse.Serialize());
                        Console.WriteLine("=================================================");
                        Console.WriteLine($"{name} has left the game.");
                        return;

                    case PacketType.PlayerLocationRequest:
                        string direction = packet.Data[0];
                        int newY = 0;

                        if (direction == "up")
                        {
                            newY = -10;
                        }
                        else if (direction == "down")
                        {
                            newY = 10;
                        }
                        else
                        {
                            Console.WriteLine($"{name} sent an invalid direction: {direction}");
                            continue;
                        }

                        if (pid == 1)
                        {
                            lock (_manager)
                            {
                                _manager.RequestSetPlayer1Location(_manager.Player1.Y + newY);
                            }
                        }
                        else if (pid == 2)
                        {
                            lock (_manager)
                            {
                                _manager.RequestSetPlayer2Location(_manager.Player2.Y + newY);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine($"{name} sent an unknown packet type: {packet.Type}");
                        break;
                }

                new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            client.GetStream().Write(new Packet(PacketType.Update, _manager.SyncUpdate()).Serialize());
                            Debug.WriteLine(JsonSerializer.Serialize(_manager));
                            Thread.Sleep(1000 / 60);
                        }
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Debug.WriteLine("클라이언트 스트림이 dispose됨: " + ex.Message);
                    }
                    catch (IOException ex)
                    {
                        Debug.WriteLine("클라이언트 연결해제 또는 네트워크 오류: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("예상치 못한 오류: " + ex.Message);
                    }
                }).Start();
            }
        }
    }
}