namespace AgriFleet.Core.Dtos;

public record FieldDto(Guid Id, string Name, string CropType, double AreaHectares, double[][] Boundary);
