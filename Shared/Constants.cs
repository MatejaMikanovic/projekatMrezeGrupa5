namespace PingPongTurnir.Shared
{
    public static class Constants
    {
        // Network konstante
        public const string SERVER_IP = "127.0.0.1";
        public const int SERVER_PORT = 12000;
        public const int INITIAL_UDP_PORT = 13000;

        // Game konstante
        public const int FIELD_WIDTH = 40;
        public const int FIELD_HEIGHT = 20;
        public const int RACKET_SIZE = 4;
        public const int GAME_TICK_DELAY = 200;

        // Timing konstante 
        public const int WAIT_DELAY = 1000;
        public const int GAME_END_DELAY = 2000;
        public const int SETUP_DELAY = 500;
        public const int SHORT_DELAY = 100;
        public const int READY_TIMEOUT = 10000;

        // TCP poruke
        public const string MSG_GAME_BEGIN = "GAME_BEGIN";
        public const string MSG_GAME_START = "GAME_START";
        public const string MSG_GAME_END = "GAME_END";
        public const string MSG_LEADERBOARD = "LEADERBOARD";
        public const string MSG_WAIT = "WAIT";
        public const string MSG_TOURNAMENT_END = "TOURNAMENT_END";

        // Input poruke
        public const string MSG_PLAYER_MOVE_UP = "UP";
        public const string MSG_PLAYER_MOVE_DOWN = "DOWN";

        // Rezultat poruke
        public const string RESULT_WIN = "WIN";
        public const string RESULT_LOSE = "LOSE";

        // UDP port offset 
        public const int UDP_RECEIVE_OFFSET = 100;
    }
}