namespace MCTG
{
    public class Card
    {
        public int Id { get; set; } 
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

        public bool IsGoblin => Name.Contains("Goblin", StringComparison.OrdinalIgnoreCase);
        public bool IsDragon => Name.Contains("Dragon", StringComparison.OrdinalIgnoreCase);
        public bool IsWizzard => Name.Contains("Wizzard", StringComparison.OrdinalIgnoreCase);
        public bool IsOrk => Name.Contains("Ork", StringComparison.OrdinalIgnoreCase);
        public bool IsKnight => Name.Contains("Knight", StringComparison.OrdinalIgnoreCase);
        public bool IsKraken => Name.Contains("Kraken", StringComparison.OrdinalIgnoreCase);
        public bool IsFireElf => Name.Contains("FireElf", StringComparison.OrdinalIgnoreCase);
        public bool IsWaterSpell => IsSpell && Type.Equals("Water", StringComparison.OrdinalIgnoreCase);
    }
}