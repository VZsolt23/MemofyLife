using SQLite;

namespace MemofyLife.Data.Migrations;

/// <summary>
/// Egyirányú sémaváltozás. Kiadott migrációt soha nem módosítunk, csak újat írunk.
/// </summary>
public interface IMigration
{
    /// <summary>A migráció utáni <c>PRAGMA user_version</c> értéke (1, 2, 3…).</summary>
    int Version { get; }

    /// <summary>Tranzakción belül fut; a verziószámot a futtató állítja be.</summary>
    void Apply(SQLiteConnection connection);
}
