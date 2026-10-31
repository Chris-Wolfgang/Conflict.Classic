using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Rules;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Rules;

public class MovementRulesTests : IClassFixture<CatalogFixture>
{
    private static GameState MakeState(MapDefinition map, params Unit[] units)
    {
        var dict = new Dictionary<UnitId, Unit>();
        foreach (var u in units)
        {
            dict[u.Id] = u;
        }
        return new GameState(
            map,
            TestCatalog.Catalog,
            dict,
            new Dictionary<HexCoord, Side>(),
            Side.Blue,
            turnNumber: 1,
            phase: GamePhase.PlayerTurn,
            funds: new Dictionary<Side, int> { [Side.Blue] = 0, [Side.Red] = 0 },
            winner: null,
            randomSeed: 1);
    }

    private static MapDefinition AllPlains(int width, int height) =>
        new("plains", width, height,
            MapDefinition.EnumerateCoords(width, height)
                .Select(c => new Tile(c, Terrain.Plains, Building: null, Owner: null)));

    [Fact]
    public void GetReachable_when_map_is_open_plains_returns_full_radius()
    {
        var map = AllPlains(10, 5);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(4, 1));
        var state = MakeState(map, tank);

        var reachable = MovementRules.GetReachable(state, tank);

        // Tank moves 5 hexes; expect all hexes within 5 steps (minus current hex, minus off-map).
        var unrestrictedCount = tank.Coord.WithinRange(5).Count(c => map.Contains(c)) - 1;
        Assert.Equal(unrestrictedCount, reachable.Count);
        Assert.DoesNotContain(tank.Coord, reachable);
    }

    [Fact]
    public void GetReachable_when_other_units_block_hexes_excludes_them()
    {
        var map = AllPlains(6, 3);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(0, 0));
        var blocker = Unit.FullStrength(new UnitId(2), Side.Red, TestCatalog.Infantry, new HexCoord(1, 0));
        var state = MakeState(map, tank, blocker);

        var reachable = MovementRules.GetReachable(state, tank);

        Assert.DoesNotContain(new HexCoord(1, 0), reachable);
    }

    [Fact]
    public void FindPath_when_destination_is_over_budget_returns_null()
    {
        var map = AllPlains(20, 3);
        var infantry = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Infantry, new HexCoord(0, 0));
        var state = MakeState(map, infantry);

        // Infantry MOVING is 4 - cannot reach (10, 0) in one turn.
        var path = MovementRules.FindPath(state, infantry, new HexCoord(10, 0));

        Assert.Null(path);
    }

    [Fact]
    public void FindPath_when_destination_is_adjacent_plain_costs_one()
    {
        var map = AllPlains(6, 3);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(2, 1));
        var state = MakeState(map, tank);

        var path = MovementRules.FindPath(state, tank, new HexCoord(3, 1));

        Assert.NotNull(path);
        Assert.Equal(1, path!.TotalCost);
    }

    [Fact]
    public void FindPath_when_route_crosses_forest_costs_two_per_hex()
    {
        var tiles = MapDefinition.EnumerateCoords(6, 3)
            .Select(c => new Tile(c, c.Q == 3 ? Terrain.Forest : Terrain.Plains, Building: null, Owner: null));
        var map = new MapDefinition("forest", 6, 3, tiles);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(2, 1));
        var state = MakeState(map, tank);

        var path = MovementRules.FindPath(state, tank, new HexCoord(3, 1));

        Assert.NotNull(path);
        Assert.Equal(2, path!.TotalCost);
    }

    [Fact]
    public void GetReachable_when_unit_has_no_moves_remaining_returns_empty()
    {
        var map = AllPlains(6, 3);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(2, 1)) with { MovesRemaining = 0 };
        var state = MakeState(map, tank);

        Assert.Empty(MovementRules.GetReachable(state, tank));
    }

    [Fact]
    public void GetReachable_when_unit_has_zero_fuel_returns_empty()
    {
        // Fuel is a per-turn counter: 0 = can't move this turn at all.
        var map = AllPlains(6, 3);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(2, 1)) with { Fuel = 0 };
        var state = MakeState(map, tank);

        Assert.Empty(MovementRules.GetReachable(state, tank));
    }

    [Fact]
    public void GetReachable_when_fuel_is_one_returns_full_range()
    {
        // One unit of fuel buys an entire turn of movement; the budget is
        // MovesRemaining, not Fuel.
        var map = AllPlains(10, 5);
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(2, 1)) with { Fuel = 1 };
        var state = MakeState(map, tank);

        var reachable = MovementRules.GetReachable(state, tank);

        // Tank moves 5 hexes; on 10x5 open plains from (2,1) that reaches every
        // in-bounds hex within 5 steps except its own: 39 hexes.
        Assert.Equal(39, reachable.Count);
    }

    [Fact]
    public void FindPath_when_state_is_null_throws_ArgumentNullException()
    {
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(0, 0));

        var ex = Assert.Throws<ArgumentNullException>(() => MovementRules.FindPath(null!, tank, new HexCoord(1, 0)));

        Assert.Equal("state", ex.ParamName);
    }

    [Fact]
    public void FindPath_when_unit_is_null_throws_ArgumentNullException()
    {
        var state = MakeState(AllPlains(2, 1));

        var ex = Assert.Throws<ArgumentNullException>(() => MovementRules.FindPath(state, null!, new HexCoord(1, 0)));

        Assert.Equal("unit", ex.ParamName);
    }

    [Fact]
    public void GetReachable_when_state_is_null_throws_ArgumentNullException()
    {
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(0, 0));

        var ex = Assert.Throws<ArgumentNullException>(() => MovementRules.GetReachable(null!, tank));

        Assert.Equal("state", ex.ParamName);
    }

    [Fact]
    public void GetReachable_when_unit_is_null_throws_ArgumentNullException()
    {
        var state = MakeState(AllPlains(2, 1));

        var ex = Assert.Throws<ArgumentNullException>(() => MovementRules.GetReachable(state, null!));

        Assert.Equal("unit", ex.ParamName);
    }
}
