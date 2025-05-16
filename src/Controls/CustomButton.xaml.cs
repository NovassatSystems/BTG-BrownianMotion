using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace BTGBrownianMotion.Controls;

public partial class CustomButton : Border
{
    public CustomButton()
    {
        InitializeComponent();

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (IsEnabled)
            {
                Clicked?.Invoke(this, EventArgs.Empty);
                
            }
        };
        GestureRecognizers.Add(tapGesture);
    }

    public event EventHandler? Clicked;

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(CustomButton),
            default(ICommand));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(
            nameof(Icon),
            typeof(ImageSource),
            typeof(CustomButton),
            default(ImageSource));

    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(CustomButton),
            string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
