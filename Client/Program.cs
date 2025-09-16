using System;
using PingPongTurnir;
using PingPongTurnir.Client;

namespace PingPongTurnir.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Ping Pong Klijent";

            var client = new PingPongClient();
            client.Run();

            Console.WriteLine("\nIgra zavrsena. Pritisni Enter za izlazak!");
            Console.ReadLine();
        }
    }
}
