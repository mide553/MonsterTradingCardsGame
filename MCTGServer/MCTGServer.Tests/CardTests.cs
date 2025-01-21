using NUnit.Framework;

namespace MCTG.Tests
{
    [TestFixture]
    public class CardTests
    {
        [Test]
        public void TestCardProperties()
        {
            var card = new Card("testcard", "type", 10, false, 10);
            Assert.AreEqual("testcard", card.Name);
            Assert.AreEqual("type", card.Type);
            Assert.AreEqual(10, card.Power);
            Assert.AreEqual(false, card.IsSpell);
            Assert.AreEqual(10, card.Damage);
        }

        [Test]
        public void TestIsGoblin()
        {
            var card = new Card("Goblin Warrior", "type", 10, false, 10);
            Assert.IsTrue(card.IsGoblin);
        }

        [Test]
        public void TestIsDragon()
        {
            var card = new Card("Fire Dragon", "type", 10, false, 10);
            Assert.IsTrue(card.IsDragon);
        }

        [Test]
        public void TestIsWizzard()
        {
            var card = new Card("Wizzard Mage", "type", 10, false, 10);
            Assert.IsTrue(card.IsWizzard);
        }

        [Test]
        public void TestIsOrk()
        {
            var card = new Card("Ork Warrior", "type", 10, false, 10);
            Assert.IsTrue(card.IsOrk);
        }

        [Test]
        public void TestIsKnight()
        {
            var card = new Card("Knight Rider", "type", 10, false, 10);
            Assert.IsTrue(card.IsKnight);
        }

        [Test]
        public void TestIsKraken()
        {
            var card = new Card("Sea Kraken", "type", 10, false, 10);
            Assert.IsTrue(card.IsKraken);
        }

        [Test]
        public void TestIsFireElf()
        {
            var card = new Card("FireElf Archer", "type", 10, false, 10);
            Assert.IsTrue(card.IsFireElf);
        }

        [Test]
        public void TestIsWaterSpell()
        {
            var card = new Card("Water Spell", "Water", 10, true, 10);
            Assert.IsTrue(card.IsWaterSpell);
        }
    }
}
