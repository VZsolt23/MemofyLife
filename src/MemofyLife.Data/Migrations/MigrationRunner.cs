using SQLite;

namespace MemofyLife.Data.Migrations;

public static class MigrationRunner
{
    public static async Task RunAsync(
        SQLiteAsyncConnection connection,
        IReadOnlyList<IMigration> migrations)
    {
        EnsureSequential(migrations);

        int current = await GetVersionAsync(connection).ConfigureAwait(false);
        int latest = migrations.Count == 0 ? 0 : migrations[^1].Version;

        if (current > latest)
        {
            throw new InvalidOperationException(
                $"Az adatbázis sémaverziója ({current}) újabb, mint amit az alkalmazás ismer ({latest}).");
        }

        foreach (IMigration migration in migrations.Where(m => m.Version > current))
        {
            await connection.RunInTransactionAsync(c =>
            {
                migration.Apply(c);
                c.Execute($"PRAGMA user_version = {migration.Version}");
            }).ConfigureAwait(false);
        }
    }

    public static Task<int> GetVersionAsync(SQLiteAsyncConnection connection)
    {
        return connection.ExecuteScalarAsync<int>("PRAGMA user_version");
    }

    private static void EnsureSequential(IReadOnlyList<IMigration> migrations)
    {
        for (var i = 0; i < migrations.Count; i++)
        {
            if (migrations[i].Version != i + 1)
            {
                throw new InvalidOperationException(
                    $"A migrációk verziószáma 1-től folytonos kell legyen; a(z) {i + 1}. helyen {migrations[i].Version} áll.");
            }
        }
    }
}
