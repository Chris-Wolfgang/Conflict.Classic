using Wolfgang.Conflict.Classic.Engine.Hex;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Hex;

public class HexCoordTests
{
    [Fact]
    public void Zero_when_read_is_origin()
    {
        Assert.Equal(new HexCoord(0, 0), HexCoord.Zero);
    }

    [Fact]
    public void S_when_read_is_negative_sum_of_Q_and_R()
    {
        var sut = new HexCoord(3, -2);

        Assert.Equal(-1, sut.S);
    }

    [Fact]
    public void DistanceTo_when_target_is_self_returns_zero()
    {
        var sut = new HexCoord(2, 5);

        Assert.Equal(0, sut.DistanceTo(sut));
    }

    [Theory]
    [InlineData(0, 0, 1, 0, 1)]
    [InlineData(0, 0, 0, 3, 3)]
    [InlineData(0, 0, 3, -3, 3)]
    [InlineData(1, 1, -2, -2, 6)]
    public void DistanceTo_when_coords_are_known_returns_expected_distance(int q1, int r1, int q2, int r2, int expected)
    {
        var a = new HexCoord(q1, r1);
        var b = new HexCoord(q2, r2);

        Assert.Equal(expected, a.DistanceTo(b));
    }

    [Fact]
    public void DistanceTo_when_arguments_are_swapped_returns_same_distance()
    {
        var a = new HexCoord(4, -3);
        var b = new HexCoord(-1, 2);

        Assert.Equal(a.DistanceTo(b), b.DistanceTo(a));
    }

    [Fact]
    public void Neighbors_when_called_returns_six_distinct_hexes_each_at_distance_one()
    {
        var sut = new HexCoord(5, 7);

        var neighbors = sut.Neighbors().ToList();

        Assert.Equal(6, neighbors.Count);
        Assert.Equal(6, neighbors.Distinct().Count());
        Assert.All(neighbors, n => Assert.Equal(1, sut.DistanceTo(n)));
    }

    [Theory]
    [InlineData(HexDirection.East, 1, 0)]
    [InlineData(HexDirection.NorthEast, 1, -1)]
    [InlineData(HexDirection.NorthWest, 0, -1)]
    [InlineData(HexDirection.West, -1, 0)]
    [InlineData(HexDirection.SouthWest, -1, 1)]
    [InlineData(HexDirection.SouthEast, 0, 1)]
    public void Neighbor_when_direction_is_given_applies_axial_offset(HexDirection direction, int dq, int dr)
    {
        var origin = new HexCoord(10, 20);

        var moved = origin.Neighbor(direction);

        Assert.Equal(new HexCoord(10 + dq, 20 + dr), moved);
    }

    [Fact]
    public void WithinRange_when_range_is_zero_returns_only_self()
    {
        var sut = new HexCoord(4, 4);

        var hexes = sut.WithinRange(0).ToList();

        Assert.Single(hexes);
        Assert.Equal(sut, hexes[0]);
    }

    [Theory]
    [InlineData(1, 7)]
    [InlineData(2, 19)]
    [InlineData(3, 37)]
    [InlineData(4, 61)]
    public void WithinRange_when_range_is_n_returns_one_plus_3n_n_plus_one_hexes(int radius, int expectedCount)
    {
        var hexes = HexCoord.Zero.WithinRange(radius).ToList();

        Assert.Equal(expectedCount, hexes.Count);
        Assert.All(hexes, h => Assert.True(HexCoord.Zero.DistanceTo(h) <= radius));
    }

    [Fact]
    public void WithinRange_when_range_is_negative_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HexCoord.Zero.WithinRange(-1).ToList());
    }

    [Fact]
    public void ToString_when_called_formats_axial_pair()
    {
        Assert.Equal("(3,-2)", new HexCoord(3, -2).ToString());
    }
}
