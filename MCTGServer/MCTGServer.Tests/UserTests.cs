using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void TestUserProperties()
        {
            var user = new User("testuser", "password");
            Assert.AreEqual("testuser", user.Username);
            Assert.AreEqual("password", user.Password);
            Assert.AreEqual(20, user.Coins);
            Assert.AreEqual(100, user.ELO);
            Assert.AreEqual(0, user.GamesPlayed);
        }

        [Test]
        public void TestUserStack()
        {
            var user = new User("testuser", "password");
            var card = new Card("testcard", "type", 10, false, 10);
            user.Stack.Add(card);
            Assert.AreEqual(1, user.Stack.Count);
            Assert.AreEqual("testcard", user.Stack[0].Name);
        }

        [Test]
        public void TestUserDeck()
        {
            var user = new User("testuser", "password");
            var card = new Card("testcard", "type", 10, false, 10);
            user.Deck.Add(card);
            Assert.AreEqual(1, user.Deck.Count);
            Assert.AreEqual("testcard", user.Deck[0].Name);
        }
    }
}
