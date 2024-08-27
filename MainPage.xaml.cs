namespace maui.boozer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnApplyClicked(object sender, EventArgs e)
        {
            decimal value = (Decimal)(A.Value * 0.33 + B.Value * 0.5 + C.Value * 1.0 + D.Value * 1.5);
            
            ((Button)sender).Text = $"Внесено {value} л.";
        }
    }
}
