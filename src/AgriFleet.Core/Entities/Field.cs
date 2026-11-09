namespace AgriFleet.Core.Entities;

public class Field
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CropType { get; set; } = string.Empty;
    public double AreaHectares { get; set; }

    /// <summary>Polygon as a JSON array of [lat, lng] pairs.</summary>
    public string BoundaryJson { get; set; } = "[]";

    public List<Flight> Flights { get; set; } = new();
}
