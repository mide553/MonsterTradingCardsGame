namespace MCTG
{
    public class Card
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Power { get; set; }
        public bool IsSpell { get; set; }
        public int Damage { get; set; }

        public Card(string name, string type, int power, bool isSpell, int damage)
        {
            Name = name;
            Type = type;
            Power = power;
            IsSpell = isSpell;
            Damage = damage;
        }
    }
}