
namespace maui.boozer.Controls;

public partial class StepperView : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(int), typeof(StepperView), 0, BindingMode.TwoWay);

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public StepperView()
    {
        InitializeComponent();
    }

    private void OnPlusButtonClicked(object sender, EventArgs e)
    {
        if ((Value + 1) <= 10)
        {
            Value += 1;
        }
    }

    private void OnMinusButtonClicked(object sender, EventArgs e)
    {
        if ((Value - 1) >= 0)
        {
            Value -= 1;
        }
    }
}