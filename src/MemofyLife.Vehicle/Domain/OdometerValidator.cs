namespace MemofyLife.Vehicle.Domain;

/// <summary>
/// A km-állás ellenőrzése a szomszédos bejegyzésekhez képest. Csak figyelmeztet,
/// nem tilt: elírás vagy óracsere előfordul.
/// </summary>
public static class OdometerValidator
{
    /// <param name="km">Az új km-állás.</param>
    /// <param name="initialKm">A jármű kezdő km-állása.</param>
    /// <param name="maxBefore">A legnagyobb km-állás az azonos vagy korábbi napokon.</param>
    /// <param name="minAfter">A legkisebb km-állás a későbbi napokon.</param>
    public static OdometerWarning Check(int km, int initialKm, int? maxBefore, int? minAfter)
    {
        if (km < initialKm)
        {
            return OdometerWarning.BelowInitial;
        }

        if (maxBefore is not null && km < maxBefore.Value)
        {
            return OdometerWarning.BelowEarlierEntry;
        }

        if (minAfter is not null && km > minAfter.Value)
        {
            return OdometerWarning.AboveLaterEntry;
        }

        return OdometerWarning.None;
    }
}
