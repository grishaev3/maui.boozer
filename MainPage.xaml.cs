using System.Collections.ObjectModel;
using System.Text.Json;
using maui.boozer.Model;

namespace maui.boozer
{
    public partial class MainPage : ContentPage
    {
        private string _storageFileName = "shots.json";
        private string _storagePath => Path.Combine(FileSystem.AppDataDirectory, _storageFileName);

        public string Tooday { get; set; }

        public ObservableCollection<Shot> Shots { get; set; }

        public MainPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            Shots = await LoadShotsAsync(_storagePath);

            UpdateDoze(Shots);

            BindingContext = this;
        }

        public async Task<ObservableCollection<Shot>> LoadShotsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

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
                return new ObservableCollection<Shot>(result.OrderByDescending(x => x.Date));
            }
        }

        public async Task SaveShotsAsync(ObservableCollection<Shot> shots, string filePath)
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

            Shots.Add(new Shot
            {
                Date = DateTime.Now,
                ThirdLitterCount = A.Value,
                HalfLitterCount = B.Value,
                OneLitterCount = C.Value,
                OneAndHalfLitterCount = D.Value
            });


            UpdateDoze(Shots);

            try
            {
                await SaveShotsAsync(Shots, _storagePath);

                ((Button)sender).Text = $"Внесено {value} л.";
            }
            catch (Exception ex)
            {
                ((Button)sender).Text = $"Ошибка: {ex.Message}";
            }
        }

        private void UpdateDoze(ObservableCollection<Shot> shots)
        {
            var now = DateTime.Now.Date;

            decimal tooday = shots.Where(e => e.Date.Date == now).Sum(e => 
                GetTotal(e.ThirdLitterCount, e.HalfLitterCount, e.OneLitterCount, e.OneAndHalfLitterCount));

            Tooday = $"Выпито за сегодня: {tooday} л.";
        }

        private static decimal GetTotal(int a, int b, int c, int d) => (decimal)(a * 0.33 + b * 0.5 + c * 1.0 + d * 1.5);
    }
}
