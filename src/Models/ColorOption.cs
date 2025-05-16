namespace BTGBrownianMotion.Models;

public class ColorOption
{
    public string Nome { get; set; }
    public Color Cor { get; set; }

    public override string ToString() => Nome; 
}
