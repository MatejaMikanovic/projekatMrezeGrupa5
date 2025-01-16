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
    class Program
    {
        // Lista za čuvanje podataka o igračima
        static List<string> players = new List<string>();

        static void Main(string[] args)
        {
            // Zadatak 2.1) Pokretanje TCP osluškivača za prijavu igrača
            Thread serverThread = new Thread(StartServer);
            serverThread.Start();

            // Testiranje sa dva klijenta (simulacija)
            Thread player1Thread = new Thread(() => StartClient("Player 1", "John Doe"));
            player1Thread.Start();
            Thread player2Thread = new Thread(() => StartClient("Player 2", "Jane Smith"));
            player2Thread.Start();
        }

        static void StartServer()
        {
            // Zadatak 2.1) Pokretanje TCP osluškivača za prijavu igrača
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server started...");

            // Zadatak 2.2) Prijem podataka za dva igrača
            for (int i = 0; i < 2; i++)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Player connected.");
                NetworkStream stream = client.GetStream();

                // Zadatak 2.2) Prijem osnovnih podataka igrača
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string playerInfo = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                players.Add(playerInfo);
                Console.WriteLine($"Received player info: {playerInfo}");

                // Zadatak 2.3) Slanje potvrde o uspešnoj prijavi
                string response = "Registration successful";
                buffer = Encoding.UTF8.GetBytes(response);
                stream.Write(buffer, 0, buffer.Length);
                Console.WriteLine("Confirmation sent.");
            }

            server.Stop();
            Console.WriteLine("Server stopped.");
        }

        static void StartClient(string playerName, string fullName)
        {
            // Povezivanje na server putem TCP-a
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            // Slanje osnovnih podataka igrača
            byte[] buffer = Encoding.UTF8.GetBytes(fullName);
            stream.Write(buffer, 0, buffer.Length);

            // Primanje potvrde o uspešnoj prijavi
            buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"{playerName} received: {response}");

            client.Close();
        }
    }
}
