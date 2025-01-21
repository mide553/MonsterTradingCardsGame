using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class ServerTests
    {
        private Server server;
        private NetworkStreamWrapper stream;

        [SetUp]
        public void Setup()
        {
            server = new Server();
            stream = new NetworkStreamWrapper();

            var user = new User("testuser", "password") { Token = "valid_token", Coins = 10 };
            server.db.SaveUser(user);
        }

        [TearDown]
        public void TearDown()
        {
            stream.Dispose();
        }

        [Test]
        public void TestHandleRegister()
        {
            var requestLines = new string[]
            {
                "POST /register HTTP/1.1",
                "",
                JsonConvert.SerializeObject(new User("testuser", "password"))
            };

            server.HandleRegister(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Assert.IsTrue(response.Contains("User registered successfully!"));
        }

        [Test]
        public void TestHandleLogin()
        {
            var requestLines = new string[]
            {
                "POST /login HTTP/1.1",
                "",
                JsonConvert.SerializeObject(new User("testuser", "password"))
            };

            server.HandleLogin(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Assert.IsTrue(response.Contains("Login successful!"));
        }

        [Test]
        public void TestHandleAddCard()
        {
            var requestLines = new string[]
            {
                "POST /cards HTTP/1.1",
                "Authorization: Bearer valid_token",
                "",
                JsonConvert.SerializeObject(new Card("testcard", "type", 10, false, 10))
            };

            server.HandleAddCard(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Console.WriteLine(response);
            Assert.IsTrue(response.Contains("Card added successfully!"), $"Expected response to contain 'Card added successfully!' but got: {response}");
        }

        [Test]
        public void TestHandleGetCards()
        {
            var requestLines = new string[]
            {
                "GET /cards HTTP/1.1",
                "Authorization: Bearer valid_token",
                ""
            };

            server.HandleGetCards(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Console.WriteLine(response);
            Assert.IsTrue(response.Contains("Cards retrieved successfully!"), $"Expected response to contain 'Cards retrieved successfully!' but got: {response}");
        }

        [Test]
        public void TestHandleAcquirePackage()
        {
            var requestLines = new string[]
            {
                "POST /acquire HTTP/1.1",
                "Authorization: Bearer valid_token",
                ""
            };

            server.HandleAcquirePackage(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Console.WriteLine(response);
            Assert.IsTrue(response.Contains("Package acquired successfully!"), $"Expected response to contain 'Package acquired successfully!' but got: {response}");
        }

        [Test]
        public void TestHandleManageDeck()
        {
            var requestLines = new string[]
            {
                "POST /deck HTTP/1.1",
                "Authorization: Bearer valid_token",
                "",
                JsonConvert.SerializeObject(new List<Card>
                {
                    new Card("Card1", "Type1", 10, false, 10),
                    new Card("Card2", "Type2", 20, false, 20),
                    new Card("Card3", "Type3", 30, false, 30),
                    new Card("Card4", "Type4", 40, false, 40)
                })
            };

            var user = server.db.GetUserByToken("valid_token");
            user.Stack.Clear();
            user.Stack.AddRange(new List<Card>
            {
                new Card("Card1", "Type1", 10, false, 10),
                new Card("Card2", "Type2", 20, false, 20),
                new Card("Card3", "Type3", 30, false, 30),
                new Card("Card4", "Type4", 40, false, 40)
            });
            server.db.UpdateUserStack(user.Username, user.Stack);

            server.HandleManageDeck(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Console.WriteLine(response);
            Assert.IsTrue(response.Contains("Deck updated successfully!"), $"Expected response to contain 'Deck updated successfully!' but got: {response}");
        }

        [Test]
        public void TestHandleBattle()
        {
            // Register users
            var registerRequest1 = new string[]
            {
                "POST /register HTTP/1.1",
                "",
                JsonConvert.SerializeObject(new User("armin", "armin"))
            };
            server.HandleRegister(registerRequest1, stream);

            var registerRequest2 = new string[]
            {
                "POST /register HTTP/1.1",
                "",
                JsonConvert.SerializeObject(new User("bob", "bob"))
            };
            server.HandleRegister(registerRequest2, stream);

            var loginRequest1 = new string[]
            {
                "POST /login HTTP/1.1",
                "",
                JsonConvert.SerializeObject(new User("armin", "armin"))
            };
            server.HandleLogin(loginRequest1, stream);
            var token1 = server.db.GetUserByUsername("armin").Token;

            var loginRequest2 = new string[]
            {
                "POST /login HTTP/1.1",
                "",
                JsonConvert.SerializeObject(new User("bob", "bob"))
            };
            server.HandleLogin(loginRequest2, stream);
            var token2 = server.db.GetUserByUsername("bob").Token;

            var addCardRequest1 = new string[]
            {
                "POST /cards HTTP/1.1",
                $"Authorization: Bearer {token1}",
                "",
                JsonConvert.SerializeObject(new Card("WaterSpell", "Water", 20, true, 50))
            };
            server.HandleAddCard(addCardRequest1, stream);

            var addCardRequest2 = new string[]
            {
                "POST /cards HTTP/1.1",
                $"Authorization: Bearer {token1}",
                "",
                JsonConvert.SerializeObject(new Card("Knight", "Normal", 30, false, 35))
            };
            server.HandleAddCard(addCardRequest2, stream);

            var addCardRequest3 = new string[]
            {
                "POST /cards HTTP/1.1",
                $"Authorization: Bearer {token2}",
                "",
                JsonConvert.SerializeObject(new Card("Dragon", "Monster", 25, false, 70))
            };
            server.HandleAddCard(addCardRequest3, stream);

            var addCardRequest4 = new string[]
            {
                "POST /cards HTTP/1.1",
                $"Authorization: Bearer {token2}",
                "",
                JsonConvert.SerializeObject(new Card("Goblin", "Normal", 15, false, 25))
            };
            server.HandleAddCard(addCardRequest4, stream);

            var acquirePackageRequest1 = new string[]
            {
                "POST /acquire HTTP/1.1",
                $"Authorization: Bearer {token1}",
                ""
            };
            server.HandleAcquirePackage(acquirePackageRequest1, stream);

            var acquirePackageRequest2 = new string[]
            {
                "POST /acquire HTTP/1.1",
                $"Authorization: Bearer {token2}",
                ""
            };
            server.HandleAcquirePackage(acquirePackageRequest2, stream);

            var configureDeckRequest1 = new string[]
            {
                "POST /deck HTTP/1.1",
                $"Authorization: Bearer {token1}",
                "",
                JsonConvert.SerializeObject(new List<Card>
                {
                    new Card("WaterSpell", "Water", 20, true, 50),
                    new Card("Knight", "Normal", 30, false, 35),
                    new Card("Card1", "Type1", 10, false, 10),
                    new Card("Card2", "Type2", 20, true, 20)
                })
            };
            server.HandleManageDeck(configureDeckRequest1, stream);

            var configureDeckRequest2 = new string[]
            {
                "POST /deck HTTP/1.1",
                $"Authorization: Bearer {token2}",
                "",
                JsonConvert.SerializeObject(new List<Card>
                {
                    new Card("Dragon", "Monster", 25, false, 70),
                    new Card("Goblin", "Normal", 15, false, 25),
                    new Card("Card1", "Type1", 10, false, 10),
                    new Card("Card2", "Type2", 20, true, 20)
                })
            };
            server.HandleManageDeck(configureDeckRequest2, stream);

            var battleRequest = new string[]
            {
                "POST /battle HTTP/1.1",
                $"Authorization: Bearer {token1}",
                "",
                JsonConvert.SerializeObject("bob")
            };
            server.HandleBattle(battleRequest, stream);

            var response = Encoding.UTF8.GetString(stream.ToArray());
            Console.WriteLine(response);
            Assert.IsTrue(response.Contains("Battle completed!"), $"Expected response to contain 'Battle completed!' but got: {response}");
        }


        [Test]
        public void TestHandleGetStack()
        {
            var requestLines = new string[]
            {
                "GET /stack HTTP/1.1",
                "Authorization: Bearer valid_token",
                ""
            };

            server.HandleGetStack(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Console.WriteLine(response);
            Assert.IsTrue(response.Contains("Stack retrieved successfully!"), $"Expected response to contain 'Stack retrieved successfully!' but got: {response}");
        }

        [Test]
        public void TestHandleScoreboard()
        {
            var requestLines = new string[]
            {
                "GET /scoreboard HTTP/1.1",
                ""
            };

            server.HandleScoreboard(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Assert.IsTrue(response.Contains("Scoreboard retrieved successfully!"));
        }

        [Test]
        public void TestHandleProfile()
        {
            var requestLines = new string[]
            {
                "GET /profile HTTP/1.1",
                "Authorization: Bearer valid_token",
                ""
            };

            server.HandleProfile(requestLines, stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Console.WriteLine(response);
            Assert.IsTrue(response.Contains("Profile retrieved successfully!"), $"Expected response to contain 'Profile retrieved successfully!' but got: {response}");
        }

        [Test]
        public void TestHandleNotFound()
        {
            var requestLines = new string[]
            {
                "GET /unknown HTTP/1.1",
                ""
            };

            server.HandleNotFound(stream);
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Assert.IsTrue(response.Contains("Endpoint not found!"));
        }

        [Test]
        public void TestLogMessage()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                server.LogMessage("Test message", ConsoleColor.Green);
                var result = sw.ToString().Trim();
                Assert.IsTrue(result.Contains("Test message"));
            }
        }

        [Test]
        public void TestSendResponse()
        {
            server.SendResponse(stream, 200, new { message = "Test response" });
            var response = Encoding.UTF8.GetString(stream.ToArray());
            Assert.IsTrue(response.Contains("Test response"));
        }

        [Test]
        public void TestGetTokenFromHeaders()
        {
            var requestLines = new string[]
            {
                "GET / HTTP/1.1",
                "Authorization: Bearer test_token",
                ""
            };

            var token = server.GetTokenFromHeaders(requestLines);
            Assert.AreEqual("test_token", token);
        }

        [Test]
        public void TestGeneratePackage()
        {
            var package = server.GeneratePackage();
            Assert.AreEqual(5, package.Count);
        }

        [Test]
        public void TestCalculateDamage()
        {
            var attacker = new Card("attacker", "Fire", 10, true, 10);
            var defender = new Card("defender", "Water", 10, true, 10);
            var damage = server.CalculateDamage(attacker, defender);
            Assert.AreEqual(5, damage);
        }

        [Test]
        public void TestCalculateDamageSpecialCases()
        {
            var attacker = new Card("Goblin", "Normal", 10, false, 10);
            var defender = new Card("Dragon", "Fire", 10, false, 10);
            var damage = server.CalculateDamage(attacker, defender);
            Assert.AreEqual(0, damage);
        }

        [Test]
        public void TestHandleMultipleRequests()
        {
            int numberOfThreads = 10;
            var threads = new List<Thread>();
            var responses = new List<string>();

            for (int i = 0; i < numberOfThreads; i++)
            {
                var username = $"user{i}";
                var registerRequest = new string[]
                {
                    "POST /register HTTP/1.1",
                    "",
                    JsonConvert.SerializeObject(new User(username, "password"))
                };
                server.HandleRegister(registerRequest, stream);
            }

            var tokens = new List<string>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                var username = $"user{i}";
                var loginRequest = new string[]
                {
                    "POST /login HTTP/1.1",
                    "",
                    JsonConvert.SerializeObject(new User(username, "password"))
                };
                server.HandleLogin(loginRequest, stream);
                var token = server.db.GetUserByUsername(username).Token;
                tokens.Add(token);
            }

            for (int i = 0; i < numberOfThreads; i++)
            {
                var token = tokens[i];
                var thread = new Thread(() =>
                {
                    var requestLines = new string[]
                    {
                        "GET /profile HTTP/1.1",
                        $"Authorization: Bearer {token}",
                        ""
                    };

                    server.HandleProfile(requestLines, stream);
                    var response = Encoding.UTF8.GetString(stream.ToArray());
                    lock (responses)
                    {
                        responses.Add(response);
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            foreach (var response in responses)
            {
                Console.WriteLine(response);
                Assert.IsTrue(response.Contains("Profile retrieved successfully!"), $"Expected response to contain 'Profile retrieved successfully!' but got: {response}");
            }
        }
    }
}
