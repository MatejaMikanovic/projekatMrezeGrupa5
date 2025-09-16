using System;
using PingPongTurnir.Server.Services;
using PingPongTurnir.Shared;

namespace PingPongTurnir.Server
{
    public class PingPongServer
    {
        private readonly NetworkManager _networkManager;
        private readonly GameEngine _gameEngine;
        private readonly LeaderboardManager _leaderboardManager;
        private readonly TournamentManager _tournamentManager;

        public PingPongServer()
        {
            _networkManager = new NetworkManager();
            _gameEngine = new GameEngine(_networkManager);
            _leaderboardManager = new LeaderboardManager();
            _tournamentManager = new TournamentManager(_networkManager, _gameEngine, _leaderboardManager);
        }

        public void Run()
        {
            Console.WriteLine("=== Pokretanje Ping Pong Servera ===");

            Console.Write("Unesi broj igrača: ");
            if (!int.TryParse(Console.ReadLine(), out int numberOfPlayers) || numberOfPlayers < 2)
            {
                Console.WriteLine("Greska: Potrebno je barem 2 igraca!");
                return;
            }

            Console.Write("Unesi broj poena za pobedu: ");
            if (!int.TryParse(Console.ReadLine(), out int pointsToWin) || pointsToWin <= 0)
            {
                Console.WriteLine("Greska: Broj poena mora biti pozitivna vrednost!");
                return;
            }

            try
            {
                _networkManager.StartTcpServer(numberOfPlayers);
                _tournamentManager.StartTournament(numberOfPlayers, pointsToWin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska: Server:  {ex.Message}");
                Console.WriteLine("Zavrsavamo turnir.");
            }

            Console.WriteLine("Turnir je gotov. Pritisnite Enter za izlazak!");
            Console.ReadLine();

            _networkManager.Stop();
            Console.WriteLine("\nServer je zaustavljen.");
        }
    }
}
