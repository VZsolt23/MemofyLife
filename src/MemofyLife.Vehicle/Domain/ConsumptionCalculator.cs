namespace MemofyLife.Vehicle.Domain;

public readonly record struct FuelFill(int EntryId, int OdometerKm, double Liters, bool IsFullTank);

/// <summary>
/// Fogyasztás (l/100 km) két teli tankolás között. A köztes, részleges tankolások literjei
/// a következő teli tankoláshoz adódnak. Az első teli tankolás előtti tankolások nem számítanak.
/// </summary>
public static class ConsumptionCalculator
{
    /// <summary>
    /// Teli tankolásonkénti fogyasztás: kulcs a teli tankolás bejegyzés-azonosítója.
    /// Csak az előző teli tankolással lezárt szakaszokra ad értéket.
    /// </summary>
    public static IReadOnlyDictionary<int, double> PerFill(IEnumerable<FuelFill> fills)
    {
        var result = new Dictionary<int, double>();

        foreach (Segment segment in Segments(fills))
        {
            result[segment.ClosingEntryId] = segment.Liters / segment.DistanceKm * 100;
        }

        return result;
    }

    /// <summary>Az összes lezárt szakasz súlyozott átlaga, vagy null, ha nincs ilyen.</summary>
    public static double? Average(IEnumerable<FuelFill> fills)
    {
        double liters = 0;
        long distance = 0;

        foreach (Segment segment in Segments(fills))
        {
            liters += segment.Liters;
            distance += segment.DistanceKm;
        }

        return distance > 0 ? liters / distance * 100 : null;
    }

    private static IEnumerable<Segment> Segments(IEnumerable<FuelFill> fills)
    {
        int? lastFullKm = null;
        double litersSince = 0;

        foreach (FuelFill fill in fills.OrderBy(f => f.OdometerKm).ThenBy(f => f.EntryId))
        {
            if (lastFullKm is null)
            {
                if (fill.IsFullTank)
                {
                    lastFullKm = fill.OdometerKm;
                    litersSince = 0;
                }

                continue;
            }

            litersSince += fill.Liters;

            if (!fill.IsFullTank)
            {
                continue;
            }

            int distance = fill.OdometerKm - lastFullKm.Value;

            // Azonos km-állás (pl. elírás) esetén nincs értelmes fogyasztás; a szakaszt elvetjük.
            if (distance > 0)
            {
                yield return new Segment(fill.EntryId, litersSince, distance);
            }

            lastFullKm = fill.OdometerKm;
            litersSince = 0;
        }
    }

    private readonly record struct Segment(int ClosingEntryId, double Liters, int DistanceKm);
}
