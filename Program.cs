using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        private static List<Player> players = new List<Player>();
        private static int udpPort = 5000;

        static void Main(string[] args)
        {
            Console.WriteLine("Server starting...");
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 4000);
            tcpListener.Start();

            Thread tcpThread = new Thread(() => HandleTCPConnections(tcpListener));
            tcpThread.Start();

            Console.WriteLine("Server running. Waiting for players...");
        }

        private static void HandleTCPConnections(TcpListener tcpListener)
        {
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                Thread playerThread = new Thread(() => HandlePlayer(client));
                playerThread.Start();
            }
        }

        private static void HandlePlayer(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            string playerName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            Player player = new Player { Name = playerName, UDPPort = udpPort++ };
            players.Add(player);

            Console.WriteLine($"Player {player.Name} registered. Assigned UDP port {player.UDPPort}");

            string response = $"Welcome {player.Name}. Your UDP port is {player.UDPPort}";
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);

            client.Close();
        }

        private class Player
        {
            public string Name { get; set; }
            public int UDPPort { get; set; }
        }
    }
}