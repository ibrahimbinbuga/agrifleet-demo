using AgriFleet.Core.Services;

namespace AgriFleet.Core.Simulation;

public record FlightPlan(
    GeoPoint Base,
    IReadOnlyList<GeoPoint> SprayPath,
    double TankPctPerMeter,
    double BatteryDrainFactor)
{
    public const double BatteryFaultChance = 0.10;
    public const double BatteryFaultDrainFactor = 2.2;

    private const double TargetSprayPathMeters = 7000;
    private const double TrialSwathMeters = 50;
    private const double PointSpacingMeters = 25;
    private const double BaseOffsetMeters = 170;

    public static FlightPlan Create(IReadOnlyList<GeoPoint> boundary, Random random)
    {
        var path = FitPathToTargetLength(boundary, random.Next());
        var tankUsagePct = 75 + random.NextDouble() * 30;
        var drainFactor = random.NextDouble() < BatteryFaultChance ? BatteryFaultDrainFactor : 1;

        var basePoint = new GeoPoint(
            boundary.Min(p => p.Lat) - BaseOffsetMeters / SprayPathGenerator.MetersPerDegreeLat,
            boundary.Average(p => p.Lng));

        return new FlightPlan(basePoint, path, tankUsagePct / FlightPhysics.PathLengthMeters(path), drainFactor);
    }

    private static IReadOnlyList<GeoPoint> FitPathToTargetLength(IReadOnlyList<GeoPoint> boundary, int seed)
    {
        var trial = SprayPathGenerator.Generate(boundary, TrialSwathMeters, PointSpacingMeters, seed);
        var swathMeters = TrialSwathMeters * FlightPhysics.PathLengthMeters(trial) / TargetSprayPathMeters;

        return SprayPathGenerator.Generate(boundary, swathMeters, PointSpacingMeters, seed);
    }
}
