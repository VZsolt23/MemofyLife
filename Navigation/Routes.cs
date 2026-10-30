namespace MemofyLife.Navigation;

public static class Routes
{
    // Tab-gyökerek
    public const string HomeTab = "home";
    public const string EventsTab = "events";
    public const string VehicleTab = "vehicle";
    public const string ArcadeTab = "arcade";

    // Lapozott oldalak
    public const string SettingsPage = "settings";
    public const string PersonEditPage = "person-edit";
    public const string VehicleLogPage = "vehicle-log";
    public const string EntryEditPage = "entry-edit";
    public const string MineSweeperPage = "mine-sweeper";

    public static class Params
    {
        public const string PersonId = "personId";
        public const string VehicleId = "vehicleId";
        public const string EntryId = "entryId";
        public const string EntryType = "entryType";
        public const string Variant = "variant";
    }
}
