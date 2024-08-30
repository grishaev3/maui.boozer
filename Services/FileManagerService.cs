namespace maui.boozer.Services
{
    public interface IFileManagerService
    {
        Task CopyFileToAppDataDirectory(string filename);
    }

    public class FileManagerService : IFileManagerService
    {
        //https://stackoverflow.com/questions/76674724/maui-android-how-to-read-asset-files
        public async Task CopyFileToAppDataDirectory(string filename)
        {
            // Open the source file
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(filename);

            // Create an output filename
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            // Copy the file to the AppDataDirectory
            using FileStream outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);
        }
    }
}
