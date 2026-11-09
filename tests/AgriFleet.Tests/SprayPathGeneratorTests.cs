using AgriFleet.Core.Services;
using AgriFleet.Core.Simulation;
using FluentAssertions;

namespace AgriFleet.Tests;

public class SprayPathGeneratorTests
{
    private const double SwathMeters = 60;
    private const double SpacingMeters = 25;
    private const int ExpectedRows = 13;

    private static readonly GeoPoint[] Field =
    [
        new(36.925, 34.905),
        new(36.931, 34.906),
        new(36.930, 34.914),
        new(36.924, 34.913),
    ];

    private static IReadOnlyList<GeoPoint> Generate(int seed = 7) =>
        SprayPathGenerator.Generate(Field, SwathMeters, SpacingMeters, seed);

    private static List<List<GeoPoint>> Rows(IReadOnlyList<GeoPoint> path) =>
        path.GroupBy(p => p.Lat).Select(row => row.ToList()).ToList();

    [Fact]
    public void Path_stays_inside_the_bounding_box()
    {
        var (minLat, maxLat) = (Field.Min(p => p.Lat), Field.Max(p => p.Lat));
        var (minLng, maxLng) = (Field.Min(p => p.Lng), Field.Max(p => p.Lng));

        Generate().Should().OnlyContain(p => p.Lat >= minLat && p.Lat <= maxLat && p.Lng >= minLng && p.Lng <= maxLng);
    }

    [Fact]
    public void Path_is_clipped_to_the_convex_polygon()
    {
        Generate().Should().OnlyContain(p => IsInsideConvex(Field, p));
    }

    [Fact]
    public void Path_covers_one_row_per_swath()
    {
        Rows(Generate()).Should().HaveCount(ExpectedRows);
    }

    [Fact]
    public void Rows_are_evenly_spaced()
    {
        var latitudes = Rows(Generate()).Select(row => row[0].Lat).OrderBy(lat => lat).ToList();
        var gaps = latitudes.Zip(latitudes.Skip(1), (a, b) => b - a).ToList();

        gaps.Should().OnlyContain(gap => Math.Abs(gap - gaps[0]) < 1e-9);
    }

    [Fact]
    public void Consecutive_rows_alternate_direction()
    {
        var directions = Rows(Generate()).Select(row => Math.Sign(row[^1].Lng - row[0].Lng)).ToList();

        directions.Zip(directions.Skip(1)).Should().OnlyContain(pair => pair.First == -pair.Second);
    }

    [Fact]
    public void Points_within_a_row_respect_the_requested_spacing()
    {
        foreach (var row in Rows(Generate()))
        {
            row.Zip(row.Skip(1), FlightPhysics.DistanceMeters).Should().OnlyContain(meters => meters <= SpacingMeters + 0.001);
        }
    }

    [Fact]
    public void Same_seed_produces_the_same_path()
    {
        Generate(seed: 42).Should().Equal(Generate(seed: 42));
    }

    [Fact]
    public void Seeds_select_among_the_four_starting_corners()
    {
        Enumerable.Range(0, 40).Select(seed => Generate(seed)[0]).Distinct().Should().HaveCount(4);
    }

    [Fact]
    public void Boundary_with_fewer_than_three_points_is_rejected()
    {
        var act = () => SprayPathGenerator.Generate(Field.Take(2).ToArray(), SwathMeters, SpacingMeters, 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, 25)]
    [InlineData(60, 0)]
    public void Non_positive_distances_are_rejected(double swath, double spacing)
    {
        var act = () => SprayPathGenerator.Generate(Field, swath, spacing, 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static bool IsInsideConvex(GeoPoint[] polygon, GeoPoint point)
    {
        const double tolerance = 1e-10;

        var crossProducts = polygon.Zip(polygon.Skip(1).Append(polygon[0]), (a, b) =>
            (b.Lng - a.Lng) * (point.Lat - a.Lat) - (b.Lat - a.Lat) * (point.Lng - a.Lng)).ToList();

        return crossProducts.All(c => c >= -tolerance) || crossProducts.All(c => c <= tolerance);
    }
}
