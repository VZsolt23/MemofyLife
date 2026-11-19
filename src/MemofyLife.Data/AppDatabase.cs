using MemofyLife.Data.Migrations;
using SQLite;

namespace MemofyLife.Data;

/// <summary>
/// Az alkalmazás egyetlen SQLite-adatbázisa. Az első használatkor nyitja meg a kapcsolatot,
/// bekapcsolja a külső kulcsokat és lefuttatja a hiányzó migrációkat.
/// </summary>
public sealed class AppDatabase : IAsyncDisposable
{
    private const SQLiteOpenFlags OpenFlags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;

    private readonly string _path;
    private readonly IReadOnlyList<IMigration> _migrations;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public AppDatabase(string path)
        : this(path, MigrationCatalog.All)
    {
    }

    public AppDatabase(string path, IReadOnlyList<IMigration> migrations)
    {
        _path = path;
        _migrations = migrations;
    }

    public string Path => _path;

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _initLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_connection is null)
            {
                var connection = new SQLiteAsyncConnection(
                    _path,
                    OpenFlags,
                    storeDateTimeAsTicks: false);

                // Kapcsolatonként kell bekapcsolni, enélkül a CASCADE nem fut le.
                await connection.ExecuteAsync("PRAGMA foreign_keys = ON").ConfigureAwait(false);
                await MigrationRunner.RunAsync(connection, _migrations).ConfigureAwait(false);
                _connection = connection;
            }

            return _connection;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync().ConfigureAwait(false);
            _connection = null;
        }

        _initLock.Dispose();
    }
}
