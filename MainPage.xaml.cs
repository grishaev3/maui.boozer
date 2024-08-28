using System.Collections.ObjectModel;
using System.Text.Json;
using maui.boozer.Model;

namespace maui.boozer
{
    public partial class MainPage : ContentPage
    {
        private string _storageFileName = "shots.json";
        private string _storagePath => Path.Combine(FileSystem.AppDataDirectory, _storageFileName);

        public static readonly BindableProperty ToodayProperty = BindableProperty.Create(nameof(Tooday), typeof(decimal), typeof(MainPage));
        public decimal Tooday
        {
            get => (decimal)GetValue(ToodayProperty);
            set => SetValue(ToodayProperty, value);
        }

        public ObservableCollection<Shot> ShotsCollection { get; private set; }

        public MainPage()
        {
            InitializeComponent();

            LoadData();
        }

        private async void LoadData()
        {
            ShotsCollection = await LoadShotsAsync(_storagePath);

            UpdateToodayProperty(ShotsCollection);

            BindingContext = this;
        }

        public static async Task<ObservableCollection<Shot>> LoadShotsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            // C:\Users\{current_user}\AppData\Local\Packages\com.companyname.maui.boozer_9zz4h110yvjzm\LocalState\shots.json
            string jsonString = await File.ReadAllTextAsync(filePath);

            ObservableCollection<Shot>? result = null;
            try
            {
                result = JsonSerializer.Deserialize<ObservableCollection<Shot>>(jsonString);
            }
            catch
            {
                File.Delete(filePath);
                return [];
            }

            if (result == null)
            {
                throw new Exception("Deserialize error");
            }
            else
            {
                return new ObservableCollection<Shot>(result);
            }
        }

        public static async Task SaveShotsAsync(ObservableCollection<Shot> shots, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(shots);

            await File.WriteAllTextAsync(filePath, jsonString);
        }

        private async void OnApplyClicked(object sender, EventArgs e)
        {
            decimal value = GetTotal(A.Value, B.Value, C.Value, D.Value);
            if (value == 0)
            {
                ((Button)sender).Text = $"Ничего не отмечено";

                return;
            }

            ShotsCollection.Add(new Shot
            {
                Date = DateTime.Now,
                ThirdLitterCount = A.Value,
                HalfLitterCount = B.Value,
                OneLitterCount = C.Value,
                OneAndHalfLitterCount = D.Value
            });

            UpdateToodayProperty(ShotsCollection);

            try
            {
                await MainPage.SaveShotsAsync(ShotsCollection, _storagePath);

                ((Button)sender).Text = $"Внесено {value} л.";

                A.Value = B.Value = C.Value = D.Value = 0;
            }
            catch (Exception ex)
            {
                ((Button)sender).Text = $"Ошибка: {ex.Message}";
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("", "Точно удаляем?", "Да", "Нет");

            if (!answer)
            {
                return;
            }

            var shot = (Shot)((Button)sender).BindingContext;

            var item = ShotsCollection.FirstOrDefault(e => e.ID == shot.ID);
            if (item != null)
            {
                ShotsCollection.Remove(item);

                UpdateToodayProperty(ShotsCollection);

                await MainPage.SaveShotsAsync(ShotsCollection, _storagePath);
            }
        }

        private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            // TODO:
        }

        private void UpdateToodayProperty(ObservableCollection<Shot> shots)
        {
            var now = DateTime.Now.Date;

            Tooday = shots.Where(e => e.Date.Date == now).Sum(GetTotal);
        }

        private static decimal GetTotal(int a, int b, int c, int d) => (decimal)(a * 0.33 + b * 0.5 + c * 1.0 + d * 1.5);

        private static decimal GetTotal(Shot s) => (decimal)(s.ThirdLitterCount * 0.33 + s.HalfLitterCount * 0.5 + s.OneLitterCount * 1.0 + s.OneAndHalfLitterCount * 1.5);
    }
}
