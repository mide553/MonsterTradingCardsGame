using Npgsql;
using System;
using System.Collections.Generic;

namespace MCTG
{
    public class Database
    {
        private readonly string connectionString;

        public Database()
        {
            connectionString = "Host=localhost;Username=postgres;Password=password;Database=postgres";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS users (
                    username VARCHAR(50) PRIMARY KEY,
                    password VARCHAR(100),
                    token VARCHAR(100),
                    coins INTEGER DEFAULT 20,
                    elo INTEGER DEFAULT 100,
                    games_played INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS cards (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(50),
                    type VARCHAR(50),
                    power INTEGER,
                    is_spell BOOLEAN,
                    damage INTEGER,
                    owner_username VARCHAR(50) REFERENCES users(username)
                );

                CREATE TABLE IF NOT EXISTS trades (
                    id SERIAL PRIMARY KEY,
                    card_id INTEGER REFERENCES cards(id),
                    owner_username VARCHAR(50) REFERENCES users(username),
                    requirement_type VARCHAR(50),
                    min_damage INTEGER
                );

                CREATE TABLE IF NOT EXISTS deck_cards (
                    username VARCHAR(50) REFERENCES users(username),
                    card_id INTEGER REFERENCES cards(id),
                    PRIMARY KEY (username, card_id)
                );", conn);
            
            cmd.ExecuteNonQuery();
        }

        public void SaveUser(User user)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO users (username, password, token, coins, elo, games_played)
                VALUES (@username, @password, @token, @coins, @elo, @games_played)
                ON CONFLICT (username) 
                DO UPDATE SET
                    token = @token,
                    coins = @coins,
                    elo = @elo,
                    games_played = @games_played", conn);

            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("password", user.Password);
            cmd.Parameters.AddWithValue("token", user.Token ?? (object)DBNull.Value);  // Fixed null handling
            cmd.Parameters.AddWithValue("coins", user.Coins);
            cmd.Parameters.AddWithValue("elo", user.ELO);
            cmd.Parameters.AddWithValue("games_played", user.GamesPlayed);

            cmd.ExecuteNonQuery();
        }

        public User? GetUserByToken(string token)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                SELECT username, password, token, coins, elo, games_played 
                FROM users 
                WHERE token = @token", conn);
            cmd.Parameters.AddWithValue("token", token);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User(
                    reader.GetString(0),
                    reader.GetString(1))
                {
                    Token = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Coins = reader.GetInt32(3),
                    ELO = reader.GetInt32(4),
                    GamesPlayed = reader.GetInt32(5)
                };
            }
            return null;
        }

        public User? GetUserByUsername(string username)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                SELECT username, password, token, coins, elo, games_played 
                FROM users 
                WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User(
                    reader.GetString(0),
                    reader.GetString(1))
                {
                    Token = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Coins = reader.GetInt32(3),
                    ELO = reader.GetInt32(4),
                    GamesPlayed = reader.GetInt32(5)
                };
            }
            return null;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                SELECT username, password, token, coins, elo, games_played 
                FROM users", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User(
                    reader.GetString(0),
                    reader.GetString(1))
                {
                    Token = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Coins = reader.GetInt32(3),
                    ELO = reader.GetInt32(4),
                    GamesPlayed = reader.GetInt32(5)
                });
            }
            return users;
        }

        public void SaveCard(Card card, string ownerUsername)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO cards (name, type, power, is_spell, damage, owner_username)
                VALUES (@name, @type, @power, @is_spell, @damage, @owner_username)
                RETURNING id", conn);

            cmd.Parameters.AddWithValue("name", card.Name);
            cmd.Parameters.AddWithValue("type", card.Type);
            cmd.Parameters.AddWithValue("power", card.Power);
            cmd.Parameters.AddWithValue("is_spell", card.IsSpell);
            cmd.Parameters.AddWithValue("damage", card.Damage);
            cmd.Parameters.AddWithValue("owner_username", ownerUsername);

            var result = cmd.ExecuteScalar() as int?;
            card.Id = result ?? 0;
        }

        public List<Card> GetUserStack(string username)
        {
            var stack = new List<Card>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                SELECT id, name, type, power, is_spell, damage
                FROM cards
                WHERE owner_username = @username", conn);
            cmd.Parameters.AddWithValue("username", username);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var card = new Card(
                    reader.GetString(1),  // name
                    reader.GetString(2),  // type
                    reader.GetInt32(3),   // power
                    reader.GetBoolean(4), // is_spell
                    reader.GetInt32(5)    // damage
                )
                {
                    Id = reader.GetInt32(0)
                };
                stack.Add(card);
            }
            return stack;
        }

        public void UpdateUserStack(string username, List<Card> stack)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                using (var cmd = new NpgsqlCommand(@"
                    DELETE FROM deck_cards WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand(@"
                    DELETE FROM cards WHERE owner_username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.ExecuteNonQuery();
                }

                foreach (var card in stack)
                {
                    SaveCard(card, username);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void SaveUserDeck(string username, List<Card> deck)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                foreach (var card in deck)
                {
                    SaveCard(card, username);
                }

                using (var cmd = new NpgsqlCommand(@"
                    DELETE FROM deck_cards WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.ExecuteNonQuery();
                }

                foreach (var card in deck)
                {
                    using var cmd = new NpgsqlCommand(@"
                        INSERT INTO deck_cards (username, card_id)
                        VALUES (@username, @card_id)", conn);
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("card_id", card.Id);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Card> GetUserDeck(string username)
        {
            var deck = new List<Card>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                SELECT c.id, c.name, c.type, c.power, c.is_spell, c.damage
                FROM deck_cards dc
                JOIN cards c ON dc.card_id = c.id
                WHERE dc.username = @username", conn);
            cmd.Parameters.AddWithValue("username", username);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var card = new Card(
                    reader.GetString(1),  // name
                    reader.GetString(2),  // type
                    reader.GetInt32(3),   // power
                    reader.GetBoolean(4), // is_spell
                    reader.GetInt32(5)    // damage
                )
                {
                    Id = reader.GetInt32(0)
                };
                deck.Add(card);
            }
            return deck;
        }
    }
}
