using System.Net;
using System.Net.Sockets;
using System.Text;
using PingPongTurnir.Shared;
using PingPongTurnir.Shared.Models;
using PingPongTurnir.Shared.Utils;

namespace PingPongTurnir.Client.Services
{
    public class NetworkService
    {
        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;
        private UdpClient _udpSender;
        private UdpClient _udpReceiver;

        public string PlayerName { get; private set; } = string.Empty;
        public string OpponentName { get; private set; } = string.Empty;
        public int UdpPort { get; private set; }
        public bool IsGameRunning { get; private set; } = false;

        private const int TCP_TIMEOUT = 5000;
        private const int UDP_TIMEOUT = 3000;

        public bool ConnectToServer(string playerName)
        {
            try
            {
                PlayerName = playerName;
                _tcpClient = new TcpClient();
                _tcpClient.Connect(Constants.SERVER_IP, Constants.SERVER_PORT);
                _tcpStream = _tcpClient.GetStream();

                SendTcpMessage(playerName);
                Console.WriteLine($"Povezan na server kao {playerName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska pri povezivanju: {ex.Message}");
                return false;
            }
        }

        public string WaitForGameStart()
        {
            if (_tcpStream == null) return string.Empty;

            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _tcpStream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("Server zatvorio konekciju");
                    return string.Empty;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"Primljena poruka: {message}");
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska u TCP komunikaciji: {ex.Message}");
                return string.Empty;
            }
        }

        public void SetupUdp(string gameStartMessage)
        {
            CleanupUdp();

            var parts = gameStartMessage.Split(':');
            if (parts.Length < 3) return;

            UdpPort = int.Parse(parts[1]);
            OpponentName = parts[2];

            try
            {
                _udpSender = new UdpClient();
                _udpReceiver = new UdpClient(UdpPort + 100); 

                Console.WriteLine($"UDP setup: send={UdpPort}, recv={UdpPort + 100}");
                IsGameRunning = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP setup greska: {ex.Message}");
                IsGameRunning = false;
            }
        }

        private void CleanupUdp()
        {
            IsGameRunning = false;

            _udpSender?.Close();
            _udpSender?.Dispose();
            _udpSender = null;

            _udpReceiver?.Close();
            _udpReceiver?.Dispose();
            _udpReceiver = null;
        }

        public void SendInput(string command)
        {
            if (!IsGameRunning || _udpSender == null) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(command);
                _udpSender.Send(data, data.Length, Constants.SERVER_IP, UdpPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska pri slanju komande: {ex.Message}");
            }
        }

        public GameData? ReceiveGameState()
        {
            if (!IsGameRunning || _udpReceiver == null) return null;

            try
            {
                _udpReceiver.Client.ReceiveTimeout = UDP_TIMEOUT;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpReceiver.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(data).Trim();

                if (json.StartsWith("{"))
                {
                    return JsonHelper.Deserialize<GameData>(json);
                }

                return null;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                return null; 
            }
            catch (ObjectDisposedException)
            {
                IsGameRunning = false;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP receive greska: {ex.Message}");
                IsGameRunning = false;
                return null;
            }
        }

        public void SendReady()
        {
            SendTcpMessage("READY");
        }

        private void SendTcpMessage(string message)
        {
            if (_tcpStream?.CanWrite != true) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                _tcpStream.Write(data, 0, data.Length);
                _tcpStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP send greska: {ex.Message}");
            }
        }

        public void Stop()
        {
            CleanupUdp();
        }

        public void ShutdownAll()
        {
            Stop();

            _tcpStream?.Close();
            _tcpStream?.Dispose();
            _tcpStream = null;

            _tcpClient?.Close();
            _tcpClient?.Dispose();
            _tcpClient = null;

            Console.WriteLine($"Sve konekcije zatvorene za {PlayerName}");
        }
    }
}