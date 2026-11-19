using MemofyLife.Vehicle.Domain;
using MemofyLife.Vehicle.Models;

namespace MemofyLife.Tests.Vehicle;

public sealed class ValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 19);

    [Theory]
    [InlineData(50_000, OdometerWarning.None)]
    [InlineData(5_000, OdometerWarning.BelowInitial)]
    [InlineData(39_000, OdometerWarning.BelowEarlierEntry)]
    [InlineData(61_000, OdometerWarning.AboveLaterEntry)]
    public void Odometer_ComparesToNeighbours(int km, OdometerWarning expected)
    {
        Assert.Equal(expected, OdometerValidator.Check(km, 10_000, 40_000, 60_000));
    }

    [Fact]
    public void Odometer_NoNeighboursIsFine()
    {
        Assert.Equal(OdometerWarning.None, OdometerValidator.Check(10_000, 0, null, null));
    }

    [Fact]
    public void Fuel_RequiresLiters()
    {
        EntryInput entry = Entry(EntryType.Fuel) with { Liters = null };

        Assert.NotNull(InputValidator.Validate(entry));
        Assert.Null(InputValidator.Validate(entry with { Liters = 38.4 }));
    }

    [Fact]
    public void Service_RequiresCategory()
    {
        EntryInput entry = Entry(EntryType.Service);

        Assert.NotNull(InputValidator.Validate(entry));
        Assert.Null(InputValidator.Validate(entry with { CategoryId = 1 }));
    }

    [Fact]
    public void Entry_RejectsNegativeCost()
    {
        Assert.NotNull(InputValidator.Validate(Entry(EntryType.Odometer) with { TotalCost = -1 }));
    }

    [Fact]
    public void Vehicle_RequiresName()
    {
        Assert.NotNull(InputValidator.Validate(new VehicleProfile { Name = "  " }, 2026));
        Assert.Null(InputValidator.Validate(new VehicleProfile { Name = "Saját autó" }, 2026));
    }

    [Fact]
    public void Vehicle_RejectsFutureModelYear()
    {
        var vehicle = new VehicleProfile { Name = "Autó", ModelYear = 2028 };

        Assert.NotNull(InputValidator.Validate(vehicle, 2026));
        Assert.Null(InputValidator.Validate(vehicle with { ModelYear = 2027 }, 2026));
    }

    [Fact]
    public void RecurringReminder_RequiresLastDone()
    {
        var reminder = new ReminderInput { VehicleId = 1, Title = "Olajcsere", IntervalKm = 15_000 };

        Assert.NotNull(InputValidator.Validate(reminder));
        Assert.Null(InputValidator.Validate(reminder with { LastDoneKm = 100_000 }));
    }

    [Fact]
    public void OneOffReminder_RequiresDue()
    {
        var reminder = new ReminderInput { VehicleId = 1, Title = "Műszaki" };

        Assert.NotNull(InputValidator.Validate(reminder));
        Assert.Null(InputValidator.Validate(reminder with { DueDate = Today }));
    }

    private static EntryInput Entry(EntryType type)
    {
        return new EntryInput { VehicleId = 1, Type = type, Date = Today, OdometerKm = 100_000 };
    }
}
