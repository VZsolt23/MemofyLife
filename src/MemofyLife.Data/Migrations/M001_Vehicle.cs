using SQLite;

namespace MemofyLife.Data.Migrations;

/// <summary>
/// A jármű modul táblái (docs/adatbazis_es_projektstruktura.md, 3.3–3.8) és a
/// szervizkategóriák kezdő adatai.
/// </summary>
internal sealed class M001_Vehicle : IMigration
{
    // Az sqlite-net egy Execute hívásban csak egy utasítást futtat, ezért külön elemek.
    private static readonly string[] Statements =
    [
        """
        CREATE TABLE vehicle (
            id                  INTEGER PRIMARY KEY AUTOINCREMENT,
            name                TEXT    NOT NULL,
            make                TEXT    NULL,
            model               TEXT    NULL,
            model_year          INTEGER NULL,
            license_plate       TEXT    NULL,
            fuel_type           INTEGER NULL,
            initial_odometer_km INTEGER NOT NULL DEFAULT 0 CHECK (initial_odometer_km >= 0),
            is_archived         INTEGER NOT NULL DEFAULT 0,
            created_at          TEXT    NOT NULL,
            updated_at          TEXT    NOT NULL
        )
        """,
        """
        CREATE TABLE service_category (
            id         INTEGER PRIMARY KEY AUTOINCREMENT,
            name       TEXT    NOT NULL UNIQUE COLLATE NOCASE,
            is_system  INTEGER NOT NULL DEFAULT 0,
            sort_order INTEGER NOT NULL
        )
        """,
        """
        CREATE TABLE vehicle_entry (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            vehicle_id  INTEGER NOT NULL REFERENCES vehicle (id) ON DELETE CASCADE,
            entry_type  INTEGER NOT NULL CHECK (entry_type IN (1, 2, 3)),
            entry_date  TEXT    NOT NULL,
            odometer_km INTEGER NOT NULL CHECK (odometer_km >= 0),
            total_cost  INTEGER NULL CHECK (total_cost IS NULL OR total_cost >= 0),
            note        TEXT    NULL,
            created_at  TEXT    NOT NULL,
            updated_at  TEXT    NOT NULL
        )
        """,
        "CREATE INDEX ix_vehicle_entry_log ON vehicle_entry (vehicle_id, entry_date DESC, id DESC)",
        "CREATE INDEX ix_vehicle_entry_odometer ON vehicle_entry (vehicle_id, odometer_km)",
        """
        CREATE TABLE fuel_detail (
            entry_id     INTEGER PRIMARY KEY REFERENCES vehicle_entry (id) ON DELETE CASCADE,
            liters       REAL    NOT NULL CHECK (liters > 0),
            is_full_tank INTEGER NOT NULL DEFAULT 1
        )
        """,
        """
        CREATE TABLE service_detail (
            entry_id    INTEGER PRIMARY KEY REFERENCES vehicle_entry (id) ON DELETE CASCADE,
            category_id INTEGER NOT NULL REFERENCES service_category (id) ON DELETE RESTRICT,
            description TEXT    NULL,
            workshop    TEXT    NULL
        )
        """,
        "CREATE INDEX ix_service_detail_category ON service_detail (category_id)",
        """
        CREATE TABLE vehicle_reminder (
            id                 INTEGER PRIMARY KEY AUTOINCREMENT,
            vehicle_id         INTEGER NOT NULL REFERENCES vehicle (id) ON DELETE CASCADE,
            title              TEXT    NOT NULL,
            category_id        INTEGER NULL REFERENCES service_category (id) ON DELETE SET NULL,
            interval_km        INTEGER NULL CHECK (interval_km IS NULL OR interval_km > 0),
            interval_months    INTEGER NULL CHECK (interval_months IS NULL OR interval_months > 0),
            last_done_km       INTEGER NULL,
            last_done_date     TEXT    NULL,
            last_entry_id      INTEGER NULL REFERENCES vehicle_entry (id) ON DELETE SET NULL,
            due_km             INTEGER NULL,
            due_date           TEXT    NULL,
            notify_before_km   INTEGER NULL,
            notify_before_days INTEGER NULL,
            is_active          INTEGER NOT NULL DEFAULT 1,
            created_at         TEXT    NOT NULL,
            updated_at         TEXT    NOT NULL
        )
        """,
        "CREATE INDEX ix_vehicle_reminder_vehicle ON vehicle_reminder (vehicle_id, is_active)",
        "CREATE INDEX ix_vehicle_reminder_category ON vehicle_reminder (category_id)",
        "CREATE INDEX ix_vehicle_reminder_last_entry ON vehicle_reminder (last_entry_id)",
        """
        INSERT INTO service_category (name, is_system, sort_order) VALUES
            ('Olajcsere', 1, 1),
            ('Fékek', 1, 2),
            ('Gumi', 1, 3),
            ('Műszaki vizsga', 1, 4),
            ('Egyéb', 1, 5)
        """,
    ];

    public int Version => 1;

    public void Apply(SQLiteConnection connection)
    {
        foreach (string sql in Statements)
        {
            connection.Execute(sql);
        }
    }
}
