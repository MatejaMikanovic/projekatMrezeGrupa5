using System;
using PingPongTurnir.Shared;
using PingPongTurnir.Server.Models;

namespace PingPongTurnir.Server.Models
{
    public class Ball
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int VelX { get; set; } // brzina po X osi (-1 ili 1)
        public int VelY { get; set; } // brzina po Y osi (-1, 0, ili 1)

        private readonly Random _random = new();

        public Ball()
        {
            Reset();
        }

        public void Reset()
        {
            // centriranje
            X = Constants.FIELD_WIDTH / 2;
            Y = Constants.FIELD_HEIGHT / 2;

            // random pravac (levo ili desno)
            VelX = _random.Next(2) == 0 ? -1 : 1;

            // random ugao (-1, 0, ili 1)
            VelY = _random.Next(3) - 1;
        }

        public void Move()
        {
            X += VelX;
            Y += VelY;
        }

        public void ReverseX()
        {
            VelX = -VelX;
        }

        public void ReverseY()
        {
            VelY = -VelY;
        }

        // provere kolizije
        public bool IsOutOfBounds()
        {
            return X <= 0 || X >= Constants.FIELD_WIDTH - 1;
        }

        public bool HitTopOrBottomWall()
        {
            return Y <= 1 || Y >= Constants.FIELD_HEIGHT - 2;
        }

        public bool IsNearLeftRacket()
        {
            return X <= 2;
        }

        public bool IsNearRightRacket()
        {
            return X >= Constants.FIELD_WIDTH - 3;
        }

        public bool CollidesWithRacket(Player player)
        {
            return Y >= player.RacketY && Y < player.RacketY + Constants.RACKET_SIZE;
        }
    }
}