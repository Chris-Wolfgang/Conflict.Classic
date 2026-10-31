using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.UI.Blazor.Rendering;
using Xunit;

namespace Wolfgang.Conflict.Classic.UI.Blazor.Tests.Unit;

public class TerrainColorsTests
{
    [Theory]
    [InlineData(Terrain.Plains, NesPalette.Plains)]
    [InlineData(Terrain.Forest, NesPalette.Forest)]
    [InlineData(Terrain.Mountain, NesPalette.Mountain)]
    [InlineData(Terrain.River, NesPalette.River)]
    [InlineData(Terrain.Bridge, NesPalette.Bridge)]
    [InlineData(Terrain.Shoal, NesPalette.Shoal)]
    [InlineData(Terrain.Sea, NesPalette.Sea)]
    [InlineData(Terrain.Reef, NesPalette.Reef)]
    [InlineData((Terrain)99, NesPalette.Background)]
    public void FillFor_when_terrain_is_given_returns_its_palette_fill(Terrain terrain, string expected)
    {
        Assert.Equal(expected, TerrainColors.FillFor(terrain));
    }

    [Theory]
    [InlineData(Terrain.Plains, NesPalette.PlainsDark)]
    [InlineData(Terrain.Forest, NesPalette.ForestDark)]
    [InlineData(Terrain.Mountain, NesPalette.MountainDark)]
    [InlineData(Terrain.Sea, NesPalette.Grid)]
    public void StrokeFor_when_terrain_is_given_returns_its_outline_colour(Terrain terrain, string expected)
    {
        Assert.Equal(expected, TerrainColors.StrokeFor(terrain));
    }
}
