using MemofyLife.Data;
using MemofyLife.Data.Migrations;
using SQLite;

namespace MemofyLife.Tests.Data;

public sealed class MigrationTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();

    public Task InitializeAsync()
    {
        return _db.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        return _db.DisposeAsync();
    }

    [Fact]
    public async Task Migrations_SetUserVersionToLatest()
    {
        SQLiteAsyncConnection connection = await _db.Database.GetConnectionAsync();

        int version = await MigrationRunner.GetVersionAsync(connection);

        Assert.Equal(MigrationCatalog.All[^1].Version, version);
    }

    [Fact]
    public async Task Migrations_CreateVehicleTables()
    {
        SQLiteAsyncConnection connection = await _db.Database.GetConnectionAsync();

        List<string> tables = await connection.QueryScalarsAsync<string>(
            "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name");

        Assert.Contains("vehicle", tables);
        Assert.Contains("vehicle_entry", tables);
        Assert.Contains("fuel_detail", tables);
        Assert.Contains("service_detail", tables);
        Assert.Contains("service_category", tables);
        Assert.Contains("vehicle_reminder", tables);
    }

    [Fact]
    public async Task Migrations_SeedServiceCategories()
    {
        SQLiteAsyncConnection connection = await _db.Database.GetConnectionAsync();

        List<string> names = await connection.QueryScalarsAsync<string>(
            "SELECT name FROM service_category ORDER BY sort_order");

        Assert.Equal(["Olajcsere", "Fékek", "Gumi", "Műszaki vizsga", "Egyéb"], names);
    }

    [Fact]
    public async Task ForeignKeys_AreEnabled()
    {
        SQLiteAsyncConnection connection = await _db.Database.GetConnectionAsync();

        int enabled = await connection.ExecuteScalarAsync<int>("PRAGMA foreign_keys");

        Assert.Equal(1, enabled);
    }

    [Fact]
    public async Task Reopening_DoesNotRunMigrationsTwice()
    {
        string path = _db.Database.Path;
        await _db.Database.DisposeAsync();

        await using var reopened = new AppDatabase(path);
        SQLiteAsyncConnection again = await reopened.GetConnectionAsync();

        int categories = await again.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM service_category");
        Assert.Equal(5, categories);
    }
}
