namespace MemofyLife.Vehicle.Domain;

public sealed record MaintenanceStatus(ReminderState State, int? KmLeft, int? DaysLeft);

/// <summary>Karbantartási esedékesség számítása és kiértékelése.</summary>
public static class MaintenanceSchedule
{
    /// <summary>Ha az emlékeztető nem ad meg saját értéket, ennyivel előtte „közeleg”.</summary>
    public const int DefaultNotifyBeforeKm = 1000;

    public const int DefaultNotifyBeforeDays = 30;

    public static bool IsRecurring(int? intervalKm, int? intervalMonths)
    {
        return intervalKm is not null || intervalMonths is not null;
    }

    /// <summary>Ismétlődő emlékeztető következő esedékessége az utolsó elvégzésből.</summary>
    public static (int? DueKm, DateOnly? DueDate) ComputeDue(
        int? intervalKm,
        int? intervalMonths,
        int? lastDoneKm,
        DateOnly? lastDoneDate)
    {
        int? dueKm = intervalKm is not null && lastDoneKm is not null
            ? lastDoneKm + intervalKm
            : null;

        DateOnly? dueDate = intervalMonths is not null && lastDoneDate is not null
            ? lastDoneDate.Value.AddMonths(intervalMonths.Value)
            : null;

        return (dueKm, dueDate);
    }

    /// <summary>
    /// A km- és a dátumalapú ág közül a sürgősebb állapot számít.
    /// Ha egyik ágnak sincs esedékessége, az állapot <see cref="ReminderState.Unknown"/>.
    /// </summary>
    public static MaintenanceStatus Evaluate(
        int? dueKm,
        DateOnly? dueDate,
        int? notifyBeforeKm,
        int? notifyBeforeDays,
        int currentKm,
        DateOnly today)
    {
        var state = ReminderState.Unknown;
        int? kmLeft = null;
        int? daysLeft = null;

        if (dueKm is not null)
        {
            kmLeft = dueKm.Value - currentKm;
            state = Max(state, Classify(kmLeft.Value, notifyBeforeKm ?? DefaultNotifyBeforeKm));
        }

        if (dueDate is not null)
        {
            daysLeft = dueDate.Value.DayNumber - today.DayNumber;
            state = Max(state, Classify(daysLeft.Value, notifyBeforeDays ?? DefaultNotifyBeforeDays));
        }

        return new MaintenanceStatus(state, kmLeft, daysLeft);
    }

    private static ReminderState Classify(int left, int threshold)
    {
        if (left < 0)
        {
            return ReminderState.Overdue;
        }

        return left <= threshold ? ReminderState.DueSoon : ReminderState.Ok;
    }

    private static ReminderState Max(ReminderState a, ReminderState b)
    {
        return a >= b ? a : b;
    }
}
