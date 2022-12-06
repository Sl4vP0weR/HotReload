namespace HotReload;

partial class HotReloader
{
    public static List<string> WatcherExtensions = new()
    {
        LibraryAssemblyFileExtension,
        ExecutableAssemblyFileExtension
    };
    private static readonly Dictionary<string, FileSystemWatcher> searchDirectories = new();
    public static IReadOnlyCollection<string> SearchDirectories => searchDirectories.Keys;

    public static void RemoveSearchDirectory(string path)
    {
        try
        {
            if (searchDirectories.TryGetValue(path, out var watcher))
                watcher.Dispose();
        }
        catch { }
        searchDirectories.Remove(path);
    }

    public static void AddSearchDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        RemoveSearchDirectory(path);
        FileSystemWatcher watcher = new(path);
        watcher.NotifyFilter = 
            NotifyFilters.Attributes |
            NotifyFilters.CreationTime |
            NotifyFilters.LastWrite |
            NotifyFilters.Size;
        watcher.Changed += FileChanged;
        searchDirectories[path] = watcher;
        watcher.IncludeSubdirectories = false;
        watcher.EnableRaisingEvents = true;
    }

    public static int LoadRetries = 3;
    private static async void FileChanged(object sender, FileSystemEventArgs e)
    {
        var filePath = e.FullPath;
        try
        {
            FileInfo file = new(filePath);
            if (!file.Exists) return;
            var fileExtensions = file.Extension.ToLower();
            if (!WatcherExtensions.Contains(fileExtensions)) return;
            for (int i = 0; i < LoadRetries+1; i++)
            {
                try
                {
                    Load(file);
                    break;
                }
                catch (IOException)
                {
                    await Task.Delay(1000);
                    if(i < LoadRetries) continue;
                    throw;
                }
            }
        }
        catch(Exception ex)
        {
            ErrorLogger?.Invoke(new Exception($"Failed to load assembly associated with {filePath}.", ex));
        }
    }
}