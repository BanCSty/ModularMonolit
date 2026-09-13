namespace Shared.Infrastructure;

public static class DatabasePathHelper
{
    private static string? _cachedDataDirectory;

    public static string GetDataDirectory()
    {
        if (_cachedDataDirectory != null)
            return _cachedDataDirectory;

        var dataDirectory = FindDataDirectory();

        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        _cachedDataDirectory = Path.GetFullPath(dataDirectory);

        Console.WriteLine($"[DatabasePathHelper] >>> FINAL Data Directory: {_cachedDataDirectory}");

        return _cachedDataDirectory;
    }

    public static string GetDatabasePath(string dbName)
    {
        var dataDirectory = GetDataDirectory();
        return Path.Combine(dataDirectory, dbName);
    }

    private static string FindDataDirectory()
    {
        Console.WriteLine("=======================================================");
        Console.WriteLine($"[DatabasePathHelper] Current Directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"[DatabasePathHelper] AppContext.BaseDirectory: {AppContext.BaseDirectory}");
        Console.WriteLine("=======================================================");

        // Ищем .sln
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        DirectoryInfo? solutionDir = null;

        var dir = currentDir;
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Any())
            {
                solutionDir = dir;
                Console.WriteLine($"[DatabasePathHelper] Found .sln in: {solutionDir.FullName}");
                break;
            }
            dir = dir.Parent;
        }

        if (solutionDir == null)
        {
            Console.WriteLine("[DatabasePathHelper] No .sln found, using current directory");
            return Path.Combine(Directory.GetCurrentDirectory(), "data");
        }

        // Ищем все Program.cs в solution (кроме bin/obj)
        Console.WriteLine($"[DatabasePathHelper] Searching for Program.cs in: {solutionDir.FullName}");

        var programFiles = Directory.GetFiles(
            solutionDir.FullName,
            "Program.cs",
            SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToList();

        Console.WriteLine($"[DatabasePathHelper] Found {programFiles.Count} Program.cs files:");
        foreach (var p in programFiles)
        {
            Console.WriteLine($"[DatabasePathHelper]   → {p}");
        }

        if (programFiles.Count == 0)
        {
            Console.WriteLine("[DatabasePathHelper] No Program.cs found, using solution dir");
            return Path.Combine(solutionDir.FullName, "data");
        }

        // Ищем Program.cs, наиболее близкий к solution (т.е. стартовый проект)
        // Берем тот, у которого путь самый короткий от solution
        var startupProgram = programFiles
            .OrderBy(p => p.Length)
            .First();

        var startupDir = Path.GetDirectoryName(startupProgram)!;
        Console.WriteLine($"[DatabasePathHelper] Startup project dir: {startupDir}");

        return Path.Combine(startupDir, "data");
    }
}