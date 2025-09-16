using System;
using PingPongTurnir.Server.Services;
using PingPongTurnir.Shared;

namespace PingPongTurnir.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var server = new PingPongServer();
            server.Run();

            Console.WriteLine("Pritisni Enter za izlazak iz servera!");
            Console.ReadLine();
        }
        
    }
}