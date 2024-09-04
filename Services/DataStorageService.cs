using maui.boozer.Model;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace maui.boozer.Services
{
    public interface IDataStorageService
    {
        //Task<ObservableCollection<Shot>> LoadMauiAssetAsync(string fileName);

        Task<SyncStatus> LoadSyncStatusAsync(string filePath);
        Task SaveSyncStatusAsync(SyncStatus status, string filePath);

        Task<ObservableCollection<Shot>> LoadShotsAsync(string filePath);
        Task SaveShotsAsync(ObservableCollection<Shot> shots, string filePath);
    }

    public class DataStorageService : IDataStorageService
    {
        public async Task<ObservableCollection<Shot>> LoadMauiAssetAsync(string fileName)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var reader = new StreamReader(stream);

            var jsonString = reader.ReadToEnd();

            var (result, ex) = DeserializeImpl(jsonString);

            if (ex != null)
            {
                throw ex;
            }

            return result;
        }

        public async Task<ObservableCollection<Shot>> LoadShotsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            // C:\Users\Sneg\AppData\Local\Packages\com.companyname.maui.boozer_9zz4h110yvjzm\LocalState\shots.json
            string jsonString = await File.ReadAllTextAsync(filePath);

            var (result, _) = DeserializeImpl(jsonString);

            return result;
        }

        private static (ObservableCollection<Shot>, Exception?) DeserializeImpl(string jsonString)
        {
            ObservableCollection<Shot>? result = null;
            try
            {
                result = JsonSerializer.Deserialize<ObservableCollection<Shot>>(jsonString);
            }
            catch (Exception ex)
            {
                return ([], ex);
            }

            if (result == null)
            {
                throw new Exception("Deserialize error");
            }
            else
            {
                return (new ObservableCollection<Shot>(result), null);
            }
        }

        public async Task SaveShotsAsync(ObservableCollection<Shot> shots, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(shots);

            await File.WriteAllTextAsync(filePath, jsonString);
        }

        public async Task<SyncStatus> LoadSyncStatusAsync(string filePath)
        {
            var defaultStatus = new SyncStatus { IsSynchronized = false };
            if (!File.Exists(filePath))
            {
                return await Task.FromResult(defaultStatus);
            }

            string jsonString = await File.ReadAllTextAsync(filePath);

            try
            {
                return JsonSerializer.Deserialize<SyncStatus>(jsonString);
            }
            catch 
            {
                return await Task.FromResult(defaultStatus);
            }
        }

        public async Task SaveSyncStatusAsync(SyncStatus status, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(status);

            await File.WriteAllTextAsync(filePath, jsonString);
        }
    }
}
