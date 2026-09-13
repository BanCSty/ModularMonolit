namespace Shared.Infrastructure;

public static class DatabasePathHelper
{
    private static string? _cachedDataDirectory;

    public static string GetDataDirectory()
    {
        if (_cachedDataDirectory != null)
            return _cachedDataDirectory;

        // Находим базовую директорию (корень solution или стартовый проект)
        var baseDirectory = FindBaseDirectory();

        var dataDirectory = Path.Combine(baseDirectory, "data");

        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        _cachedDataDirectory = Path.GetFullPath(dataDirectory);

        Console.WriteLine($"[DatabasePathHelper] Base Directory -> {baseDirectory}");
        Console.WriteLine($"[DatabasePathHelper] Data Directory -> {_cachedDataDirectory}");

        return _cachedDataDirectory;
    }

    public static string GetDatabasePath(string dbName)
    {
        var dataDirectory = GetDataDirectory();
        var dbPath = Path.Combine(dataDirectory, dbName);

        Console.WriteLine($"[DatabasePathHelper] {dbName} -> {Path.GetFullPath(dbPath)}");

        return dbPath;
    }

    private static string FindBaseDirectory()
    {
        // Ищем директорию с .sln файлом или Program.cs
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory != null)
        {
            Console.WriteLine($"[DatabasePathHelper] Checking directory: {directory.FullName}");

            // Ищем .sln файл (корень solution)
            if (directory.GetFiles("*.sln").Any())
            {
                Console.WriteLine($"[DatabasePathHelper] Found solution directory: {directory.FullName}");
                return directory.FullName;
            }

            // Ищем Program.cs (стартовый проект)
            if (File.Exists(Path.Combine(directory.FullName, "Program.cs")))
            {
                Console.WriteLine($"[DatabasePathHelper] Found project directory: {directory.FullName}");
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        // Fallback на текущую директорию
        Console.WriteLine($"[DatabasePathHelper] Using fallback directory: {Directory.GetCurrentDirectory()}");
        return Directory.GetCurrentDirectory();
    }
}