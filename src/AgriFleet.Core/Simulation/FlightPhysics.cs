using AgriFleet.Core.Services;

namespace AgriFleet.Core.Simulation;

public static class FlightPhysics
{
    public const double LitersPerHectare = 8;
    public const double LowBatteryPct = 20;
    public const double AbortBatteryPct = 15;
    public const double ChargePctPerSecond = 0.3;

    public static double BatteryDrainPerSecond(FlightPhase phase) => phase switch
    {
        FlightPhase.Takeoff or FlightPhase.Landing => 0.06,
        FlightPhase.Transit or FlightPhase.ReturnToBase => 0.035,
        FlightPhase.Spraying => 0.042,
        _ => 0,
    };

    public static double DrainBattery(double batteryPct, FlightPhase phase, double seconds, double drainFactor) =>
        Math.Max(0, batteryPct - BatteryDrainPerSecond(phase) * seconds * drainFactor);

    public static double ChargeBattery(double batteryPct, double seconds) =>
        Math.Min(100, batteryPct + ChargePctPerSecond * seconds);

    public static double DrainTank(double tankPct, double meters, double pctPerMeter) =>
        Math.Max(0, tankPct - meters * pctPerMeter);

    public static bool ShouldAbort(FlightPhase phase, double batteryPct) =>
        phase == FlightPhase.Spraying && batteryPct < AbortBatteryPct;

    public static double DistanceMeters(GeoPoint a, GeoPoint b)
    {
        var dLat = (b.Lat - a.Lat) * SprayPathGenerator.MetersPerDegreeLat;
        var dLng = (b.Lng - a.Lng) * SprayPathGenerator.MetersPerDegreeLng((a.Lat + b.Lat) / 2);

        return Math.Sqrt(dLat * dLat + dLng * dLng);
    }

    public static double PathLengthMeters(IReadOnlyList<GeoPoint> path) =>
        path.Zip(path.Skip(1), DistanceMeters).Sum();
}
