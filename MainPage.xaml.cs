using maui.boozer.Model;
using maui.boozer.Services;
using System.Collections.ObjectModel;

namespace maui.boozer
{
    public partial class MainPage : ContentPage
    {
        private readonly IDataStorageService _dataStorage;
        private readonly IFileManagerService _fileManagerService;

        private static string _storageFile = "kotlin.json";
        private static string _storageFilePath => Path.Combine(FileSystem.AppDataDirectory, _storageFile);
        private static string _syncFilePath => Path.Combine(FileSystem.AppDataDirectory, "sync.json");
        private DateTime _currentDate = DateTime.Now.Date;

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

        public string Version { get; set; }

        public ObservableCollection<Shot> ShotsCollection { get; private set; }

        public ObservableCollection<Shot> FilteredShotsCollection { get; private set; }

        public MainPage(IDataStorageService dataStorage, IFileManagerService fileManagerService)
        {
            InitializeComponent();

            _dataStorage = dataStorage;
            _fileManagerService = fileManagerService;

            Version = VersionTracking.Default.CurrentVersion.ToString();

            LoadData();
        }

        private async void LoadData()
        {
            await SyncAssetToLocalStorageIfNeeded();

            ShotsCollection = await _dataStorage.LoadShotsAsync(_storageFilePath);

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

            if (_currentDate != DateTime.Now.Date)
            {
                bool answer = await DisplayAlert("", $"Точно вносим за {_currentDate}?", "Да", "Нет");
                if (!answer)
                {
                    return;
                }
            }

            Shot item = new()
            {
                Date = GetInsertedDate(),
                ThirdLiterCount = A.Value,
                HalfLiterCount = B.Value,
                OneLiterCount = C.Value,
                OneAndHalfLiterCount = D.Value
            };
            ShotsCollection.Add(item);
            OnDateSelectedImpl(_currentDate);

            UpdateTotalPerDayProperties(ShotsCollection, _currentDate);

            try
            {
                await _dataStorage.SaveShotsAsync(ShotsCollection, _storageFilePath);

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

            UpdateTotalPerDayProperties(ShotsCollection, _currentDate);

            await _dataStorage.SaveShotsAsync(ShotsCollection, _storageFilePath);
        }

        private void OnLast10Clicked(object sender, EventArgs e)
        {
            Apply.Text = "Внести";
            OnLast10ClickedImpl();
        }
        private void OnLast10ClickedImpl()
        {
            _currentDate = DateTime.Now.Date;

            IEnumerable<Shot> filtered = ShotsCollection.OrderByDescending(e => e.Date).Take(10).OrderBy(e => e.Date);
            FilteredShotsCollection = new ObservableCollection<Shot>(filtered);

            UpdateTotalPerDayProperties(FilteredShotsCollection, _currentDate);
            OnPropertyChanged(nameof(FilteredShotsCollection));
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            OnDateSelectedImpl(e.NewDate.Date);
        }
        private void OnDateSelectedImpl(DateTime day)
        {
            _currentDate = day.Date;

            IEnumerable<Shot> filtered = ShotsCollection.Where(p => p.Date.Date == day);
            FilteredShotsCollection = new ObservableCollection<Shot>(filtered);

            UpdateTotalPerDayProperties(FilteredShotsCollection, day);
            OnPropertyChanged(nameof(FilteredShotsCollection));
        }

        private void UpdateTotalPerDayProperties(ObservableCollection<Shot> shots, DateTime day)
        {
            if (day == DateTime.Now.Date)
            {
                Day = "сегодня";
            }
            else
            {
                Day = day.ToString("dd.MM.yy ddd");
            }

            TotalPerDay = shots.Where(e => e.Date.Date == day).Sum(GetTotal);
        }

        private async Task SyncAssetToLocalStorageIfNeeded()
        {
            var status = await _dataStorage.LoadSyncStatusAsync(_syncFilePath);
            if (status?.IsSynchronized == false)
            {
                await _fileManagerService.CopyFileToAppDataDirectory(_storageFile);
                await _dataStorage.SaveSyncStatusAsync(new SyncStatus
                {
                    IsSynchronized = true,
                    SynchronizedAt = DateTime.Now
                }, _syncFilePath);
            }
        }

        private static decimal GetTotal(int a, int b, int c, int d) => (decimal)(a * 0.33 + b * 0.5 + c * 1.0 + d * 1.5);

        private static decimal GetTotal(Shot s) => (decimal)(s.ThirdLiterCount * 0.33 + s.HalfLiterCount * 0.5 + s.OneLiterCount * 1.0 + s.OneAndHalfLiterCount * 1.5);

        private DateTime GetInsertedDate()
        {
            if (_currentDate == DateTime.Now.Date)
            {
                return _currentDate.Add(DateTime.Now.TimeOfDay);
            }
            else
            {
                return _currentDate;
            }
        }
    }
}
