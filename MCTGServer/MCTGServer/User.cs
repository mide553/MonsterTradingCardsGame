using System.Collections.Generic;

namespace MCTG
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Token { get; set; } // Marked as nullable
        public int Coins { get; set; } = 20;
        public List<Card> Stack { get; set; } = new List<Card>();
        public List<Card> Deck { get; set; } = new List<Card>();
        public int ELO { get; set; } = 100; // Add ELO property
        public int GamesPlayed { get; set; } = 0; // Add GamesPlayed property

        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}