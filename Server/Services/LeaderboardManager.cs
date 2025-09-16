using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingPongTurnir.Shared.Models;
using PingPongTurnir.Server.Models;
using PingPongTurnir.Shared;


namespace PingPongTurnir.Server.Services
{
    public class LeaderboardManager
    {
        public void DisplayLeaderboard(List<Player> players)
        {
            Console.WriteLine("\n=== Tabela ===");
            var sortedPlayers = GetSortedPlayers(players);

            int rank = 1;
            foreach (var player in sortedPlayers)
            {
                Console.WriteLine($"{rank,2}. {player.Name,-20} | Wins: {player.Wins,3} | Points: {player.Points,4}");
                rank++;
            }
            Console.WriteLine("====================");
        }

        public List<LeaderboardEntry> GetLeaderboardEntries(List<Player> players)
        {
            var sortedPlayers = GetSortedPlayers(players);
            var entries = new List<LeaderboardEntry>();

            int rank = 1;
            foreach (var player in sortedPlayers)
            {
                entries.Add(new LeaderboardEntry
                {
                    Name = player.Name,
                    Wins = player.Wins,
                    Points = player.Points,
                    Rank = rank++
                });
            }

            return entries;
        }

        public string CreateLeaderboardMessage(List<Player> players)
        {
            var sortedPlayers = GetSortedPlayers(players);
            var message = new StringBuilder(Constants.MSG_LEADERBOARD + ":");

            foreach (var player in sortedPlayers)
            {
                message.Append($"{player.Name}:{player.Wins}:{player.Points};");
            }

            return message.ToString();
        }

        private List<Player> GetSortedPlayers(List<Player> players)
        {
            return players
                .OrderByDescending(p => p.Wins)
                .ThenByDescending(p => p.Points)
                .ThenBy(p => p.Name) 
                .ToList();
        }

        public void ShowTournamentStats(List<Player> players, int totalGames)
        {
            Console.WriteLine("\n=== Statistika Turnira ===");
            Console.WriteLine($"Broj igraca: {players.Count}");
            Console.WriteLine($"Broj odigranih meceva: {totalGames}");
            Console.WriteLine($"Broj skupljenih poena: {players.Sum(p => p.Points)}");

            var topPlayer = players.OrderByDescending(p => p.Wins).ThenByDescending(p => p.Points).FirstOrDefault();
            if (topPlayer != null)
            {
                Console.WriteLine($"Pobednik turnira: {topPlayer.Name}");
                Console.WriteLine($"Pobednikova statistika: {topPlayer.Wins} wins, {topPlayer.Points} points");
            }

            Console.WriteLine("==============================");
        }
    }
}