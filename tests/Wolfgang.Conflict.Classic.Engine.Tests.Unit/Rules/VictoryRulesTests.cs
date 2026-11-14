using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Rules;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Rules;

public class VictoryRulesTests : IClassFixture<CatalogFixture>
{
    private static GameState StateWith(params Unit[] units)
    {
        var map = new MapDefinition("plains", 4, 2,
            MapDefinition.EnumerateCoords(4, 2)
                .Select(c => new Tile(c, Terrain.Plains, Building: null, Owner: null)));
        var dict = new Dictionary<UnitId, Unit>();
        foreach (var u in units)
        {
            dict[u.Id] = u;
        }
        return new GameState(map, TestCatalog.Catalog, dict,
            new Dictionary<HexCoord, Side>(),
            Side.Blue, turnNumber: 1, phase: GamePhase.PlayerTurn,
            new Dictionary<Side, int> { [Side.Blue] = 0, [Side.Red] = 0 },
            winner: null, randomSeed: 1);
    }

    [Fact]
    public void DetermineWinner_when_both_commanders_are_alive_returns_no_winner()
    {
        var blue = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(0, 0), isCommander: true);
        var red = Unit.FullStrength(new UnitId(2), Side.Red, TestCatalog.Tank, new HexCoord(3, 0), isCommander: true);

        Assert.Null(VictoryRules.DetermineWinner(StateWith(blue, red)));
    }

    [Fact]
    public void DetermineWinner_when_red_commander_is_dead_returns_blue()
    {
        var blue = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(0, 0), isCommander: true);
        // Red has only a non-commander unit alive.
        var redGrunt = Unit.FullStrength(new UnitId(2), Side.Red, TestCatalog.Infantry, new HexCoord(3, 0));

        Assert.Equal(Side.Blue, VictoryRules.DetermineWinner(StateWith(blue, redGrunt)));
    }

    [Fact]
    public void DetermineWinner_when_blue_is_routed_returns_red()
    {
        var red = Unit.FullStrength(new UnitId(1), Side.Red, TestCatalog.Tank, new HexCoord(0, 0), isCommander: true);

        Assert.Equal(Side.Red, VictoryRules.DetermineWinner(StateWith(red)));
    }

    [Fact]
    public void DetermineWinner_when_state_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => VictoryRules.DetermineWinner(null!));

        Assert.Equal("state", ex.ParamName);
    }
}
