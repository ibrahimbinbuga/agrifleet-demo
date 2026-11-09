using AgriFleet.Core.Services;
using AgriFleet.Core.Simulation;
using FluentAssertions;

namespace AgriFleet.Tests;

public class FlightSimulationTests
{
    private const double StepSeconds = 10;

    private static readonly GeoPoint Base = new(37, 35);

    private static FlightPlan CreatePlan(double pathKm, double tankPctPerMeter, double drainFactor = 1) => new(
        Base,
        [Base, new GeoPoint(Base.Lat + pathKm * 1000 / SprayPathGenerator.MetersPerDegreeLat, Base.Lng)],
        tankPctPerMeter,
        drainFactor);

    private static (FlightSimulation Simulation, List<FlightPhase> Phases, List<FlightEvent> Events) Fly(FlightPlan plan)
    {
        var simulation = new FlightSimulation(plan);
        var phases = new List<FlightPhase> { simulation.Phase };
        var events = new List<FlightEvent>();

        for (var step = 0; step < 5000 && simulation.Phase != FlightPhase.Finished; step++)
        {
            events.AddRange(simulation.Step(StepSeconds));

            if (phases[^1] != simulation.Phase)
                phases.Add(simulation.Phase);
        }

        return (simulation, phases, events);
    }

    [Fact]
    public void Normal_flight_visits_every_phase_in_order()
    {
        var (_, phases, _) = Fly(CreatePlan(pathKm: 1, tankPctPerMeter: 0.08));

        phases.Should().Equal(
            FlightPhase.Takeoff,
            FlightPhase.Transit,
            FlightPhase.Spraying,
            FlightPhase.ReturnToBase,
            FlightPhase.Landing,
            FlightPhase.Charging,
            FlightPhase.Finished);
    }

    [Fact]
    public void Normal_flight_lands_once_charges_fully_and_is_not_aborted()
    {
        var (simulation, _, events) = Fly(CreatePlan(pathKm: 1, tankPctPerMeter: 0.08));

        events.Should().Equal(FlightEvent.Landed, FlightEvent.Charged);
        simulation.IsAborted.Should().BeFalse();
        simulation.AltitudeM.Should().Be(0);
        simulation.BatteryPct.Should().Be(100);
        simulation.TankLevelPct.Should().BeApproximately(20, 5);
    }

    [Fact]
    public void Battery_fault_warns_once_then_aborts_and_returns_to_base()
    {
        var (simulation, phases, events) = Fly(CreatePlan(pathKm: 8, tankPctPerMeter: 0.01, FlightPlan.BatteryFaultDrainFactor));

        events.Should().ContainInOrder(FlightEvent.LowBattery, FlightEvent.Aborted, FlightEvent.Landed);
        events.Count(e => e == FlightEvent.LowBattery).Should().Be(1);
        simulation.IsAborted.Should().BeTrue();
        phases.Should().ContainInOrder(FlightPhase.Spraying, FlightPhase.ReturnToBase, FlightPhase.Landing);
        FlightPhysics.DistanceMeters(simulation.Position, Base).Should().BeLessThan(0.001);
    }

    [Fact]
    public void Empty_tank_ends_spraying_early_without_aborting()
    {
        var (simulation, _, events) = Fly(CreatePlan(pathKm: 2, tankPctPerMeter: 0.5));

        events.Should().Contain(FlightEvent.TankEmpty);
        simulation.TankLevelPct.Should().Be(0);
        simulation.IsAborted.Should().BeFalse();
    }

    [Fact]
    public void Stepping_a_finished_flight_is_a_no_op()
    {
        var (simulation, _, _) = Fly(CreatePlan(pathKm: 1, tankPctPerMeter: 0.08));

        simulation.Step(StepSeconds).Should().BeEmpty();
        simulation.Phase.Should().Be(FlightPhase.Finished);
    }

    [Fact]
    public void Same_random_seed_produces_the_same_plan()
    {
        GeoPoint[] field = [new(36.925, 34.905), new(36.931, 34.906), new(36.930, 34.914), new(36.924, 34.913)];

        var first = FlightPlan.Create(field, new Random(99));
        var second = FlightPlan.Create(field, new Random(99));

        first.SprayPath.Should().Equal(second.SprayPath);
        first.TankPctPerMeter.Should().Be(second.TankPctPerMeter);
        first.BatteryDrainFactor.Should().Be(second.BatteryDrainFactor);
    }

    [Fact]
    public void Battery_fault_rate_is_close_to_the_configured_chance()
    {
        GeoPoint[] field = [new(36.925, 34.905), new(36.931, 34.906), new(36.930, 34.914), new(36.924, 34.913)];
        var random = new Random(2024);
        const int plans = 2000;

        var faulty = Enumerable.Range(0, plans)
            .Count(_ => FlightPlan.Create(field, random).BatteryDrainFactor > 1);

        (faulty / (double)plans).Should().BeApproximately(FlightPlan.BatteryFaultChance, 0.03);
    }
}
