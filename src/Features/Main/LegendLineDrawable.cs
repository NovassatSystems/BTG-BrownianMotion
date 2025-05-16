using BTGBrownianMotion.Models;

namespace BTGBrownianMotion.Features.Main;

public class LegendLineDrawable : IDrawable
{
    public Color Color { get; set; }
    public LineStyle Style { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Color;
        canvas.StrokeSize = Style switch
        {
            LineStyle.Fina => 1,
            LineStyle.Espessa => 4,
            _ => 2
        };

        canvas.StrokeDashPattern = Style switch
        {
            LineStyle.Tracejada => new float[] { 6, 4 },
            LineStyle.Pontilhada => new float[] { 2, 2 },
            LineStyle.TracoLongo => new float[] { 10, 4 },
            _ => null
        };

        canvas.StrokeLineCap = Style == LineStyle.Arredondada ? LineCap.Round : LineCap.Butt;
        canvas.StrokeLineJoin = Style == LineStyle.Arredondada ? LineJoin.Round : LineJoin.Miter;

        float y = dirtyRect.Height / 2;
        canvas.DrawLine(0, y, dirtyRect.Width, y);
    }
}
