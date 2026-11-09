using AgriFleet.Core.Services;
using AgriFleet.Core.Simulation;
using FluentAssertions;

namespace AgriFleet.Tests;

public class FlightPhysicsTests
{
    [Theory]
    [InlineData(FlightPhase.Takeoff, 94)]
    [InlineData(FlightPhase.Landing, 94)]
    [InlineData(FlightPhase.Transit, 96.5)]
    [InlineData(FlightPhase.ReturnToBase, 96.5)]
    [InlineData(FlightPhase.Spraying, 95.8)]
    [InlineData(FlightPhase.Charging, 100)]
    public void Battery_drains_by_phase(FlightPhase phase, double expected)
    {
        FlightPhysics.DrainBattery(100, phase, 100, drainFactor: 1).Should().BeApproximately(expected, 0.0001);
    }

    [Fact]
    public void Drain_factor_multiplies_consumption()
    {
        FlightPhysics.DrainBattery(100, FlightPhase.Spraying, 100, drainFactor: 2).Should().BeApproximately(91.6, 0.0001);
    }

    [Fact]
    public void Battery_never_drops_below_zero()
    {
        FlightPhysics.DrainBattery(1, FlightPhase.Takeoff, 1000, drainFactor: 5).Should().Be(0);
    }

    [Fact]
    public void Charging_is_capped_at_full()
    {
        FlightPhysics.ChargeBattery(99, 1000).Should().Be(100);
    }

    [Theory]
    [InlineData(100, 500, 0.01, 95)]
    [InlineData(1, 500, 0.01, 0)]
    public void Tank_drains_with_distance_and_stops_at_empty(double tank, double meters, double pctPerMeter, double expected)
    {
        FlightPhysics.DrainTank(tank, meters, pctPerMeter).Should().BeApproximately(expected, 0.0001);
    }

    [Theory]
    [InlineData(FlightPhase.Spraying, 14.9, true)]
    [InlineData(FlightPhase.Spraying, 15, false)]
    [InlineData(FlightPhase.Transit, 5, false)]
    [InlineData(FlightPhase.ReturnToBase, 5, false)]
    [InlineData(FlightPhase.Landing, 5, false)]
    public void Abort_only_applies_while_spraying_below_threshold(FlightPhase phase, double battery, bool expected)
    {
        FlightPhysics.ShouldAbort(phase, battery).Should().Be(expected);
    }

    [Fact]
    public void Distance_along_a_meridian_matches_the_degree_constant()
    {
        var meters = FlightPhysics.DistanceMeters(new GeoPoint(36, 35), new GeoPoint(37, 35));

        meters.Should().BeApproximately(SprayPathGenerator.MetersPerDegreeLat, 0.001);
    }

    [Fact]
    public void Path_length_sums_every_segment()
    {
        GeoPoint[] path = [new(36, 35), new(37, 35), new(38, 35)];

        FlightPhysics.PathLengthMeters(path).Should().BeApproximately(2 * SprayPathGenerator.MetersPerDegreeLat, 0.001);
    }
}
