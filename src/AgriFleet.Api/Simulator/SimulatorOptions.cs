namespace AgriFleet.Api.Simulator;

public class SimulatorOptions
{
    public const string SectionName = "Simulator";

    public bool Enabled { get; set; }
    public double TimeScale { get; set; } = 10;
    public int MaxConcurrentFlights { get; set; } = 3;
    public double StartChancePerTick { get; set; } = 0.15;
}
