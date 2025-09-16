using System;
using System.Threading.Tasks;
using PingPongTurnir.Server.Models;
using PingPongTurnir.Shared;

namespace PingPongTurnir.Server.Services
{
    public class GameEngine
    {
        private readonly NetworkManager _networkManager;

        public GameEngine(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public async Task RunGame(GameState game, int pointsToWin)
        {
            if (!IsGameValid(game))
            {
                Console.WriteLine("Igra nije validna - jedan od igrača nije povezan");
                return;
            }

            Console.WriteLine($"=== Pokretanje: {game.Player1.Name} vs {game.Player2.Name} ===");

            Task input1 = _networkManager.HandleUdpPlayerInput(game.UdpPort1, game.Player1, game);
            Task input2 = _networkManager.HandleUdpPlayerInput(game.UdpPort2, game.Player2, game);

            while (game.IsGameRunning && game.GetWinner(pointsToWin) == null)
            {
                ProcessBallPhysics(game);

                await _networkManager.SendGameStateUdp(game);

                await Task.Delay(Constants.GAME_TICK_DELAY);
            }

            var winner = game.GetWinner(pointsToWin);
            if (winner != null)
            {
                winner.AddWin();
                var loser = game.GetLoser(winner);

                Console.WriteLine($"Pobednik: {winner.Name} ({winner.Wins} pobeda, {winner.Points} poena)");
                Console.WriteLine($"Rezultat: {game.Player1Score} : {game.Player2Score}");

                _networkManager.SendGameEnd(game, winner, loser);
            }

            _networkManager.StopUdpInput(game);
            game.EndGame();
        }

        private bool IsGameValid(GameState game)
        {
            return game.Player1.TcpSocket.Connected && game.Player2.TcpSocket.Connected;
        }

        private void ProcessBallPhysics(GameState game)
        {
            var ball = game.Ball;

            ball.Move();

            // da li smo udarili gore/dole
            if (ball.HitTopOrBottomWall())
            {
                ball.ReverseY();
                // granica da ne izadjemo van terena
                ball.Y = Math.Max(2, Math.Min(Constants.FIELD_HEIGHT - 3, ball.Y));
            }

            // ako je lopta udarila levi reket
            if (ball.IsNearLeftRacket())
            {
                if (ball.CollidesWithRacket(game.Player1))
                {
                    HandleRacketHit(ball, game.Player1);
                    ball.X = 3; 
                }
                else if (ball.IsOutOfBounds())
                {
                    game.ScoreForPlayer2();
                    Console.WriteLine($"Poen za {game.Player2.Name}! ({game.Player1Score}:{game.Player2Score})");
                    ball.Reset();
                }
            }
            // ako je lopta udarila desni reket
            else if (ball.IsNearRightRacket())
            {
                if (ball.CollidesWithRacket(game.Player2))
                {
                    HandleRacketHit(ball, game.Player2);
                    ball.X = Constants.FIELD_WIDTH - 4; 
                }
                else if (ball.IsOutOfBounds())
                {
                    game.ScoreForPlayer1();
                    Console.WriteLine($"Poen za {game.Player1.Name}! ({game.Player1Score}:{game.Player2Score})");
                    ball.Reset();
                }
            }
        }

        private void HandleRacketHit(Ball ball, Player player)
        {
            ball.ReverseX();

            int hitPosition = ball.Y - player.RacketY;
            int racketMiddle = Constants.RACKET_SIZE / 2;

            if (hitPosition < racketMiddle - 1)
            {
                ball.VelY = -1; // Udari prema gore
            }
            else if (hitPosition > racketMiddle)
            {
                ball.VelY = 1;  // Udari prema dole
            }
            else
            {
                ball.VelY = 0;  // Pravo
            }
        }
    }
}