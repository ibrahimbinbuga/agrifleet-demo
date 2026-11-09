namespace AgriFleet.Core.Services;

public readonly record struct GeoPoint(double Lat, double Lng);

/// <summary>Builds a boustrophedon (lawnmower) path of east-west rows clipped to a convex field polygon.</summary>
public static class SprayPathGenerator
{
    public const double MetersPerDegreeLat = 111_320;

    public static IReadOnlyList<GeoPoint> Generate(
        IReadOnlyList<GeoPoint> boundary,
        double swathWidthMeters,
        double pointSpacingMeters,
        int seed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(boundary.Count, 3);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(swathWidthMeters);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pointSpacingMeters);

        var minLat = boundary.Min(p => p.Lat);
        var maxLat = boundary.Max(p => p.Lat);
        var metersPerDegreeLng = MetersPerDegreeLng((minLat + maxLat) / 2);

        var rowCount = Math.Max(1, (int)Math.Ceiling((maxLat - minLat) * MetersPerDegreeLat / swathWidthMeters));
        var rowStep = (maxLat - minLat) / rowCount;

        var random = new Random(seed);
        var startFromNorth = random.Next(2) == 0;
        var eastbound = random.Next(2) == 0;

        var path = new List<GeoPoint>();

        for (var i = 0; i < rowCount; i++)
        {
            var row = startFromNorth ? rowCount - 1 - i : i;
            var lat = minLat + (row + 0.5) * rowStep;

            if (!TryGetRowSpan(boundary, lat, out var west, out var east))
                continue;

            var (from, to) = eastbound ? (west, east) : (east, west);
            var pointCount = Math.Max(2, (int)Math.Ceiling((east - west) * metersPerDegreeLng / pointSpacingMeters) + 1);

            for (var j = 0; j < pointCount; j++)
                path.Add(new GeoPoint(lat, from + (to - from) * j / (pointCount - 1)));

            eastbound = !eastbound;
        }

        return path;
    }

    public static double MetersPerDegreeLng(double lat) => MetersPerDegreeLat * Math.Cos(lat * Math.PI / 180);

    private static bool TryGetRowSpan(IReadOnlyList<GeoPoint> boundary, double lat, out double west, out double east)
    {
        west = double.MaxValue;
        east = double.MinValue;

        for (var i = 0; i < boundary.Count; i++)
        {
            var a = boundary[i];
            var b = boundary[(i + 1) % boundary.Count];

            if (a.Lat <= lat == b.Lat <= lat)
                continue;

            var lng = a.Lng + (lat - a.Lat) / (b.Lat - a.Lat) * (b.Lng - a.Lng);
            west = Math.Min(west, lng);
            east = Math.Max(east, lng);
        }

        return west < east;
    }
}
