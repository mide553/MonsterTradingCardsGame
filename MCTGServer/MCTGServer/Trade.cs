namespace MCTG
{
    public class Trade
    {
        public Card CardOffered { get; set; } = null!;
        public string RequirementType { get; set; } = string.Empty;
        public string OwnerUsername { get; set; } = string.Empty;
    }
}
