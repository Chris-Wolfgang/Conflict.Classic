using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Rules;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Rules;

public class CaptureRulesTests : IClassFixture<CatalogFixture>
{
    [Theory]
    [InlineData(BuildingKind.City, true)]
    [InlineData(BuildingKind.AirFactory, true)]
    [InlineData(BuildingKind.Port, true)]
    [InlineData(BuildingKind.Hq, false)]
    [InlineData(BuildingKind.LandFactory, false)]
    public void IsCapturable_when_building_kind_is_given_matches_manual_rules(BuildingKind building, bool expected)
    {
        Assert.Equal(expected, CaptureRules.IsCapturable(building));
    }

    [Fact]
    public void ComputeFlips_when_infantry_holds_neutral_city_returns_flip()
    {
        var cityCoord = new HexCoord(1, 0);
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == cityCoord
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.City, Owner: null)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var map = new MapDefinition("city", 3, 1, tiles);
        var infantry = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Infantry, cityCoord);
        var state = new GameState(map, TestCatalog.Catalog,
            new Dictionary<UnitId, Unit> { [infantry.Id] = infantry },
            new Dictionary<HexCoord, Side>(),
            Side.Blue, turnNumber: 1, phase: GamePhase.PlayerTurn,
            new Dictionary<Side, int> { [Side.Blue] = 0, [Side.Red] = 0 },
            winner: null, randomSeed: 1);

        var flips = CaptureRules.ComputeFlips(state, Side.Blue);

        Assert.Single(flips);
        Assert.Equal(Side.Blue, flips[cityCoord]);
    }

    [Fact]
    public void ComputeFlips_when_infantry_already_owns_building_does_not_flip()
    {
        var cityCoord = new HexCoord(1, 0);
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == cityCoord
                ? new Tile(c, Terrain.Plains, BuildingKind.City, Side.Blue)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var map = new MapDefinition("city", 3, 1, tiles);
        var infantry = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Infantry, cityCoord);
        var state = new GameState(map, TestCatalog.Catalog,
            new Dictionary<UnitId, Unit> { [infantry.Id] = infantry },
            new Dictionary<HexCoord, Side>(),
            Side.Blue, turnNumber: 1, phase: GamePhase.PlayerTurn,
            new Dictionary<Side, int> { [Side.Blue] = 0, [Side.Red] = 0 },
            winner: null, randomSeed: 1);

        Assert.Empty(CaptureRules.ComputeFlips(state, Side.Blue));
    }

    [Fact]
    public void ComputeFlips_when_any_unit_holds_a_city_captures_it()
    {
        // Per the original game, any unit captures — not just infantry.
        var cityCoord = new HexCoord(1, 0);
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == cityCoord
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.City, Owner: null)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var map = new MapDefinition("city", 3, 1, tiles);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, cityCoord);
        var state = new GameState(map, TestCatalog.Catalog,
            new Dictionary<UnitId, Unit> { [tank.Id] = tank },
            new Dictionary<HexCoord, Side>(),
            Side.Blue, turnNumber: 1, phase: GamePhase.PlayerTurn,
            new Dictionary<Side, int> { [Side.Blue] = 0, [Side.Red] = 0 },
            winner: null, randomSeed: 1);

        var flips = CaptureRules.ComputeFlips(state, Side.Blue);

        Assert.Single(flips);
        Assert.Equal(Side.Blue, flips[cityCoord]);
    }

    [Fact]
    public void ComputeFlips_when_state_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => CaptureRules.ComputeFlips(null!, Side.Blue));

        Assert.Equal("state", ex.ParamName);
    }
}
