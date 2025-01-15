using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCTG
{
    public class Server
    {
        private readonly Database db;
        private List<User> users = new List<User>();
        private List<Card> cards = new List<Card>();
        private List<Trade> trades = new List<Trade>();

        public Server()
        {
            db = new Database();
            users = db.GetAllUsers();
        }

        public void Start()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 10001);
            server.Start();
            Console.WriteLine("Server started on port 10001");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] requestLines = request.Split("\r\n");

                if (requestLines.Length > 0)
                {
                    string[] requestLine = requestLines[0].Split(" ");
                    if (requestLine.Length >= 2)
                    {
                        string method = requestLine[0];
                        string path = requestLine[1];

                        LogMessage($"Received request: {method} {path}", ConsoleColor.Yellow);

                        if (method == "POST" && path == "/register")
                        {
                            HandleRegister(requestLines, stream);
                        }
                        else if (method == "POST" && path == "/login")
                        {
                            HandleLogin(requestLines, stream);
                        }
                        else if (method == "POST" && path == "/cards")
                        {
                            HandleAddCard(requestLines, stream);
                        }
                        else if (method == "GET" && path == "/cards")
                        {
                            HandleGetCards(requestLines, stream);
                        }
                        else if (method == "POST" && path == "/acquire")
                        {
                            HandleAcquirePackage(requestLines, stream);
                        }
                        else if (method == "POST" && path == "/deck")
                        {
                            HandleManageDeck(requestLines, stream);
                        }
                        else if (method == "POST" && path == "/battle")
                        {
                            HandleBattle(requestLines, stream);
                        }
                        else if (method == "POST" && path == "/trade")
                        {
                            HandleTrade(requestLines, stream);
                        }
                        else if (method == "GET" && path == "/stack")
                        {
                            HandleGetStack(requestLines, stream);
                        }
                        else if (method == "GET" && path == "/scoreboard")
                        {
                            HandleScoreboard(requestLines, stream);
                        }
                        else if (method == "GET" && path == "/profile")
                        {
                            HandleProfile(requestLines, stream);
                        }
                        else
                        {
                            HandleNotFound(stream);
                        }
                    }
                }

                client.Close();
            }
        }

        private void LogMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = color;
            Console.WriteLine($"[{timestamp}] {message}");
            Console.ResetColor();
        }

        private void SendResponse(NetworkStream stream, int statusCode, object content)
        {
            string statusText = statusCode switch
            {
                200 => "OK",
                201 => "Created",
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Not Found",
                _ => "Internal Server Error"
            };

            string response = $"HTTP/1.1 {statusCode} {statusText}\r\n" +
                             "Content-Type: application/json\r\n\r\n" +
                             JsonConvert.SerializeObject(content, Formatting.Indented);
            
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
            stream.Flush();
        }

        private void HandleRegister(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Register request received", ConsoleColor.Cyan);

            string body = requestLines[requestLines.Length - 1];
            var user = JsonConvert.DeserializeObject<User>(body);
            if (user != null)
            {
                db.SaveUser(user);
                users = db.GetAllUsers();
                SendResponse(stream, 201, new 
                { 
                    status = "success",
                    message = "User registered successfully!",
                    username = user.Username
                });
                LogMessage($"User registered: {user.Username}", ConsoleColor.Green);
            }
        }

        private void HandleLogin(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Login request received", ConsoleColor.Cyan);

            string body = requestLines[requestLines.Length - 1];
            var loginRequest = JsonConvert.DeserializeObject<User>(body);
            var user = db.GetUserByUsername(loginRequest?.Username ?? "");

            if (user != null && user.Password == loginRequest?.Password)
            {
                user.Token = Guid.NewGuid().ToString();
                db.SaveUser(user);
                users = db.GetAllUsers();
                SendResponse(stream, 200, new 
                { 
                    token = user.Token,
                    status = "success",
                    message = "Login successful!",
                    username = user.Username
                });
                LogMessage($"User logged in: {user.Username}", ConsoleColor.Green);
            }
            else
            {
                SendResponse(stream, 401, new 
                { 
                    status = "error",
                    message = "Invalid credentials!"
                });
                LogMessage("Login failed: Invalid credentials", ConsoleColor.Red);
            }
        }

        private void HandleAddCard(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Add card request received", ConsoleColor.Cyan);

            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null)
            {
                string body = requestLines[requestLines.Length - 1];
                var card = JsonConvert.DeserializeObject<Card>(body);
                if (card != null)
                {
                    db.SaveCard(card, user.Username);
                    SendResponse(stream, 201, new 
                    { 
                        status = "success",
                        message = "Card added successfully!",
                        card = new 
                        {
                            name = card.Name,
                            type = card.Type,
                            power = card.Power,
                            damage = card.Damage
                        }
                    });
                    LogMessage($"Card added: {card.Name} for user {user.Username}", ConsoleColor.Green);
                }
            }
            else
            {
                SendResponse(stream, 401, new 
                { 
                    status = "error",
                    message = "Invalid token!" 
                });
                LogMessage("Add card failed: Invalid token", ConsoleColor.Red);
            }
        }

        private void HandleGetCards(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Get cards request received", ConsoleColor.Cyan);

            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null)
            {
                SendResponse(stream, 200, new 
                { 
                    status = "success",
                    message = "Cards retrieved successfully!",
                    cards = cards
                });
                LogMessage($"Cards retrieved for user: {user.Username}", ConsoleColor.Green);
            }
            else
            {
                SendResponse(stream, 401, new 
                { 
                    status = "error",
                    message = "Invalid token!" 
                });
                LogMessage("Get cards failed: Invalid token", ConsoleColor.Red);
            }
        }

        private void HandleAcquirePackage(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Acquire package request received", ConsoleColor.Cyan);

            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null && user.Coins >= 5)
            {
                user.Coins -= 5;
                var package = GeneratePackage();
                user.Stack.AddRange(package);

                db.SaveUser(user);

                foreach (var card in package)
                {
                    db.SaveCard(card, user.Username);
                }

                SendResponse(stream, 200, new 
                { 
                    status = "success",
                    message = "Package acquired successfully!",
                    package = package,
                    remainingCoins = user.Coins 
                });
                LogMessage($"Package acquired by user: {user.Username} (Coins remaining: {user.Coins})", ConsoleColor.Green);
            }
            else
            {
                SendResponse(stream, 400, new 
                { 
                    status = "error",
                    message = "Not enough coins or invalid token!" 
                });
                LogMessage("Package acquisition failed: Insufficient coins or invalid token", ConsoleColor.Red);
            }
        }

        private void HandleManageDeck(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Manage deck request received", ConsoleColor.Cyan);

            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null)
            {
                user.Stack = db.GetUserStack(user.Username);

                string body = requestLines[requestLines.Length - 1];
                var deck = JsonConvert.DeserializeObject<List<Card>>(body);

                if (deck != null)
                {
                    if (deck.Count != 4)
                    {
                        SendResponse(stream, 400, new { 
                            status = "error", 
                            message = "Deck must contain exactly 4 cards!",
                            currentCount = deck.Count
                        });
                        LogMessage($"Deck update failed: Invalid card count ({deck.Count})", ConsoleColor.Red);
                        return;
                    }

                    bool cardsInStack = deck.All(card => 
                        user.Stack.Any(stackCard => 
                            stackCard.Name == card.Name &&
                            stackCard.Type == card.Type &&
                            stackCard.Power == card.Power &&
                            stackCard.IsSpell == card.IsSpell &&
                            stackCard.Damage == card.Damage));

                    if (cardsInStack)
                    {
                        var actualDeck = deck.Select(card => 
                            user.Stack.First(stackCard => 
                                stackCard.Name == card.Name &&
                                stackCard.Type == card.Type &&
                                stackCard.Power == card.Power &&
                                stackCard.IsSpell == card.IsSpell &&
                                stackCard.Damage == card.Damage)).ToList();

                        user.Deck = actualDeck;
                        user.Stack = user.Stack.Except(actualDeck).ToList();

                        db.UpdateUserStack(user.Username, user.Stack);
                        db.SaveUserDeck(user.Username, user.Deck);
                        SendResponse(stream, 200, new { 
                            status = "success",
                            message = "Deck updated successfully!",
                            deck = actualDeck
                        });
                        LogMessage($"Deck updated for user: {user.Username}", ConsoleColor.Green);
                    }
                    else
                    {
                        SendResponse(stream, 400, new { 
                            status = "error",
                            message = "Cards not in user's stack!",
                            requiredCards = deck.Select(c => c.Name),
                            availableCards = user.Stack.Select(c => c.Name)
                        });
                        LogMessage("Deck update failed: Cards not in stack", ConsoleColor.Red);
                    }
                }
            }
        }

        private void HandleBattle(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Battle request received", ConsoleColor.Cyan);

            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null)
            {
                user.Deck = db.GetUserDeck(user.Username);

                if (user.Deck.Count == 4)
                {
                    string body = requestLines[requestLines.Length - 1];
                    var opponentUsername = JsonConvert.DeserializeObject<string>(body);
                    var opponent = users.Find(u => u.Username == opponentUsername);

                    if (opponent != null)
                    {
                        opponent.Deck = db.GetUserDeck(opponent.Username);

                        if (opponent.Deck.Count == 4)
                        {
                            var battleLog = new StringBuilder();
                            battleLog.AppendLine($"=== Battle: {user.Username} vs {opponent.Username} ===");
                            battleLog.AppendLine();

                            var userDeck = new List<Card>(user.Deck);
                            var opponentDeck = new List<Card>(opponent.Deck);
                            var random = new Random();
                            var rounds = 0;
                            var userWins = 0;
                            var opponentWins = 0;

                            LogMessage($"Battle started: {user.Username} vs {opponent.Username}", ConsoleColor.Yellow);
                            battleLog.AppendLine($"=== Battle: {user.Username} vs {opponent.Username} ===\n");

                            while (rounds < 100 && userDeck.Count > 0 && opponentDeck.Count > 0)
                            {
                                var userCard = userDeck[random.Next(userDeck.Count)];
                                var opponentCard = opponentDeck[random.Next(opponentDeck.Count)];

                                battleLog.AppendLine($"Round {rounds + 1}:");
                                battleLog.AppendLine($"{user.Username}: {userCard.Name} ({userCard.Damage} dmg)");
                                battleLog.AppendLine($"{opponent.Username}: {opponentCard.Name} ({opponentCard.Damage} dmg)");

                                if (userCard.IsWaterSpell && opponentCard.IsKnight)
                                    battleLog.AppendLine($"Special: {opponentCard.Name} drowns instantly against {userCard.Name}!");
                                else if (opponentCard.IsWaterSpell && userCard.IsKnight)
                                    battleLog.AppendLine($"Special: {userCard.Name} drowns instantly against {opponentCard.Name}!");
                                else if (userCard.IsGoblin && opponentCard.IsDragon)
                                    battleLog.AppendLine($"Special: {userCard.Name} is too afraid to attack {opponentCard.Name}!");
                                else if (opponentCard.IsGoblin && userCard.IsDragon)
                                    battleLog.AppendLine($"Special: {opponentCard.Name} is too afraid to attack {userCard.Name}!");

                                int userDamage = CalculateDamage(userCard, opponentCard);
                                int opponentDamage = CalculateDamage(opponentCard, userCard);

                                if (userDamage > opponentDamage)
                                {
                                    battleLog.AppendLine($"Winner: {user.Username}\n");
                                    opponentDeck.Remove(opponentCard);
                                    userDeck.Add(opponentCard);
                                    userWins++;
                                }
                                else if (opponentDamage > userDamage)
                                {
                                    battleLog.AppendLine($"Winner: {opponent.Username}\n");
                                    userDeck.Remove(userCard);
                                    opponentDeck.Add(userCard);
                                    opponentWins++;
                                }
                                else
                                {
                                    battleLog.AppendLine("Result: Draw!\n");
                                }
                                
                                battleLog.AppendLine();
                                rounds++;
                            }

                            string winner = userDeck.Count > opponentDeck.Count ? user.Username : opponent.Username;
                            battleLog.AppendLine($"=== Battle Results ===");
                            battleLog.AppendLine($"Winner: {winner}");
                            battleLog.AppendLine($"Rounds: {rounds}");
                            battleLog.AppendLine($"Score - {user.Username}: {userWins}, {opponent.Username}: {opponentWins}");

                            user.GamesPlayed++;
                            opponent.GamesPlayed++;

                            if (winner == user.Username)
                            {
                                user.ELO += 3;
                                opponent.ELO -= 5;
                                user.Coins += 5;
                            }
                            else
                            {
                                user.ELO -= 5;
                                opponent.ELO += 3;
                                opponent.Coins += 5;
                            }

                            db.SaveUser(user);
                            db.SaveUser(opponent);

                            SendResponse(stream, 200, new 
                            { 
                                status = "success",
                                message = "Battle completed!",
                                winner = winner,
                                battleLog = battleLog.ToString(),
                                stats = new 
                                {
                                    rounds = rounds,
                                    userWins = userWins,
                                    opponentWins = opponentWins
                                }
                            });

                            LogMessage($"Battle completed: {winner} wins! ({rounds} rounds)", ConsoleColor.Green);
                        }
                        else
                        {
                            SendResponse(stream, 400, new 
                            { 
                                status = "error",
                                message = "Opponent's deck must contain exactly 4 cards!" 
                            });
                            LogMessage("Battle failed: Invalid opponent deck count", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        SendResponse(stream, 400, new 
                        { 
                            status = "error",
                            message = "Opponent not found!" 
                        });
                        LogMessage("Battle failed: Opponent not found", ConsoleColor.Red);
                    }
                }
                else
                {
                    SendResponse(stream, 400, new 
                    { 
                        status = "error",
                        message = "Your deck must contain exactly 4 cards!" 
                    });
                    LogMessage("Battle failed: Invalid deck count", ConsoleColor.Red);
                }
            }
            else
            {
                SendResponse(stream, 401, new 
                { 
                    status = "error",
                    message = "Invalid token!" 
                });
                LogMessage("Battle failed: Invalid token", ConsoleColor.Red);
            }
        }

        private int CalculateDamage(Card attacker, Card defender)
        {
            if (attacker.IsGoblin && defender.IsDragon)
                return 0;
            
            if (defender.IsWizzard && attacker.IsOrk)
                return 0;
            
            if (defender.IsKnight && attacker.IsWaterSpell)
                return int.MaxValue;
            
            if (defender.IsKraken && attacker.IsSpell)
                return 0;
            
            if (defender.IsDragon && attacker.IsFireElf)
                return 0;

            if (attacker.IsSpell || defender.IsSpell)
            {
                if (attacker.Type == "Water" && defender.Type == "Fire")
                    return attacker.Damage * 2;
                else if (attacker.Type == "Fire" && defender.Type == "Water")
                    return attacker.Damage / 2;
                else if (attacker.Type == "Fire" && defender.Type == "Normal")
                    return attacker.Damage * 2;
                else if (attacker.Type == "Normal" && defender.Type == "Fire")
                    return attacker.Damage / 2;
                else if (attacker.Type == "Normal" && defender.Type == "Water")
                    return attacker.Damage * 2;
                else if (attacker.Type == "Water" && defender.Type == "Normal")
                    return attacker.Damage / 2;
            }

            return attacker.Damage;
        }

        private void HandleTrade(string[] requestLines, NetworkStream stream)
        {
            LogMessage("Trade request received", ConsoleColor.Cyan);

            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null)
            {
                string body = requestLines[requestLines.Length - 1];
                var tradeRequest = JsonConvert.DeserializeObject<TradeRequest>(body);

                if (tradeRequest != null)
                {
                    var cardToTrade = user.Stack.Find(c => c.Name == tradeRequest.CardName);
                    if (cardToTrade == null || user.Deck.Any(c => c.Name == tradeRequest.CardName))
                    {
                        SendResponse(stream, 400, new 
                        { 
                            status = "error",
                            message = "Card not available or is in deck"
                        });
                        return;
                    }

                    user.Stack.Remove(cardToTrade);
                    trades.Add(new Trade 
                    { 
                        CardOffered = cardToTrade,
                        RequirementType = tradeRequest.Requirement,
                        OwnerUsername = user.Username
                    });

                    SendResponse(stream, 200, new 
                    { 
                        status = "success",
                        message = "Trade offer created!",
                        trade = tradeRequest
                    });
                    LogMessage($"Trade created by user: {user.Username}", ConsoleColor.Green);
                }
                else
                {
                    SendResponse(stream, 400, new 
                    { 
                        status = "error",
                        message = "Invalid trade request!" 
                    });
                    LogMessage("Trade failed: Invalid request", ConsoleColor.Red);
                }
            }
            else
            {
                SendResponse(stream, 401, new 
                { 
                    status = "error",
                    message = "Invalid token!" 
                });
                LogMessage("Trade failed: Invalid token", ConsoleColor.Red);
            }
        }

        private void HandleGetStack(string[] requestLines, NetworkStream stream)
        {
            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null)
            {
                var stack = db.GetUserStack(user.Username);
                SendResponse(stream, 200, new 
                { 
                    message = "Stack retrieved successfully!", 
                    stack = stack 
                });
                LogMessage($"Stack retrieved for user: {user.Username}", ConsoleColor.Green);
            }
            else
            {
                SendResponse(stream, 401, new { message = "Invalid token!" });
                LogMessage("Stack retrieval failed: Invalid token", ConsoleColor.Red);
            }
        }

        private void HandleNotFound(NetworkStream stream)
        {
            string response = "HTTP/1.1 404 Not Found\r\nContent-Type: application/json\r\n\r\n" + JsonConvert.SerializeObject(new { message = "Endpoint not found!" });
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
            stream.Flush();
        }

        private void HandleScoreboard(string[] requestLines, NetworkStream stream)
        {
            var sortedUsers = db.GetAllUsers().OrderByDescending(u => u.ELO).ToList();
            SendResponse(stream, 200, new 
            { 
                status = "success",
                message = "Scoreboard retrieved successfully!",
                scoreboard = sortedUsers.Select(u => new { u.Username, u.ELO, u.GamesPlayed })
            });
            LogMessage("Scoreboard retrieved", ConsoleColor.Green);
        }

        private void HandleProfile(string[] requestLines, NetworkStream stream)
        {
            string? token = GetTokenFromHeaders(requestLines);
            var user = db.GetUserByToken(token ?? "");

            if (user != null)
            {
                SendResponse(stream, 200, new 
                { 
                    status = "success",
                    message = "Profile retrieved successfully!",
                    profile = new { user.Username, user.ELO, user.GamesPlayed, user.Coins }
                });
                LogMessage($"Profile retrieved for user: {user.Username}", ConsoleColor.Green);
            }
            else
            {
                SendResponse(stream, 401, new 
                { 
                    status = "error",
                    message = "Invalid token!" 
                });
                LogMessage("Profile retrieval failed: Invalid token", ConsoleColor.Red);
            }
        }

        private string? GetTokenFromHeaders(string[] requestLines)
        {
            foreach (var line in requestLines)
            {
                if (line.StartsWith("Authorization: Bearer "))
                {
                    return line.Substring("Authorization: Bearer ".Length);
                }
            }
            return null;
        }

        private List<Card> GeneratePackage()
        {
            var package = new List<Card>();
            for (int i = 0; i < 5; i++)
            {
                package.Add(new Card("Card" + i, "Type" + i, i * 10, i % 2 == 0, i * 10));
            }
            return package;
        }
    }
}