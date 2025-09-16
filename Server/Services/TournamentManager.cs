using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PingPongTurnir.Server.Models;
using PingPongTurnir.Shared;

namespace PingPongTurnir.Server.Services
{
    public class TournamentManager
    {
        private readonly NetworkManager _networkManager;
        private readonly GameEngine _gameEngine;
        private readonly LeaderboardManager _leaderboardManager;

        private int _udpPortCounter = Constants.INITIAL_UDP_PORT;
        private int _totalGames = 0;

        public TournamentManager(NetworkManager networkManager, GameEngine gameEngine, LeaderboardManager leaderboardManager)
        {
            _networkManager = networkManager;
            _gameEngine = gameEngine;
            _leaderboardManager = leaderboardManager;
        }

        public async Task StartTournament(int numberOfPlayers, int pointsToWin)
        {
            var players = await WaitForPlayers(numberOfPlayers);
            Console.WriteLine("\nSvi igraci povezani! Pokrecemo turnir...");

            await RunEliminationRounds(players, pointsToWin);

            _leaderboardManager.DisplayLeaderboard(players);
            _leaderboardManager.ShowTournamentStats(players, _totalGames);

            string finalLeaderboard = _leaderboardManager.CreateLeaderboardMessage(players);
            _networkManager.SendTcpMessageToAll(players, finalLeaderboard);
            _networkManager.SendTcpMessageToAll(players, Constants.MSG_TOURNAMENT_END);
        }

        private async Task<List<Player>> WaitForPlayers(int numberOfPlayers)
        {
            var players = new List<Player>();
            Console.WriteLine($"\nCekamo {numberOfPlayers} igraca...");

            while (players.Count < numberOfPlayers)
            {
                var socket = _networkManager.AcceptClientWithTimeout(1000);
                if (socket != null)
                {
                    try
                    {
                        string name = _networkManager.ReceivePlayerName(socket);
                        var player = new Player(name, socket);
                        players.Add(player);
                        Console.WriteLine($"Povezan: {name} ({players.Count}/{numberOfPlayers})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greska pri dodavanju igraca: {ex.Message}");
                        socket.Close();
                    }
                }
                else
                {
                    Console.Write(".");
                }
            }

            return players;
        }

        private async Task RunEliminationRounds(List<Player> allPlayers, int pointsToWin)
        {
            var currentPlayers = new List<Player>(allPlayers);
            int roundNumber = 1;

            while (currentPlayers.Count > 1)
            {
                Console.WriteLine($"\n=== Runda {roundNumber} === ({currentPlayers.Count} igraca)");

                var nextRoundPlayers = new List<Player>();
                ShufflePlayers(currentPlayers);
                ResetUdpPorts();

                for (int i = 0; i < currentPlayers.Count - 1; i += 2)
                {
                    var player1 = currentPlayers[i];
                    var player2 = currentPlayers[i + 1];

                    if (!IsPlayerConnected(player1) || !IsPlayerConnected(player2))
                    {
                        var connectedPlayer = IsPlayerConnected(player1) ? player1 :
                                            IsPlayerConnected(player2) ? player2 : null;
                        if (connectedPlayer != null)
                        {
                            Console.WriteLine($"{connectedPlayer.Name} prolazi automatski (protivnik diskonektovan)");
                            nextRoundPlayers.Add(connectedPlayer);
                        }
                        continue;
                    }

                    var winner = await PlayMatch(player1, player2, pointsToWin, allPlayers);
                    if (winner != null)
                    {
                        nextRoundPlayers.Add(winner);
                    }

                    _totalGames++;
                    await Task.Delay(Constants.SETUP_DELAY);
                }

                // ako ih ima neparan broj npr 3 5 jedan ide dalje 
                if (currentPlayers.Count % 2 == 1)
                {
                    var luckyPlayer = currentPlayers[^1];
                    if (IsPlayerConnected(luckyPlayer))
                    {
                        Console.WriteLine($"{luckyPlayer.Name} prolazi automatski (neparan broj)");
                        nextRoundPlayers.Add(luckyPlayer);
                    }
                }

                currentPlayers = nextRoundPlayers;
                roundNumber++;

                if (currentPlayers.Count > 1)
                {
                    Console.WriteLine($"\nProlaze u rundu {roundNumber}: {string.Join(", ", currentPlayers.ConvertAll(p => p.Name))}");
                    _leaderboardManager.DisplayLeaderboard(allPlayers);
                    await Task.Delay(Constants.GAME_END_DELAY);
                }
            }

            if (currentPlayers.Count == 1)
            {
                Console.WriteLine($"\nPOBEDNIK TURNIRA: {currentPlayers[0].Name}!");
            }
        }

        private async Task<Player?> PlayMatch(Player player1, Player player2, int pointsToWin, List<Player> allPlayers)
        {
            Console.WriteLine($"\n=== {player1.Name} vs {player2.Name} ===");

            player1.UdpPort = GetNextUdpPort();
            player2.UdpPort = GetNextUdpPort();

            player1.ResetRacketPosition();
            player2.ResetRacketPosition();

            var game = new GameState(player1, player2, player1.UdpPort, player2.UdpPort);

            string leaderboard = _leaderboardManager.CreateLeaderboardMessage(allPlayers);
            _networkManager.NotifyWaitingPlayers(allPlayers, player1, player2, leaderboard);

            _networkManager.SendGameStart(player1, player2);

            await WaitForPlayersReady(player1, player2);

        
            Console.WriteLine("Šaljem GAME_BEGIN...");
            _networkManager.SendTcpMessage(player1.TcpSocket, Constants.MSG_GAME_BEGIN);
            _networkManager.SendTcpMessage(player2.TcpSocket, Constants.MSG_GAME_BEGIN);

            await Task.Delay(Constants.SETUP_DELAY);

          
            await _gameEngine.RunGame(game, pointsToWin);

            var winner = game.GetWinner(pointsToWin);
            if (winner != null)
            {
                Console.WriteLine($"Pobednik: {winner.Name}");
            }

            game.EndGame();
            return winner;
        }

        private async Task WaitForPlayersReady(Player p1, Player p2)
        {
            bool p1Ready = false, p2Ready = false;
            DateTime startTime = DateTime.Now;

            Console.WriteLine($"Cekam da se {p1.Name} i {p2.Name} pripreme...");

            while ((!p1Ready || !p2Ready) && DateTime.Now - startTime < TimeSpan.FromMilliseconds(Constants.READY_TIMEOUT))
            {
                if (!p1Ready && p1.TcpSocket.Available > 0)
                {
                    byte[] buffer = new byte[256];
                    int bytes = p1.TcpSocket.Receive(buffer);
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    if (msg == "READY")
                    {
                        p1Ready = true;
                        Console.WriteLine($"{p1.Name} spreman!");
                    }
                }

                if (!p2Ready && p2.TcpSocket.Available > 0)
                {
                    byte[] buffer = new byte[256];
                    int bytes = p2.TcpSocket.Receive(buffer);
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    if (msg == "READY")
                    {
                        p2Ready = true;
                        Console.WriteLine($"{p2.Name} spreman!");
                    }
                }

                await Task.Delay(50);
            }

            if (p1Ready && p2Ready)
            {
                Console.WriteLine("Oba igraca spremna!");
            }
            else
            {
                Console.WriteLine("Timeout - neki igrači se nisu pripremili na vreme.");
            }
        }

        private void ResetUdpPorts()
        {
            _udpPortCounter = Constants.INITIAL_UDP_PORT;
            Console.WriteLine($"UDP portovi resetovani na {_udpPortCounter}");
        }

        private int GetNextUdpPort()
        {
            return _udpPortCounter++;
        }

        private void ShufflePlayers(List<Player> players)
        {
            var random = new Random();
            for (int i = players.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (players[i], players[j]) = (players[j], players[i]);
            }
        }

        private bool IsPlayerConnected(Player player)
        {
            try
            {
                return player.TcpSocket.Connected &&
                       !(player.TcpSocket.Poll(1, SelectMode.SelectRead) && player.TcpSocket.Available == 0);
            }
            catch
            {
                return false;
            }
        }
    }
}