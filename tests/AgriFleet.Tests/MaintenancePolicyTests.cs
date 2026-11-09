using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using AgriFleet.Core.Services;
using FluentAssertions;

namespace AgriFleet.Tests;

public class MaintenancePolicyTests
{
    private static readonly DateTime Now = new(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    private static Drone CreateDrone(double minutes, double daysSinceService) => new()
    {
        FlightMinutesSinceMaintenance = minutes,
        LastMaintenanceAt = Now.AddDays(-daysSinceService),
    };

    [Theory]
    [InlineData(0, 0, MaintenanceStatus.Ok)]
    [InlineData(2699.9, 80.9, MaintenanceStatus.Ok)]
    [InlineData(2700, 0, MaintenanceStatus.DueSoon)]
    [InlineData(2999.9, 0, MaintenanceStatus.DueSoon)]
    [InlineData(3000, 0, MaintenanceStatus.Overdue)]
    [InlineData(0, 81, MaintenanceStatus.DueSoon)]
    [InlineData(0, 89.9, MaintenanceStatus.DueSoon)]
    [InlineData(0, 90, MaintenanceStatus.Overdue)]
    public void Evaluate_applies_limits_at_their_boundaries(double minutes, double days, MaintenanceStatus expected)
    {
        MaintenancePolicy.Evaluate(CreateDrone(minutes, days), Now).Status.Should().Be(expected);
    }

    [Fact]
    public void Evaluate_prefers_overdue_days_over_due_soon_minutes()
    {
        var result = MaintenancePolicy.Evaluate(CreateDrone(2800, 95), Now);

        result.Status.Should().Be(MaintenanceStatus.Overdue);
        result.Reason.Should().Contain("gün");
    }

    [Fact]
    public void Evaluate_reports_flight_minutes_first_when_both_limits_are_due_soon()
    {
        var result = MaintenancePolicy.Evaluate(CreateDrone(2950, 89), Now);

        result.Status.Should().Be(MaintenanceStatus.DueSoon);
        result.Reason.Should().Contain("2950/3000");
    }

    [Theory]
    [InlineData(1500, 9, 0.5)]
    [InlineData(300, 45, 0.5)]
    [InlineData(3300, 0, 1.1)]
    public void Urgency_uses_the_limit_closest_to_being_reached(double minutes, double days, double expected)
    {
        MaintenancePolicy.Urgency(CreateDrone(minutes, days), Now).Should().BeApproximately(expected, 0.0001);
    }
}
