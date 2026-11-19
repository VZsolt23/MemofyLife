namespace MemofyLife.Data.Migrations;

public static class MigrationCatalog
{
    /// <summary>Az összes migráció verziószám szerint növekvő sorrendben.</summary>
    public static IReadOnlyList<IMigration> All { get; } =
    [
        new M001_Vehicle(),
    ];
}
