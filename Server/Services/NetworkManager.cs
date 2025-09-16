using System.Net;
using System.Net.Sockets;
using System.Text;
using PingPongTurnir.Shared;
using PingPongTurnir.Server.Models;
using PingPongTurnir.Shared.Utils;

namespace PingPongTurnir.Server.Services
{
    public class NetworkManager
    {
        private Socket? _serverSocket;
        private Dictionary<int, CancellationTokenSource> _udpCancellationTokens = new();

        public void StartTcpServer(int maxPlayers)
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Constants.SERVER_PORT));
            _serverSocket.Listen(maxPlayers);
            Console.WriteLine($"TCP Server pokrenut na port {Constants.SERVER_PORT}");
        }

        public Socket? AcceptClientWithTimeout(int timeoutMs = 1000)
        {
            if (_serverSocket == null) return null;

            _serverSocket.Blocking = false;
            List<Socket> checkRead = new() { _serverSocket };
            Socket.Select(checkRead, null, null, timeoutMs * 1000);

            if (checkRead.Count > 0)
            {
                try
                {
                    var client = _serverSocket.Accept();
                    client.Blocking = true; 
                    return client;
                }
                catch (SocketException)
                {
                    return null;
                }
            }

            return null;
        }

        public string ReceivePlayerName(Socket clientSocket)
        {
            if (clientSocket.Poll(3000000, SelectMode.SelectRead)) 
            {
                byte[] buffer = new byte[256];
                int bytesReceived = clientSocket.Receive(buffer);
                return Encoding.UTF8.GetString(buffer, 0, bytesReceived).Trim();
            }
            throw new Exception("Timeout pri primanju imena igraca");
        }

        public void SendTcpMessage(Socket socket, string message)
        {
            if (!socket.Connected) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                socket.Send(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP send greska: {ex.Message}");
            }
        }

        public void SendTcpMessageToAll(List<Player> players, string message)
        {
            foreach (var player in players)
            {
                SendTcpMessage(player.TcpSocket, message);
            }
        }

        public async Task SendGameStateUdp(GameState game)
        {
            var gameData = game.ToGameData();
            string json = JsonHelper.Serialize(gameData);
            byte[] data = Encoding.UTF8.GetBytes(json);

            try
            {
                var endpoint1 = new IPEndPoint(game.Player1.EndPoint.Address, game.UdpPort1 + Constants.UDP_RECEIVE_OFFSET);
                var endpoint2 = new IPEndPoint(game.Player2.EndPoint.Address, game.UdpPort2 + Constants.UDP_RECEIVE_OFFSET);

                using var udpClient = new UdpClient();

                await udpClient.SendAsync(data, data.Length, endpoint1);
                await udpClient.SendAsync(data, data.Length, endpoint2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP send greska: {ex.Message}");
            }
        }

        public Task HandleUdpPlayerInput(int udpPort, Player player, GameState game)
        {
            var cts = new CancellationTokenSource();
            _udpCancellationTokens[udpPort] = cts;

            return Task.Run(async () =>
            {
                using var udpServer = new UdpClient();
                udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpServer.Client.Bind(new IPEndPoint(IPAddress.Any, udpPort));

                while (game.IsGameRunning && !cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await udpServer.ReceiveAsync();
                        string command = Encoding.UTF8.GetString(result.Buffer).Trim();

                        switch (command)
                        {
                            case Constants.MSG_PLAYER_MOVE_UP:
                                player.MoveRacketUp();
                                break;
                            case Constants.MSG_PLAYER_MOVE_DOWN:
                                player.MoveRacketDown();
                                break;
                        }
                    }
                    catch (Exception ex) when (!cts.Token.IsCancellationRequested)
                    {
                        Console.WriteLine($"UDP input greska: {ex.Message}");
                        break;
                    }
                }
            }, cts.Token);
        }

        public void StopUdpInput(GameState game)
        {
            foreach (var port in new[] { game.UdpPort1, game.UdpPort2 })
            {
                if (_udpCancellationTokens.TryGetValue(port, out var cts))
                {
                    cts.Cancel();
                    _udpCancellationTokens.Remove(port);
                }
            }
        }

        public void SendGameStart(Player player1, Player player2)
        {
            string msg1 = $"{Constants.MSG_GAME_START}:{player1.UdpPort}:{player2.Name}";
            string msg2 = $"{Constants.MSG_GAME_START}:{player2.UdpPort}:{player1.Name}";

            SendTcpMessage(player1.TcpSocket, msg1);
            SendTcpMessage(player2.TcpSocket, msg2);
        }

        public void SendGameEnd(GameState game, Player winner, Player loser)
        {
            string winMsg = $"{Constants.MSG_GAME_END}:{Constants.RESULT_WIN}:{game.Player1Score}:{game.Player2Score}";
            string loseMsg = $"{Constants.MSG_GAME_END}:{Constants.RESULT_LOSE}:{game.Player1Score}:{game.Player2Score}";

            SendTcpMessage(winner.TcpSocket, winMsg);
            SendTcpMessage(loser.TcpSocket, loseMsg);
        }

        public void NotifyWaitingPlayers(List<Player> allPlayers, Player p1, Player p2, string leaderboard)
        {
            foreach (var player in allPlayers)
            {
                if (player != p1 && player != p2)
                {
                    SendTcpMessage(player.TcpSocket, Constants.MSG_WAIT);
                    SendTcpMessage(player.TcpSocket, leaderboard);
                }
            }
        }

        public void Stop()
        {
            foreach (var cts in _udpCancellationTokens.Values)
            {
                cts.Cancel();
            }
            _udpCancellationTokens.Clear();

            _serverSocket?.Close();
            _serverSocket?.Dispose();
        }
    }
}