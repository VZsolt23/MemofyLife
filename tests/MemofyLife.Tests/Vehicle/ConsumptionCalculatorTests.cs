using MemofyLife.Vehicle.Domain;

namespace MemofyLife.Tests.Vehicle;

public sealed class ConsumptionCalculatorTests
{
    [Fact]
    public void TwoFullTanks_GiveConsumptionForSecond()
    {
        FuelFill[] fills =
        [
            new(1, 10_000, 40, true),
            new(2, 10_500, 35, true),
        ];

        IReadOnlyDictionary<int, double> result = ConsumptionCalculator.PerFill(fills);

        Assert.Single(result);
        Assert.Equal(7.0, result[2], 6);
    }

    [Fact]
    public void PartialFill_IsAddedToNextFullTank()
    {
        FuelFill[] fills =
        [
            new(1, 10_000, 40, true),
            new(2, 10_300, 15, false),
            new(3, 10_800, 25, true),
        ];

        IReadOnlyDictionary<int, double> result = ConsumptionCalculator.PerFill(fills);

        Assert.False(result.ContainsKey(2));
        Assert.Equal(5.0, result[3], 6); // (15 + 25) / 800 * 100
    }

    [Fact]
    public void FillsBeforeFirstFullTank_AreIgnored()
    {
        FuelFill[] fills =
        [
            new(1, 9_800, 20, false),
            new(2, 10_000, 40, true),
            new(3, 10_400, 28, true),
        ];

        IReadOnlyDictionary<int, double> result = ConsumptionCalculator.PerFill(fills);

        Assert.Equal(7.0, result[3], 6);
        Assert.Single(result);
    }

    [Fact]
    public void Order_IsByOdometerNotInput()
    {
        FuelFill[] fills =
        [
            new(2, 10_500, 35, true),
            new(1, 10_000, 40, true),
        ];

        Assert.Equal(7.0, ConsumptionCalculator.PerFill(fills)[2], 6);
    }

    [Fact]
    public void ZeroDistance_SegmentIsSkipped()
    {
        FuelFill[] fills =
        [
            new(1, 10_000, 40, true),
            new(2, 10_000, 5, true),
            new(3, 10_500, 35, true),
        ];

        IReadOnlyDictionary<int, double> result = ConsumptionCalculator.PerFill(fills);

        Assert.False(result.ContainsKey(2));
        Assert.Equal(7.0, result[3], 6);
    }

    [Fact]
    public void Average_IsWeightedByDistance()
    {
        FuelFill[] fills =
        [
            new(1, 10_000, 40, true),
            new(2, 10_500, 35, true), // 7,0
            new(3, 11_500, 50, true), // 5,0
        ];

        Assert.Equal(85.0 / 1500 * 100, ConsumptionCalculator.Average(fills)!.Value, 6);
    }

    [Fact]
    public void Average_IsNullWithoutClosedSegment()
    {
        FuelFill[] fills = [new(1, 10_000, 40, true), new(2, 10_200, 10, false)];

        Assert.Null(ConsumptionCalculator.Average(fills));
    }
}
