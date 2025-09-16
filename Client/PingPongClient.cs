using System;
using System.Threading;
using System.Threading.Tasks;
using PingPongTurnir.Client.Services;
using PingPongTurnir.Shared;
using PingPongTurnir.Shared.Models;

namespace PingPongTurnir.Client
{
    public class PingPongClient
    {
        private string _playerName;
        private NetworkService _network;
        private bool _isGameRunning;
        private bool _isActivePlayer;
        private CancellationTokenSource _inputCancellation;
        private Task _inputTask;

        public PingPongClient()
        {
            _network = new NetworkService();
        }

        public void Run()
        {
            Console.Write("Unesi svoje ime: ");
            _playerName = Console.ReadLine() ?? "Player";

            if (!_network.ConnectToServer(_playerName))
                return;

            Console.WriteLine("Povezan na server. Cekamo pocetak igre!");

            int roundNumber = 1;
            bool shouldExit = false;

            while (!shouldExit)
            {
                string message = _network.WaitForGameStart()?.Trim();

                if (string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine("Veza prekinuta.");
                    break;
                }


                if (message.StartsWith(Constants.MSG_GAME_START))
                {
                    Console.WriteLine($"\n=== Runda {roundNumber} ===");
                    HandleGameStart(message);
                    roundNumber++;
                }
                else if (message.StartsWith(Constants.MSG_WAIT))
                {
                    Console.Clear();
                    Console.WriteLine("Cekamo sledecu rundu...");
                    Thread.Sleep(Constants.WAIT_DELAY);
                }
                else if (message.StartsWith(Constants.MSG_GAME_END))
                {
                    HandleGameEnd(message);
                    ResetGameState();
                }
                else if (message.StartsWith(Constants.MSG_LEADERBOARD))
                {
                    DisplayLeaderboard(message);
                }
                else if (message == "TOURNAMENT_END")
                {
                    shouldExit = true;
                }
            }

            Cleanup();
        }

        private void HandleGameStart(string message)
        {
            ResetGameState();

            _network.SetupUdp(message);
            _network.SendReady();

            var parts = message.Split(':');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int myPort))
            {
                if (_network.UdpPort == myPort)
                {
                    _isActivePlayer = true;
                    Console.WriteLine($"Moj mec! UDP port: {_network.UdpPort}");
                    Console.WriteLine($"Protivnik: {_network.OpponentName}");

                    StartInputHandling();
                    WaitForGameBegin();
                    _isGameRunning = true;
                    RunGameLoop();
                }
                else
                {
                    _isActivePlayer = false;
                    Console.Clear();
                    Console.WriteLine("Posmatram mec...");
                    WaitForGameEnd();
                }
            }
        }

        private void ResetGameState()
        {
            StopInputHandling();
            _isGameRunning = false;
            _isActivePlayer = false;
            _network.Stop();
        }

        private void StartInputHandling()
        {
            _inputCancellation = new CancellationTokenSource();
            _inputTask = Task.Run(async () =>
            {
                while (!_inputCancellation.Token.IsCancellationRequested)
                {
                    if (!_isGameRunning || !_isActivePlayer)
                    {
                        await Task.Delay(100, _inputCancellation.Token);
                        continue;
                    }

                    string command = InputHandler.ReadInput();
                    if (command != null && _isGameRunning && _isActivePlayer)
                    {
                        _network.SendInput(command);
                    }

                    await Task.Delay(10, _inputCancellation.Token);
                }
            }, _inputCancellation.Token);
        }

        private void StopInputHandling()
        {
            if (_inputCancellation != null && !_inputCancellation.Token.IsCancellationRequested)
            {
                _inputCancellation.Cancel();

                if (_inputTask != null)
                {
                    try
                    {
                        _inputTask.Wait(1000); 
                    }
                    catch (AggregateException)
                    {
                        
                    }
                }

                _inputCancellation?.Dispose();
                _inputCancellation = null;
                _inputTask = null;
            }
        }

        private void WaitForGameBegin()
        {
            Console.WriteLine("Cekam GAME_BEGIN...");
            while (true)
            {
                string msg = _network.WaitForGameStart()?.Trim();
                if (string.IsNullOrWhiteSpace(msg)) continue;

                if (msg.StartsWith(Constants.MSG_GAME_BEGIN))
                {
                    Console.WriteLine("Igra pocinje!");
                    break;
                }
            }
        }

        private void WaitForGameEnd()
        {
            while (true)
            {
                string message = _network.WaitForGameStart();
                if (string.IsNullOrWhiteSpace(message)) continue;

                if (message.StartsWith(Constants.MSG_GAME_END))
                {
                    HandleGameEnd(message);
                    break;
                }
                else if (message.StartsWith(Constants.MSG_LEADERBOARD))
                {
                    DisplayLeaderboard(message);
                }
            }
        }

        private void RunGameLoop()
        {
            Console.WriteLine("GameLoop pokrenut!");

            try
            {
                while (_network.IsGameRunning && _isGameRunning)
                {
                    var gameData = _network.ReceiveGameState();
                    if (gameData != null)
                    {
                        DisplayGame(gameData);
                    }
                    else if (!_network.IsGameRunning)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska u GameLoop: {ex.Message}");
            }

            _isGameRunning = false;
        }

        private void DisplayGame(GameData game)
        {
            Console.Clear();

            // shared renderer
            char[,] field = GameFieldRenderer.CreateField(game);

            // teren
            for (int y = 0; y < Constants.FIELD_HEIGHT; y++)
            {
                for (int x = 0; x < Constants.FIELD_WIDTH; x++)
                    Console.Write(field[y, x]);
                Console.WriteLine();
            }

            Console.WriteLine($"\n{game.Player1Name} [{game.Score1}] vs [{game.Score2}] {game.Player2Name}");

            if (_isActivePlayer)
            {
                Console.WriteLine("Koristite strelice za pomeranje");
            }
        }

        private void DisplayLeaderboard(string message)
        {
            Console.WriteLine("\n=== Tabela ===");
            var parts = message.Split(':');
            if (parts.Length > 1)
            {
                var playerData = parts[1].Split(';');
                int rank = 1;
                foreach (var player in playerData)
                {
                    if (!string.IsNullOrEmpty(player))
                    {
                        var playerInfo = player.Split(':');
                        if (playerInfo.Length == 3)
                        {
                            Console.WriteLine($"{rank,2}. {playerInfo[0],-20} | Wins: {playerInfo[1],3} | Points: {playerInfo[2],4}");
                            rank++;
                        }
                    }
                }
            }
            Console.WriteLine("====================");
        }

        private void HandleGameEnd(string message)
        {
            Console.WriteLine("\nKraj meca!");

            var parts = message.Split(':');
            if (parts.Length >= 4)
            {
                string result = parts[1];
                int.TryParse(parts[2], out int s1);
                int.TryParse(parts[3], out int s2);

                if (result == Constants.RESULT_WIN)
                    Console.WriteLine($"Pobeda! Rezultat: {s1} : {s2}");
                else if (result == Constants.RESULT_LOSE)
                    Console.WriteLine($"Poraz. Rezultat: {s1} : {s2}");
                else
                    Console.WriteLine($"Rezultat: {s1} : {s2}");
            }

            Thread.Sleep(Constants.GAME_END_DELAY);
        }

        private void Cleanup()
        {
            StopInputHandling();
            _network.ShutdownAll();
            Console.WriteLine("Cleanup zavrsen.");
        }
    }
}