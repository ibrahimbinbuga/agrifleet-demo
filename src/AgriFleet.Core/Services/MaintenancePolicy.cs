using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Services;

public record MaintenanceEvaluation(MaintenanceStatus Status, string Reason);

public static class MaintenancePolicy
{
    public const int FlightMinutesLimit = 3000;
    public const int DaysLimit = 90;
    public const int FlightMinutesDueSoon = FlightMinutesLimit * 9 / 10;
    public const int DaysDueSoon = DaysLimit * 9 / 10;

    public static MaintenanceEvaluation Evaluate(Drone drone, DateTime now)
    {
        var minutes = drone.FlightMinutesSinceMaintenance;
        var days = (now - drone.LastMaintenanceAt).TotalDays;

        if (minutes >= FlightMinutesLimit)
            return new(MaintenanceStatus.Overdue, $"Uçuş süresi limiti aşıldı ({minutes:F0}/{FlightMinutesLimit} dk)");

        if (days >= DaysLimit)
            return new(MaintenanceStatus.Overdue, $"Son bakımdan bu yana {days:F0} gün geçti (limit {DaysLimit})");

        if (minutes >= FlightMinutesDueSoon)
            return new(MaintenanceStatus.DueSoon, $"Uçuş süresi limite yaklaşıyor ({minutes:F0}/{FlightMinutesLimit} dk)");

        if (days >= DaysDueSoon)
            return new(MaintenanceStatus.DueSoon, $"Bakım tarihi yaklaşıyor ({days:F0}/{DaysLimit} gün)");

        return new(MaintenanceStatus.Ok, "Bakım gerekmiyor");
    }
}
