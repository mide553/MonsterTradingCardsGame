namespace MCTG
{
    public class Trade
    {
        public int Id { get; set; }
        public Card CardOffered { get; set; } = null!;
        public string RequirementType { get; set; } = string.Empty;
        public int MinimumDamage { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
    }
}
