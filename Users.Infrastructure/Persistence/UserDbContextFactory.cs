using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence;
using Shared.Infrastructure;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var dbPath = DatabasePathHelper.GetDatabasePath("users.db");

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new UserDbContext(optionsBuilder.Options);
    }

    //public UserDbContext CreateDbContext(string[] args)
    //{
    //    // Ищем директорию с Program.cs (стартовый проект)
    //    var startupProjectDir = FindStartupProjectDirectory();

    //    // data директория должна быть внутри стартового проекта
    //    var dataDirectory = Path.Combine(startupProjectDir, "data");

    //    if (!Directory.Exists(dataDirectory))
    //    {
    //        Directory.CreateDirectory(dataDirectory);
    //    }

    //    var dbPath = Path.Combine(dataDirectory, "users.db");

    //    Console.WriteLine($"Using database: {dbPath}");

    //    var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
    //    optionsBuilder.UseSqlite($"Data Source={dbPath}");

    //    return new UserDbContext(optionsBuilder.Options);
    //}

    //private string FindStartupProjectDirectory()
    //{
    //    // Начинаем с текущей директории
    //    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

    //    // Поднимаемся вверх, пока не найдём Program.cs
    //    while (directory != null)
    //    {
    //        // Проверяем, есть ли Program.cs в этой директории
    //        if (File.Exists(Path.Combine(directory.FullName, "Program.cs")))
    //        {
    //            return directory.FullName;
    //        }

    //        // Проверяем в поддиректориях
    //        var programFile = directory.GetFiles("Program.cs", SearchOption.AllDirectories)
    //            .FirstOrDefault();

    //        if (programFile != null)
    //        {
    //            return programFile.DirectoryName;
    //        }

    //        directory = directory.Parent;
    //    }

    //    throw new Exception("Startup project not found");
    //}
}