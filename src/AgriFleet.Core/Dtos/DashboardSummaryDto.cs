namespace AgriFleet.Core.Dtos;

public record DashboardSummaryDto(
    int ActiveDrones,
    int FlightsToday,
    double HectaresToday,
    double LiquidTodayLiters,
    int MaintenanceDueCount,
    int OpenTickets);
