using AgriFleet.Core.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace AgriFleet.Api.Hubs;

public interface ITelemetryClient
{
    Task TelemetryBatch(IReadOnlyList<LiveTelemetryDto> points);
    Task DroneStatusChanged(DroneStatusChangedDto change);
    Task AlertRaised(DroneAlertDto alert);
}

public class TelemetryHub : Hub<ITelemetryClient>;
