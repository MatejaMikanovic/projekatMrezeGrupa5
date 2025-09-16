using System.Net.Sockets;
using PingPongTurnir.Shared.Models;

namespace PingPongTurnir.Server.Models
{
    public class GameState
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Ball Ball { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public bool IsGameRunning { get; set; }
        public int UdpPort1 { get; set; }
        public int UdpPort2 { get; set; }

        public GameState(Player player1, Player player2, int udpPort1, int udpPort2)
        {
            Player1 = player1;
            Player2 = player2;
            Ball = new Ball();
            Player1Score = 0;
            Player2Score = 0;
            IsGameRunning = true;
            UdpPort1 = udpPort1;
            UdpPort2 = udpPort2;

            

            player1.UdpPort = udpPort1;
            player2.UdpPort = udpPort2;
        }

        public GameData ToGameData()
        {
            return new GameData
            {
                BallX = Ball.X,
                BallY = Ball.Y,
                Player1Y = Player1.RacketY,
                Player2Y = Player2.RacketY,
                Score1 = Player1Score,
                Score2 = Player2Score,
                Player1Name = Player1.Name,
                Player2Name = Player2.Name
            };
        }

        public Player? GetWinner(int pointsToWin)
        {
            if (Player1Score >= pointsToWin)
                return Player1;
            if (Player2Score >= pointsToWin)
                return Player2;
            return null;
        }

        public Player GetLoser(Player winner)
        {
            return winner == Player1 ? Player2 : Player1;
        }

        public void ScoreForPlayer1()
        {
            Player1Score++;
            Player1.AddPoints(1);
            ResetPositions();
        }

        public void ScoreForPlayer2()
        {
            Player2Score++;
            Player2.AddPoints(1);
            ResetPositions();
        }

        private void ResetPositions()
        {
            Ball.Reset();
            Player1.ResetRacketPosition();
            Player2.ResetRacketPosition();
        }

        public void EndGame()
        {
            IsGameRunning = false;

        }
    }
}