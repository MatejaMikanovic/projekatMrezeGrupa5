using System;
using PingPongTurnir.Shared;

namespace PingPongTurnir.Client.Services
{
    public static class InputHandler
    {
        public static string? ReadInput()
        {
            if (!Console.KeyAvailable)
                return null;

            var key = Console.ReadKey(true).Key;

            return key switch
            {
                ConsoleKey.UpArrow => Constants.MSG_PLAYER_MOVE_UP,
                ConsoleKey.DownArrow => Constants.MSG_PLAYER_MOVE_DOWN,
                _ => null
            };
        }
    }
}
