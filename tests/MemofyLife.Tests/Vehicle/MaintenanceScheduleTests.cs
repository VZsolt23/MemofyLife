using MemofyLife.Vehicle.Domain;

namespace MemofyLife.Tests.Vehicle;

public sealed class MaintenanceScheduleTests
{
    private static readonly DateOnly Today = new(2026, 11, 19);

    [Fact]
    public void ComputeDue_AddsIntervals()
    {
        (int? dueKm, DateOnly? dueDate) = MaintenanceSchedule.ComputeDue(
            15_000, 12, 100_000, new DateOnly(2026, 3, 31));

        Assert.Equal(115_000, dueKm);
        Assert.Equal(new DateOnly(2027, 3, 31), dueDate);
    }

    [Fact]
    public void ComputeDue_MissingBranchGivesNull()
    {
        (int? dueKm, DateOnly? dueDate) = MaintenanceSchedule.ComputeDue(
            15_000, null, 100_000, Today);

        Assert.Equal(115_000, dueKm);
        Assert.Null(dueDate);
    }

    [Fact]
    public void ComputeDue_EndOfMonthIsClamped()
    {
        (_, DateOnly? dueDate) = MaintenanceSchedule.ComputeDue(
            null, 1, null, new DateOnly(2026, 1, 31));

        Assert.Equal(new DateOnly(2026, 2, 28), dueDate);
    }

    [Theory]
    [InlineData(120_000, ReminderState.Ok)]       // 5 000 km van hátra
    [InlineData(124_000, ReminderState.DueSoon)]  // 1 000 km: a határon már közeleg
    [InlineData(125_000, ReminderState.DueSoon)]  // pont esedékes
    [InlineData(125_001, ReminderState.Overdue)]
    public void Evaluate_KmBranch(int currentKm, ReminderState expected)
    {
        MaintenanceStatus status = MaintenanceSchedule.Evaluate(
            125_000, null, null, null, currentKm, Today);

        Assert.Equal(expected, status.State);
        Assert.Equal(125_000 - currentKm, status.KmLeft);
        Assert.Null(status.DaysLeft);
    }

    [Fact]
    public void Evaluate_DateBranch_UsesCustomThreshold()
    {
        MaintenanceStatus status = MaintenanceSchedule.Evaluate(
            null, Today.AddDays(10), null, 7, 0, Today);

        Assert.Equal(ReminderState.Ok, status.State);
        Assert.Equal(10, status.DaysLeft);
    }

    [Fact]
    public void Evaluate_WorstBranchWins()
    {
        MaintenanceStatus status = MaintenanceSchedule.Evaluate(
            200_000, Today.AddDays(-1), null, null, 100_000, Today);

        Assert.Equal(ReminderState.Overdue, status.State);
        Assert.Equal(-1, status.DaysLeft);
    }

    [Fact]
    public void Evaluate_NoDueIsUnknown()
    {
        MaintenanceStatus status = MaintenanceSchedule.Evaluate(
            null, null, null, null, 100_000, Today);

        Assert.Equal(ReminderState.Unknown, status.State);
    }
}
