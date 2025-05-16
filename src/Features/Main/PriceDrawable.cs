using BTGBrownianMotion.Models;
using System.Diagnostics;

namespace BTGBrownianMotion.Features.Main;

/// <summary>
/// Componente gráfico que implementa <see cref="IDrawable"/> para renderizar simulações de movimento Browniano.
/// Responsável por desenhar o fundo, a grade, as curvas e a interação com o ponteiro.
/// </summary>
public class PriceDrawable : IDrawable
{
    private record GraphMetrics(RectF GraphRect, double Min, double Max);

    /// <summary>
    /// Define a quantidade de linhas verticais (divisões no eixo X) na grade do gráfico.
    /// </summary>
    public int VerticalLineCount { get; set; } = 5;

    /// <summary>
    /// Define a quantidade de linhas horizontais (divisões no eixo Y) na grade do gráfico.
    /// </summary>
    public int HorizontalLineCount { get; set; } = 5;

    /// <summary>
    /// Posição atual do ponteiro, usada para exibir informações da curva selecionada.
    /// </summary>
    public PointF? PointerPosition { get; set; }

    /// <summary>
    /// Curva atualmente selecionada para exibir detalhes ao interagir com o gráfico.
    /// </summary>
    public LegendItem? SelectedCurve { get; set; }

    /// <summary>
    /// Lista de curvas a serem desenhadas no gráfico.
    /// </summary>
    public List<CurveData> Curves { get; set; } = [];

    /// <summary>
    /// Lista de progresso por curva, permitindo animações ou renderização parcial.
    /// </summary>
    public List<float> ProgressByCurve { get; set; } = [];

    /// <summary>
    /// Define a cor de fundo do gráfico, com base em uma enum de opções visuais.
    /// </summary>
    public BackgroundColorOption BackgroundOption { get; set; } = BackgroundColorOption.Grafite;


    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Curves.Count == 0 || dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
            return;

        // Encontrar mínimo e máximo globais
        var metrics = CalculateGraphMetrics(dirtyRect);
        if (metrics is null)
            return;

        DrawBackground(canvas, dirtyRect);

        // Grade
        ConfigureCanvasStyle(canvas);

        // Linhas horizontais
        DrawHorizontalGridLines(canvas, metrics.GraphRect, metrics.Min, metrics.Max);

        // Linhas verticais
        DrawVerticalGridLines(canvas, metrics.GraphRect);

        // Desenhar curvas
        DrawCurves(canvas, metrics.GraphRect, metrics.Min, metrics.Max);

        // Tooltip
        DrawPointerInfo(canvas, metrics.GraphRect, dirtyRect, metrics.Min, metrics.Max);
    }

    /// <summary>
    /// Calcula os valores mínimo e máximo globais de todas as curvas e define a área interna útil para o gráfico.
    /// </summary>
    /// <param name="dirtyRect">O retângulo total de desenho disponível.</param>
    /// <returns>Um objeto <see cref="GraphMetrics"/> com a área do gráfico, o valor mínimo e o valor máximo global, ou <c>null</c> se todos os valores forem iguais.</returns>
    /// <remarks>
    /// Esse método é fundamental para normalizar os pontos na escala vertical,
    /// garantindo que todas as curvas sejam proporcionalmente desenhadas.
    /// </remarks>
    GraphMetrics? CalculateGraphMetrics(RectF dirtyRect)
    {
        var allPoints = Curves.SelectMany(c => c.Points);
        double max = allPoints.Max();
        double min = allPoints.Min();

        if (max == min)
            return null;

        float padding = 48f;
        var graphRect = new RectF(
            x: padding,
            y: padding,
            width: dirtyRect.Width - 2 * padding,
            height: dirtyRect.Height - 2 * padding
        );

        return new GraphMetrics(graphRect, min, max);
    }

    /// <summary>
    /// Preenche o fundo do gráfico com a cor definida pela propriedade <see cref="BackgroundOption"/>.
    /// </summary>
    /// <param name="canvas">O canvas onde o fundo será desenhado.</param>
    /// <param name="rect">A área total da tela onde o fundo será aplicado.</param>
    /// <remarks>
    /// Essa etapa é feita antes de qualquer outro elemento visual do gráfico.
    /// A cor é escolhida com base em uma enum definida pelo usuário.
    /// </remarks>
    void DrawBackground(ICanvas canvas, RectF rect)
    {
        canvas.FillColor = BackgroundOption switch
        {
            BackgroundColorOption.Cinza => Colors.Gray,
            BackgroundColorOption.Preto => Colors.Black,
            BackgroundColorOption.AzulClaro => Color.FromArgb("#195AB4"),
            BackgroundColorOption.AzulEscuro => Color.FromArgb("#001E62"),
            BackgroundColorOption.Grafite => Color.FromArgb("#1C233B"),
            BackgroundColorOption.Transparente => Colors.Transparent,
            BackgroundColorOption.Branco => Colors.White,
            _ => Color.FromArgb("#001E62")
        };

        canvas.FillRoundedRectangle(rect,16);
    }

    /// <summary>
    /// Define as cores e estilos de fonte e traço para os elementos da grade do gráfico.
    /// </summary>
    /// <param name="canvas">O canvas onde os estilos serão aplicados.</param>
    /// <remarks>
    /// Isso garante consistência visual para as linhas da grade e os rótulos numéricos.
    /// </remarks>
    void ConfigureCanvasStyle(ICanvas canvas)
    {
        canvas.StrokeColor = Colors.DimGray;
        canvas.StrokeSize = 1;
        canvas.FontColor = BackgroundOption != BackgroundColorOption.Branco ?  Colors.White : Colors.Black;
        canvas.FontSize = 12;
        canvas.Font = new Microsoft.Maui.Graphics.Font("PrometoBlack");
    }

    /// <summary>
    /// Desenha as linhas horizontais da grade e seus rótulos de valor vertical.
    /// </summary>
    /// <param name="canvas">O canvas onde os elementos serão desenhados.</param>
    /// <param name="graphRect">Área útil onde as curvas e a grade serão desenhadas.</param>
    /// <param name="min">Valor mínimo global das curvas.</param>
    /// <param name="max">Valor máximo global das curvas.</param>
    /// <remarks>
    /// As linhas horizontais ajudam na leitura dos valores verticais.
    /// Os rótulos são alinhados à esquerda do gráfico.
    /// </remarks>
    void DrawHorizontalGridLines(ICanvas canvas, RectF graphRect, double min, double max)
    {
        for (int i = 0; i <= HorizontalLineCount; i++)
        {
            float y = graphRect.Y + i * (graphRect.Height / HorizontalLineCount);
            canvas.DrawLine(graphRect.X, y, graphRect.X + graphRect.Width, y);

            double valor = max - ((max - min) / HorizontalLineCount) * i;
            canvas.DrawString($"{valor:F0}", graphRect.X - 16, y + 2, HorizontalAlignment.Right);
        }
    }

    /// <summary>
    /// Desenha as linhas verticais da grade e os rótulos de tempo (dias).
    /// </summary>
    /// <param name="canvas">O canvas onde os elementos serão desenhados.</param>
    /// <param name="graphRect">Área útil do gráfico.</param>
    /// <remarks>
    /// Os rótulos representam os dias da simulação, iniciando em 1. A distribuição é proporcional ao total de pontos.
    /// </remarks>
    void DrawVerticalGridLines(ICanvas canvas, RectF graphRect)
    {
        int maxPoints = Curves.Max(c => c.Points.Count);

        for (int i = 0; i <= VerticalLineCount; i++)
        {
            float x = graphRect.X + i * (graphRect.Width / VerticalLineCount);
            canvas.DrawLine(x, graphRect.Y, x, graphRect.Y + graphRect.Height);

            int dia = (int)(i * (maxPoints - 1) / (float)VerticalLineCount);
            canvas.DrawString($"{dia + 1}", x, graphRect.Y + graphRect.Height + 20, HorizontalAlignment.Center);
        }
    }

    /// <summary>
    /// Desenha todas as curvas no gráfico, respeitando estilo, progresso e escala vertical.
    /// </summary>
    /// <param name="canvas">O canvas de desenho.</param>
    /// <param name="graphRect">Área do gráfico onde as curvas são traçadas.</param>
    /// <param name="min">Valor mínimo global</param>
    /// <param name="max">Valor máximo global</param>
    void DrawCurves(ICanvas canvas, RectF graphRect, double min, double max)
    {
        Debug.WriteLine($"[DRAW] Desenhando {Curves.Count} curva(s)");
        for (int idx = 0; idx < Curves.Count; idx++)
        {
            var curve = Curves[idx];
            var estilo = curve.Style;
            var pontos = curve.Points;
            var cor = curve.Color;

            float progress = (idx < ProgressByCurve.Count) ? ProgressByCurve[idx] : 1f;

            int visibleCount = (int)(pontos.Count * progress);
            if (visibleCount < 2) continue;

            float stepX = graphRect.Width / (pontos.Count - 1);

            canvas.StrokeDashPattern = estilo switch
            {
                LineStyle.Tracejada => [3, 4],
                LineStyle.Pontilhada => [1, 1],
                LineStyle.TracoLongo => [10, 4],
                _ => null
            };

            canvas.StrokeSize = estilo switch
            {
                LineStyle.Fina => 0.5f,
                LineStyle.Espessa => 6,
                _ => 2
            };

            canvas.StrokeLineCap = estilo == LineStyle.Arredondada ? LineCap.Round : LineCap.Butt;
            canvas.StrokeLineJoin = estilo == LineStyle.Arredondada ? LineJoin.Round : LineJoin.Miter;

            var path = new PathF();
            for (int i = 0; i < visibleCount; i++)
            {
                float x = graphRect.X + i * stepX;
                float y = graphRect.Y + (float)(graphRect.Height - ((pontos[i] - min) / (max - min)) * graphRect.Height);

                if (i == 0) path.MoveTo(x, y);
                else path.LineTo(x, y);
            }

            canvas.StrokeColor = cor;
            
            canvas.DrawPath(path);
        }
    }

    /// <summary>
    /// Exibe a informação de valor e dia para a curva atualmente selecionada, com base na posição do ponteiro.
    /// </summary>
    /// <param name="canvas">O canvas onde o texto e o ponto serão desenhados.</param>
    /// <param name="graphRect">Área útil do gráfico.</param>
    /// <param name="dirtyRect">Área total disponível para ajustes de limites.</param>
    /// <param name="min">Valor mínimo global (para normalização do eixo Y).</param>
    /// <param name="max">Valor máximo global.</param>
    /// <remarks>
    /// O rótulo exibido mostra o valor naquele ponto específico da curva.
    /// Também é desenhado um ponto branco na interseção da curva.
    /// </remarks>
    void DrawPointerInfo(ICanvas canvas, RectF graphRect, RectF dirtyRect, double min, double max)
    {
        if (SelectedCurve is not null && PointerPosition is PointF pointer)
        {
            var idx = Curves.FindIndex(c => c.Color == SelectedCurve.Color && c.Style == SelectedCurve.Style);
            if (idx >= 0 && idx < Curves.Count)
            {
                var curve = Curves[idx];
                var pontos = curve.Points;
                if (pontos.Count > 0)
                {
                    float stepX = graphRect.Width / (pontos.Count - 1);
                    int pos = (int)((pointer.X - graphRect.X) / stepX);
                    pos = Math.Clamp(pos, 0, pontos.Count - 1);

                    float x = graphRect.X + pos * stepX;
                    float y = graphRect.Y + (float)(graphRect.Height - ((pontos[pos] - min) / (max - min)) * graphRect.Height);

                    string texto = $"Dia {pos + 1}: R$ {pontos[pos]:F2}";
                    var font = new Microsoft.Maui.Graphics.Font("PrometoMedium");
                    float fontSize = 14;
                    float padding = 8;

                    canvas.Font = font;
                    canvas.FontSize = fontSize;
                    canvas.FontColor = BackgroundOption != BackgroundColorOption.Branco ? Colors.White : Colors.Black;

                    var textSize = canvas.GetStringSize(texto, font, fontSize);
                    float boxWidth = textSize.Width + padding * 2;
                    float boxHeight = textSize.Height + padding * 2;

                    float boxX = x + 6;
                    float boxY = y - boxHeight - 10;

                    if (boxX + boxWidth > dirtyRect.Right)
                        boxX = dirtyRect.Right - boxWidth - 10;
                    if (boxY < 0)
                        boxY = 0;

                    //canvas.FillColor = Colors.White;
                    //canvas.FillRoundedRectangle(boxX, boxY, boxWidth, boxHeight, 8);

                    float textX = boxX + boxWidth / 2;
                    float textY = boxY + (boxHeight + textSize.Height) / 2 - 2;
                    canvas.DrawString(texto, textX, textY, HorizontalAlignment.Center);

                    // Ponto na curva
                    canvas.FillColor = BackgroundOption != BackgroundColorOption.Branco ? Colors.White : Colors.Black;
                    canvas.FillCircle(x, y, 4);
                }
            }
        }
    }
}