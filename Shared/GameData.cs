namespace PingPongTurnir.Shared.Models
{
    public class GameData
    {
        public int BallX { get; set; }
        public int BallY { get; set; }

        public int Player1Y { get; set; }
        public int Player2Y { get; set; }

        public int Score1 { get; set; }
        public int Score2 { get; set; }

        public string Player1Name { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
    }

    public class LeaderboardEntry
    {
        public string Name { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Points { get; set; }
        public int Rank { get; set; }
    }
}
