using BTGBrownianMotion.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace BTGBrownianMotion.Features.Main;

public partial class MainViewModel : ObservableObject
{
    #region Properties
    [ObservableProperty] public double returnValue = 1;
    [ObservableProperty] public double volatility = 20;
    [ObservableProperty] public double initialPrice = 100;

    [ObservableProperty] public int days = 50;
    [ObservableProperty] public int verticalLineCount = 5;
    [ObservableProperty] public int horizontalLineCount = 5;

    [ObservableProperty] public LegendItem selectedLegendItem;
    [ObservableProperty] public Color selectedColor = Color.FromRgba("#195AB4");
    [ObservableProperty] public LineStyle selectedLineStyle = LineStyle.Solida;
    [ObservableProperty] public BackgroundColorOption backgroundColorOption = BackgroundColorOption.Grafite;

    public ObservableCollection<double> Prices { get; } = [];
    public ObservableCollection<CurveData> Curves { get; } = [];
    public ObservableCollection<LegendItem> LegendItems { get; } = [];
    public ObservableCollection<List<double>> Simulations { get; } = [];

    public Array LineStyleOptions => Enum.GetValues(typeof(LineStyle));
    public Array BackgroundColorOptions => Enum.GetValues(typeof(BackgroundColorOption));
    #endregion

    #region Partials
    partial void OnSelectedLegendItemChanged(LegendItem value)
    {
        foreach (var item in LegendItems)
            item.IsSelected = item == value;

        OnPropertyChanged(nameof(SelectedLegendItem));
    }
    #endregion

    #region Commands
    [RelayCommand]
    public void GenerateSimulation()
    {
        Prices.Clear();
        var nova = GenerateInternalSimulation();
        foreach (var v in nova)
            Prices.Add(v);

        Curves.Clear();
        LegendItems.Clear();
        LegendItems.Add(new LegendItem
        {
            Color = SelectedColor,
            Style = SelectedLineStyle,
            Label = "Linha 1"
        });
        Curves.Add(new CurveData
        {
            Points = nova,
            Color = SelectedColor,
            Style = SelectedLineStyle
        });

        SelectedLegendItem = LegendItems.FirstOrDefault();

        OnPropertyChanged(nameof(Prices));
        OnPropertyChanged(nameof(Curves));
        OnPropertyChanged(nameof(LegendItems));
    }

    [RelayCommand]
    public void AddSimulation()
    {
        var nova = GenerateInternalSimulation();
        Simulations.Add(nova);

        Curves.Add(new CurveData
        {
            Points = nova,
            Color = SelectedColor,
            Style = SelectedLineStyle
        });

        LegendItems.Add(new LegendItem
        {
            Color = SelectedColor,
            Style = SelectedLineStyle,
            Label = $"Linha {LegendItems.Count + 1}"
        });

        OnPropertyChanged(nameof(Simulations));
        OnPropertyChanged(nameof(Curves));
        OnPropertyChanged(nameof(LegendItems));
    }

    [RelayCommand]
    public void ClearSimulations()
    {
        Prices.Clear();
        Simulations.Clear();
        Curves.Clear();
        LegendItems.Clear();

        OnPropertyChanged(nameof(Prices));
        OnPropertyChanged(nameof(Simulations));
        OnPropertyChanged(nameof(Curves));
        OnPropertyChanged(nameof(LegendItems));
    }

    [RelayCommand]
    private void SelectCurve(LegendItem item)
    {
        SelectedLegendItem = item;
    }
    #endregion

    #region Aux
    public List<double> GenerateInternalSimulation()
    {
        double price = InitialPrice;
        double sigma = Volatility / 100;
        double mu = ReturnValue / 100;
        int simulatedDays = Days > 0 ? Days : 252;

        double deltaT = 1.0 / simulatedDays;
        var simulation = new List<double> { price };

        var rng = new Random();

        for (int i = 1; i < simulatedDays; i++)
        {
            double z = NormalSample(rng);
            double variation = (mu - 0.5 * Math.Pow(sigma, 2)) * deltaT + sigma * Math.Sqrt(deltaT) * z;
            double newValue = simulation[i - 1] * Math.Exp(variation);
            simulation.Add(newValue);
        }

        return simulation;
    }

    private double NormalSample(Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
    #endregion
}
