using AgriFleet.Core.Services;

namespace AgriFleet.Core.Simulation;

/// <summary>Flight state machine driven only by elapsed simulated seconds; no clock, database or randomness.</summary>
public class FlightSimulation
{
    private const double TakeoffSeconds = 30;
    private const double LandingSeconds = 30;
    private const double CruiseAltitudeM = 5;
    private const double SprayAltitudeM = 3.5;
    private const double CruiseSpeedMs = 8;
    private const double SpraySpeedMs = 5;

    private readonly FlightPlan _plan;
    private IReadOnlyList<GeoPoint> _route = [];
    private int _targetIndex;
    private double _phaseElapsed;
    private bool _lowBatteryRaised;

    public FlightSimulation(FlightPlan plan)
    {
        ArgumentOutOfRangeException.ThrowIfZero(plan.SprayPath.Count);
        _plan = plan;
        Position = plan.Base;
    }

    public FlightPhase Phase { get; private set; } = FlightPhase.Takeoff;
    public GeoPoint Position { get; private set; }
    public double AltitudeM { get; private set; }
    public double MaxAltitudeM { get; private set; }
    public double SpeedMs { get; private set; }
    public double BatteryPct { get; private set; } = 100;
    public double TankLevelPct { get; private set; } = 100;
    public double FlightSeconds { get; private set; }
    public bool IsAborted { get; private set; }
    public bool IsAirborne => Phase is not (FlightPhase.Charging or FlightPhase.Finished);

    public IReadOnlyList<FlightEvent> Step(double seconds)
    {
        var events = new List<FlightEvent>();

        if (Phase == FlightPhase.Finished)
            return events;

        _phaseElapsed += seconds;

        if (Phase == FlightPhase.Charging)
        {
            BatteryPct = FlightPhysics.ChargeBattery(BatteryPct, seconds);

            if (BatteryPct >= 100)
            {
                Enter(FlightPhase.Finished);
                events.Add(FlightEvent.Charged);
            }

            return events;
        }

        FlightSeconds += seconds;
        BatteryPct = FlightPhysics.DrainBattery(BatteryPct, Phase, seconds, _plan.BatteryDrainFactor);

        AdvancePhase(seconds, events);
        MaxAltitudeM = Math.Max(MaxAltitudeM, AltitudeM);

        if (!_lowBatteryRaised && Phase != FlightPhase.Charging && BatteryPct < FlightPhysics.LowBatteryPct)
        {
            _lowBatteryRaised = true;
            events.Add(FlightEvent.LowBattery);
        }

        if (FlightPhysics.ShouldAbort(Phase, BatteryPct))
        {
            IsAborted = true;
            events.Add(FlightEvent.Aborted);
            Enter(FlightPhase.ReturnToBase, [_plan.Base]);
        }

        return events;
    }

    private void AdvancePhase(double seconds, List<FlightEvent> events)
    {
        switch (Phase)
        {
            case FlightPhase.Takeoff:
                SpeedMs = 0;
                AltitudeM = CruiseAltitudeM * Math.Min(1, _phaseElapsed / TakeoffSeconds);
                if (_phaseElapsed >= TakeoffSeconds)
                    Enter(FlightPhase.Transit, [_plan.SprayPath[0]]);
                break;

            case FlightPhase.Transit:
                SpeedMs = CruiseSpeedMs;
                if (MoveAlongRoute(CruiseSpeedMs * seconds))
                    Enter(FlightPhase.Spraying, _plan.SprayPath);
                break;

            case FlightPhase.Spraying:
                Spray(seconds, events);
                break;

            case FlightPhase.ReturnToBase:
                SpeedMs = CruiseSpeedMs;
                AltitudeM = CruiseAltitudeM;
                if (MoveAlongRoute(CruiseSpeedMs * seconds))
                    Enter(FlightPhase.Landing);
                break;

            case FlightPhase.Landing:
                SpeedMs = 0;
                AltitudeM = CruiseAltitudeM * Math.Max(0, 1 - _phaseElapsed / LandingSeconds);
                if (_phaseElapsed >= LandingSeconds)
                {
                    AltitudeM = 0;
                    Enter(FlightPhase.Charging);
                    events.Add(FlightEvent.Landed);
                }
                break;
        }
    }

    private void Spray(double seconds, List<FlightEvent> events)
    {
        SpeedMs = SpraySpeedMs;
        AltitudeM = SprayAltitudeM;

        var meters = SpraySpeedMs * seconds;
        var pathFinished = MoveAlongRoute(meters);
        TankLevelPct = FlightPhysics.DrainTank(TankLevelPct, meters, _plan.TankPctPerMeter);

        if (TankLevelPct <= 0)
        {
            events.Add(FlightEvent.TankEmpty);
            Enter(FlightPhase.ReturnToBase, [_plan.Base]);
        }
        else if (pathFinished)
        {
            Enter(FlightPhase.ReturnToBase, [_plan.Base]);
        }
    }

    private bool MoveAlongRoute(double meters)
    {
        while (_targetIndex < _route.Count)
        {
            var target = _route[_targetIndex];
            var distance = FlightPhysics.DistanceMeters(Position, target);

            if (distance > meters)
            {
                var ratio = meters / distance;
                Position = new GeoPoint(
                    Position.Lat + (target.Lat - Position.Lat) * ratio,
                    Position.Lng + (target.Lng - Position.Lng) * ratio);
                return false;
            }

            Position = target;
            meters -= distance;
            _targetIndex++;
        }

        return true;
    }

    private void Enter(FlightPhase phase, IReadOnlyList<GeoPoint>? route = null)
    {
        Phase = phase;
        _phaseElapsed = 0;
        _route = route ?? [];
        _targetIndex = 0;
    }
}
