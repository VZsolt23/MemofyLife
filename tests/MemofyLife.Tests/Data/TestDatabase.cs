using MemofyLife.Data;

namespace MemofyLife.Tests.Data;

/// <summary>Ideiglenes fájlba írt, migrált adatbázis; a teszt végén törlődik.</summary>
internal sealed class TestDatabase : IAsyncLifetime
{
    private readonly string _path =
        Path.Combine(Path.GetTempPath(), $"memofylife-test-{Guid.NewGuid():N}.db3");

    public TestDatabase()
    {
        Database = new AppDatabase(_path);
    }

    public AppDatabase Database { get; }

    public Task InitializeAsync()
    {
        return Database.GetConnectionAsync();
    }

    public async Task DisposeAsync()
    {
        await Database.DisposeAsync();

        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
