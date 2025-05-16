using BTGBrownianMotion.Features.Main;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BTGBrownianMotion.Models;

public partial class LegendItem : ObservableObject
{
    public Color Color { get; set; }
    public LineStyle Style { get; set; }
    public string Label { get; set; }

    [ObservableProperty] public bool isSelected;

    public LegendLineDrawable Drawable => new()
    {
        Color = this.Color,
        Style = this.Style
    };
}
