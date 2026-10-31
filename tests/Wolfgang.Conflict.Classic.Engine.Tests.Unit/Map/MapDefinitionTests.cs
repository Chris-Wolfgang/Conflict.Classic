using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Map;

public class MapDefinitionTests
{
    private static IEnumerable<Tile> AllPlains(int width, int height) =>
        MapDefinition.EnumerateCoords(width, height)
            .Select(c => new Tile(c, Terrain.Plains, Building: null, Owner: null));

    [Fact]
    public void Constructor_when_tile_set_is_complete_succeeds()
    {
        var map = new MapDefinition("test", 4, 3, AllPlains(4, 3));

        Assert.Equal("test", map.Name);
        Assert.Equal(4, map.Width);
        Assert.Equal(3, map.Height);
        Assert.Equal(12, map.Tiles.Count);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(2, -1)]
    [InlineData(3, 1)]
    public void Contains_when_coord_is_inside_offset_rectangle_returns_true(int q, int r)
    {
        var map = new MapDefinition("test", 4, 3, AllPlains(4, 3));

        Assert.True(map.Contains(new HexCoord(q, r)));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(4, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 3)]
    [InlineData(2, -2)]
    [InlineData(2, 2)]
    public void Contains_when_coord_is_outside_rectangular_bounds_returns_false(int q, int r)
    {
        var map = new MapDefinition("test", 4, 3, AllPlains(4, 3));

        Assert.False(map.Contains(new HexCoord(q, r)));
    }

    [Fact]
    public void Constructor_when_tile_is_outside_bounds_throws()
    {
        var bad = AllPlains(4, 3).Concat(new[] { new Tile(new HexCoord(5, 0), Terrain.Plains, Building: null, Owner: null) });

        Assert.Throws<ArgumentException>(() => new MapDefinition("test", 4, 3, bad));
    }

    [Fact]
    public void Constructor_when_coords_are_duplicated_throws()
    {
        var dup = AllPlains(4, 3).ToList();
        dup.Add(new Tile(dup[0].Coord, Terrain.Plains, Building: null, Owner: null));

        Assert.Throws<ArgumentException>(() => new MapDefinition("test", 4, 3, dup));
    }

    [Fact]
    public void Constructor_when_coords_are_missing_throws()
    {
        var missing = AllPlains(4, 3).Skip(1);

        Assert.Throws<ArgumentException>(() => new MapDefinition("test", 4, 3, missing));
    }

    [Fact]
    public void Constructor_when_dimensions_are_not_positive_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapDefinition("test", 0, 3, []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapDefinition("test", 4, 0, []));
    }

    [Theory]
    [InlineData(4, 3, 12)]
    [InlineData(12, 10, 120)]
    [InlineData(1, 1, 1)]
    public void EnumerateCoords_when_dimensions_are_given_yields_width_times_height_unique_coords(int width, int height, int expected)
    {
        var coords = MapDefinition.EnumerateCoords(width, height).ToList();

        Assert.Equal(expected, coords.Count);
        Assert.Equal(expected, coords.Distinct().Count());
    }

    [Fact]
    public void Constructor_when_name_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MapDefinition(null!, 1, 1, AllPlains(1, 1)));

        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Constructor_when_tiles_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MapDefinition("test", 1, 1, null!));

        Assert.Equal("tiles", ex.ParamName);
    }

    [Theory]
    [InlineData(0, 3, "width")]
    [InlineData(-1, 3, "width")]
    [InlineData(4, 0, "height")]
    [InlineData(4, -1, "height")]
    public void EnumerateCoords_when_a_dimension_is_not_positive_throws_ArgumentOutOfRangeException(int width, int height, string paramName)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => MapDefinition.EnumerateCoords(width, height).ToList());

        Assert.Equal(paramName, ex.ParamName);
    }
}
