using System;
using PingPongTurnir.Shared.Models;

namespace PingPongTurnir.Shared
{
    public static class GameFieldRenderer
    {
        public static char[,] CreateField(GameData game)
        {
            char[,] field = new char[Constants.FIELD_HEIGHT, Constants.FIELD_WIDTH];

            // fillovanje
            for (int y = 0; y < Constants.FIELD_HEIGHT; y++)
                for (int x = 0; x < Constants.FIELD_WIDTH; x++)
                    field[y, x] = ' ';

            // Reket 1 levi
            for (int i = 0; i < Constants.RACKET_SIZE; i++)
            {
                if (game.Player1Y + i >= 0 && game.Player1Y + i < Constants.FIELD_HEIGHT)
                    field[game.Player1Y + i, 0] = '|';
            }

            // Reket 2 desni
            for (int i = 0; i < Constants.RACKET_SIZE; i++)
            {
                if (game.Player2Y + i >= 0 && game.Player2Y + i < Constants.FIELD_HEIGHT)
                    field[game.Player2Y + i, Constants.FIELD_WIDTH - 1] = '|';
            }

            // lopta
            if (game.BallX >= 0 && game.BallX < Constants.FIELD_WIDTH &&
                game.BallY >= 0 && game.BallY < Constants.FIELD_HEIGHT)
            {
                field[game.BallY, game.BallX] = 'O';
            }

            return field;
        }

        public static void PrintField(char[,] field)
        {
            for (int y = 0; y < Constants.FIELD_HEIGHT; y++)
            {
                for (int x = 0; x < Constants.FIELD_WIDTH; x++)
                    Console.Write(field[y, x]);
                Console.WriteLine();
            }
        }
    }
}