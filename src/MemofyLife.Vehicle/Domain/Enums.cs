namespace MemofyLife.Vehicle.Domain;

// A számértékek az adatbázisba kerülnek: soha nem szabad megváltoztatni őket.

public enum EntryType
{
    Odometer = 1,
    Fuel = 2,
    Service = 3,
}

public enum FuelType
{
    Petrol = 1,
    Diesel = 2,
    Hybrid = 3,
    Electric = 4,
    Lpg = 5,
}

public enum OdometerWarning
{
    None = 0,
    BelowInitial = 1,
    BelowEarlierEntry = 2,
    AboveLaterEntry = 3,
}

/// <summary>Karbantartási emlékeztető állapota; a nagyobb érték a sürgősebb.</summary>
public enum ReminderState
{
    Unknown = 0,
    Ok = 1,
    DueSoon = 2,
    Overdue = 3,
}
