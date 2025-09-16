using System.Net;
using System.Net.Sockets;
using PingPongTurnir.Shared;

namespace PingPongTurnir.Server.Models
{
    public class Player
    {
        public string Name { get; set; }
        public Socket TcpSocket { get; set; }
        public int Wins { get; set; }
        public int Points { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public int UdpPort { get; set; }
        public int RacketY { get; set; }

        public Player(string name, Socket socket)
        {
            Name = name;
            TcpSocket = socket;
            Wins = 0;
            Points = 0;
            EndPoint = (IPEndPoint)socket.RemoteEndPoint!;
            RacketY = Constants.FIELD_HEIGHT / 2 - Constants.RACKET_SIZE / 2; 
        }

        public void MoveRacketUp()
        {
            if (RacketY > 1)
                RacketY--;
        }

        public void MoveRacketDown()
        {
            if (RacketY < Constants.FIELD_HEIGHT - Constants.RACKET_SIZE - 1)
                RacketY++;
        }

        public void AddWin()
        {
            Wins++;
        }

        public void AddPoints(int points)
        {
            Points += points;
        }

        public void ResetRacketPosition()
        {
            RacketY = Constants.FIELD_HEIGHT / 2 - Constants.RACKET_SIZE / 2;
        }
    }
}