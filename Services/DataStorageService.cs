using maui.boozer.Model;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace maui.boozer.Services
{
    public interface IDataStorageService
    {
        Task<ObservableCollection<Shot>> LoadShotsAsync(string filePath);

        Task SaveShotsAsync(ObservableCollection<Shot> shots, string filePath);
    }

    public class DataStorageService : IDataStorageService
    {
        public async Task<ObservableCollection<Shot>> LoadShotsAsync(string filePath)
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

        public async Task SaveShotsAsync(ObservableCollection<Shot> shots, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(shots);

            await File.WriteAllTextAsync(filePath, jsonString);
        }
    }
}
