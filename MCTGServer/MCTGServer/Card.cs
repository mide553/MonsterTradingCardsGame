public class Card
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int Power { get; set; }

    public Card(string name, string type, int power)
    {
        Name = name;
        Type = type;
        Power = power;
    }
}
