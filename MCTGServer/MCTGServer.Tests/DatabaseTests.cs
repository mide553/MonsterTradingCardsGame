using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private Database db;

        [SetUp]
        public void Setup()
        {
            db = new Database();
        }

        [Test]
        public void TestSaveUser()
        {
            var user = new User("testuser", "password");
            db.SaveUser(user);
            var retrievedUser = db.GetUserByUsername("testuser");
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual("testuser", retrievedUser.Username);
        }

        [Test]
        public void TestGetUserByToken()
        {
            var user = new User("testuser", "password") { Token = "test_token" };
            db.SaveUser(user);
            var retrievedUser = db.GetUserByToken("test_token");
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual("testuser", retrievedUser.Username);
        }

        [Test]
        public void TestGetUserByUsername()
        {
            var user = new User("testuser", "password");
            db.SaveUser(user);
            var retrievedUser = db.GetUserByUsername("testuser");
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual("testuser", retrievedUser.Username);
        }

        [Test]
        public void TestGetAllUsers()
        {
            var user1 = new User("testuser1", "password");
            var user2 = new User("testuser2", "password");
            db.SaveUser(user1);
            db.SaveUser(user2);
            var users = db.GetAllUsers();
            Assert.IsTrue(users.Count >= 2);
        }

        [Test]
        public void TestSaveCard()
        {
            var user = new User("testuser", "password");
            db.SaveUser(user);
            var card = new Card("testcard", "type", 10, false, 10);
            db.SaveCard(card, "testuser");
            var stack = db.GetUserStack("testuser");
            Assert.IsTrue(stack.Exists(c => c.Name == "testcard"));
        }

        [Test]
        public void TestGetUserStack()
        {
            var user = new User("testuser", "password");
            db.SaveUser(user);
            var card = new Card("testcard", "type", 10, false, 10);
            db.SaveCard(card, "testuser");
            var stack = db.GetUserStack("testuser");
            Assert.IsTrue(stack.Exists(c => c.Name == "testcard"));
        }

        [Test]
        public void TestUpdateUserStack()
        {
            var user = new User("testuser", "password");
            db.SaveUser(user);
            var card = new Card("testcard", "type", 10, false, 10);
            db.SaveCard(card, "testuser");
            var stack = new List<Card> { card };
            db.UpdateUserStack("testuser", stack);
            var updatedStack = db.GetUserStack("testuser");
            Assert.AreEqual(1, updatedStack.Count);
        }

        [Test]
        public void TestSaveUserDeck()
        {
            var user = new User("testuser", "password");
            db.SaveUser(user);
            var card = new Card("testcard", "type", 10, false, 10);
            db.SaveCard(card, "testuser");
            var deck = new List<Card> { card };
            db.SaveUserDeck("testuser", deck);
            var retrievedDeck = db.GetUserDeck("testuser");
            Assert.AreEqual(1, retrievedDeck.Count);
        }

        [Test]
        public void TestGetUserDeck()
        {
            var user = new User("testuser", "password");
            db.SaveUser(user);
            var card = new Card("testcard", "type", 10, false, 10);
            db.SaveCard(card, "testuser");
            var deck = new List<Card> { card };
            db.SaveUserDeck("testuser", deck);
            var retrievedDeck = db.GetUserDeck("testuser");
            Assert.AreEqual(1, retrievedDeck.Count);
        }
    }
}
