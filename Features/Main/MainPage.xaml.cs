using BTGBrownianMotion.Models;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;
using System.Collections.Specialized;

namespace BTGBrownianMotion.Features.Main;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel => BindingContext as MainViewModel;
    private PriceDrawable _drawable;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        AddEvents(viewModel);
    }

    void AddEvents(MainViewModel viewModel)
    {
        _drawable = new PriceDrawable();
        GraphView.Drawable = _drawable;

        GraphView.HandlerChanged += (s, e) =>
        {
            if (GraphView.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement platformView)
            {
                platformView.PointerMoved += (sender, args) =>
                {
                    var pos = args.GetCurrentPoint(platformView).Position;
                    _drawable.PointerPosition = new PointF((float)pos.X, (float)pos.Y);
                    GraphView.Invalidate();
                };
            }
        };


        viewModel.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(viewModel.VerticalLineCount):
                    _drawable.VerticalLineCount = viewModel.VerticalLineCount;
                    GraphView.Invalidate();
                    break;

                case nameof(viewModel.HorizontalLineCount):
                    _drawable.HorizontalLineCount = viewModel.HorizontalLineCount;
                    GraphView.Invalidate();
                    break;

                case nameof(viewModel.Prices):
                    _drawable.Curves.Clear();
                    _drawable.ProgressByCurve.Clear();

                    var curva = viewModel.Curves.FirstOrDefault();
                    if (curva is not null)
                    {
                        _drawable.Curves.Add(curva);
                        _drawable.ProgressByCurve.Add(0f);
                        AnimateCurve(0);
                    }
                    break;

                case nameof(viewModel.Curves):
                    if (viewModel.Curves.Count == 0)
                    {
                        _drawable.Curves.Clear();
                        _drawable.ProgressByCurve.Clear();
                        GraphView.Invalidate();
                    }
                    break;

                case nameof(viewModel.BackgroundColorOption):
                    _drawable.BackgroundOption = viewModel.BackgroundColorOption;
                    GraphView.Invalidate();
                    break;
                case nameof(viewModel.SelectedLegendItem):
                    _drawable.SelectedCurve = viewModel.SelectedLegendItem;
                    GraphView.Invalidate();
                    break;
            }
        };



        viewModel.Curves.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var curva = viewModel.Curves.LastOrDefault();
                if (curva is not null)
                {
                    _drawable.Curves.Add(curva);
                    _drawable.ProgressByCurve.Add(0f);

                    int index = _drawable.Curves.Count - 1;
                    AnimateCurve(index);
                }
            }
        };

        viewModel.GenerateSimulationCommand.Execute(null);
    }

    private void AnimateCurve(int index)
    {
        if (index < 0)
            return;

       
        while (_drawable.ProgressByCurve.Count <= index)
            _drawable.ProgressByCurve.Add(0f);

        var duration = TimeSpan.FromSeconds(3);
        var start = DateTime.UtcNow;
        var frameRate = 60;
        var interval = TimeSpan.FromMilliseconds(1000.0 / frameRate);

        var timer = Dispatcher.CreateTimer();
        timer.Interval = interval;

        
        int capturedIndex = index;

        timer.Tick += (_, _) =>
        {
            
            if (capturedIndex >= _drawable.ProgressByCurve.Count)
            {
                timer.Stop();
                return;
            }

            float t = (float)((DateTime.UtcNow - start).TotalMilliseconds / duration.TotalMilliseconds);

            if (t >= 1f)
            {
                _drawable.ProgressByCurve[capturedIndex] = 1f;
                GraphView.Invalidate();
                timer.Stop();
                return;
            }

            _drawable.ProgressByCurve[capturedIndex] = t;
            GraphView.Invalidate();
        };

        timer.Start();
    }

    private void ColorPicker_PickedColorChanged(object sender, Maui.ColorPicker.PickedColorChangedEventArgs e)
    {
        _viewModel.SelectedColor = e.NewPickedColorValue;
    }

    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton radio && radio.BindingContext is LegendItem item)
        {
            _viewModel.SelectedLegendItem = item;
        }
    }

    private async void OnExportarGraficoClicked(object sender, EventArgs e)
    {
        var tamanho = new Size(GraphView.Width, GraphView.Height); 
        var caminho = Path.Combine(FileSystem.CacheDirectory, $"grafico_{DateTime.Now.Ticks}.png");

        await ExportarGraficoComoImagemAsync(_drawable, tamanho, caminho);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Gráfico Exportado",
            File = new ShareFile(caminho)
        });
    }

    public async Task ExportarGraficoComoImagemAsync(IDrawable drawable, Size size, string filePath)
    {
        float width = (float)size.Width;
        float height = (float)size.Height;

        using var bitmap = new SKBitmap((int)width, (int)height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        
        var skiaCanvas = new SkiaCanvas();
        skiaCanvas.Canvas = canvas;

        drawable.Draw(skiaCanvas, new Microsoft.Maui.Graphics.RectF(0, 0, width, height));

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }


}
