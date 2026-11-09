namespace AgriFleet.Core.Simulation;

public enum FlightPhase { Takeoff, Transit, Spraying, ReturnToBase, Landing, Charging, Finished }

public enum FlightEvent { LowBattery, TankEmpty, Aborted, Landed, Charged }
