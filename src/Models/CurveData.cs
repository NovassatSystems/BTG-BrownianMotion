namespace BTGBrownianMotion.Models;

public class CurveData
{
    public List<double> Points { get; set; } = new();
    public Color Color { get; set; } = Colors.MediumPurple;
    public LineStyle Style { get; set; } = LineStyle.Solida;
}
