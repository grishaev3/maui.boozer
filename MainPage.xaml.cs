using maui.boozer.Model;
using maui.boozer.Services;
using System.Collections.ObjectModel;

namespace maui.boozer
{
    public partial class MainPage : ContentPage
    {
        private const string _storageFileName = "shots.json";
        private string _storagePath => Path.Combine(FileSystem.AppDataDirectory, _storageFileName);

        private readonly IDataStorageService _dataStorage;

        public static readonly BindableProperty TotalPerDayProperty = BindableProperty.Create(nameof(TotalPerDay), typeof(decimal), typeof(MainPage));
        public decimal TotalPerDay
        {
            get => (decimal)GetValue(TotalPerDayProperty);
            set => SetValue(TotalPerDayProperty, value);
        }

        public static readonly BindableProperty TotalDayProperty = BindableProperty.Create(nameof(Day), typeof(string), typeof(MainPage));
        public string Day 
        {
            get => (string)GetValue(TotalDayProperty);
            set  => SetValue(TotalDayProperty, value);
        }

        public ObservableCollection<Shot> ShotsCollection { get; private set; }

        public ObservableCollection<Shot> FilteredShotsCollection { get; private set; }

        public MainPage(IDataStorageService dataStorage)
        {
            InitializeComponent();

            _dataStorage = dataStorage;

            LoadData();
        }

        private async void LoadData()
        {
            ShotsCollection = await _dataStorage.LoadShotsAsync(_storagePath);

            OnLast10ClickedImpl();

            BindingContext = this;
        }

        private async void OnApplyClicked(object sender, EventArgs e)
        {
            decimal value = GetTotal(A.Value, B.Value, C.Value, D.Value);
            if (value == 0)
            {
                ((Button)sender).Text = $"Ничего не отмечено";

                return;
            }

            Shot item = new()
            {
                Date = DateTime.Now,
                ThirdLitterCount = A.Value,
                HalfLitterCount = B.Value,
                OneLitterCount = C.Value,
                OneAndHalfLitterCount = D.Value
            };
            ShotsCollection.Add(item);
            OnDateSelectedImpl(DateTime.Now.Date);

            UpdateTotalPerDayProperties(ShotsCollection);

            try
            {
                await _dataStorage.SaveShotsAsync(ShotsCollection, _storagePath);

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

            var item = ShotsCollection.Single(e => e.ID == shot.ID);
            var filteredItem = FilteredShotsCollection.Single(e => e.ID == shot.ID);

            ShotsCollection.Remove(item);
            FilteredShotsCollection.Remove(filteredItem);

            UpdateTotalPerDayProperties(ShotsCollection);

            await _dataStorage.SaveShotsAsync(ShotsCollection, _storagePath);
        }

        private void OnLast10Clicked(object sender, EventArgs e)
        {
            OnLast10ClickedImpl();
        }
        private void OnLast10ClickedImpl()
        {
            IEnumerable<Shot> filtered = ShotsCollection.Take(10);
            FilteredShotsCollection = new ObservableCollection<Shot>(filtered);

            UpdateTotalPerDayProperties(FilteredShotsCollection);
            OnPropertyChanged(nameof(FilteredShotsCollection));
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            OnDateSelectedImpl(e.NewDate.Date);
        }
        private void OnDateSelectedImpl(DateTime day)
        {
            IEnumerable<Shot> filtered = ShotsCollection.Where(p => p.Date.Date == day);
            FilteredShotsCollection = new ObservableCollection<Shot>(filtered);

            UpdateTotalPerDayProperties(FilteredShotsCollection, day);
            OnPropertyChanged(nameof(FilteredShotsCollection));
        }

        private void UpdateTotalPerDayProperties(ObservableCollection<Shot> shots, DateTime day = default)
        {
            if (day == default || day == DateTime.Now.Date)
            {
                day = DateTime.Now.Date;
                Day = "сегодня";
            }
            else
            {
                Day = day.ToString("dd.MM.yy ddd");
            }

            TotalPerDay = shots.Where(e => e.Date.Date == day).Sum(GetTotal);
        }

        private static decimal GetTotal(int a, int b, int c, int d) => (decimal)(a * 0.33 + b * 0.5 + c * 1.0 + d * 1.5);

        private static decimal GetTotal(Shot s) => (decimal)(s.ThirdLitterCount * 0.33 + s.HalfLitterCount * 0.5 + s.OneLitterCount * 1.0 + s.OneAndHalfLitterCount * 1.5);

        
    }
}
