using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Game;

public class GameEngineGuardTests : IClassFixture<CatalogFixture>
{
    private static readonly HexCoord AttackerHex = new(2, 1);
    private static readonly HexCoord FactoryHex = new(3, 1);
    private static readonly UnitId Missing = new(99);

    private static GameState State(
        GamePhase phase = GamePhase.PlayerTurn,
        IReadOnlyDictionary<HexCoord, int>? buildingHitPoints = null,
        params Unit[] units)
    {
        var tiles = MapDefinition.EnumerateCoords(10, 5)
            .Select(c => c == FactoryHex
                ? new Tile(c, Terrain.Plains, BuildingKind.LandFactory, Side.Red)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        return new GameState(
            new MapDefinition("engine-guards", 10, 5, tiles),
            TestCatalog.Catalog,
            units.ToDictionary(u => u.Id),
            new Dictionary<HexCoord, Side>(),
            Side.Blue,
            turnNumber: 1,
            phase: phase,
            funds: new Dictionary<Side, int> { [Side.Blue] = 0, [Side.Red] = 0 },
            winner: null,
            randomSeed: 7,
            buildingHitPoints: buildingHitPoints);
    }

    private static Unit Tank(int id, Side side, HexCoord coord)
        => Unit.FullStrength(new UnitId(id), side, TestCatalog.Tank, coord);

    [Fact]
    public void GetLegalMoves_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().GetLegalMoves(null!, Missing));
    }

    [Fact]
    public void GetLegalMoves_when_unit_does_not_exist_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new GameEngine().GetLegalMoves(State(), Missing));
    }

    [Fact]
    public void MoveUnit_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().MoveUnit(null!, Missing, AttackerHex));
    }

    [Fact]
    public void MoveUnit_when_game_is_over_throws_InvalidOperationException()
    {
        var tank = Tank(1, Side.Blue, AttackerHex);
        var state = State(GamePhase.GameOver, buildingHitPoints: null, tank);

        Assert.Throws<InvalidOperationException>(() => new GameEngine().MoveUnit(state, tank.Id, new HexCoord(2, 2)));
    }

    [Fact]
    public void GetLegalTargets_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().GetLegalTargets(null!, Missing));
    }

    [Fact]
    public void GetLegalTargets_when_attacker_does_not_exist_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new GameEngine().GetLegalTargets(State(), Missing));
    }

    [Fact]
    public void GetLegalTargets_when_enemy_is_adjacent_returns_its_id()
    {
        var atk = Tank(1, Side.Blue, new HexCoord(0, 0));
        var def = Unit.FullStrength(new UnitId(2), Side.Red, TestCatalog.Infantry, new HexCoord(1, 0));
        var state = State(GamePhase.PlayerTurn, buildingHitPoints: null, atk, def);

        var targets = new GameEngine().GetLegalTargets(state, atk.Id);

        Assert.Equal(new[] { def.Id }, targets);
    }

    [Fact]
    public void AttackUnit_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().AttackUnit(null!, Missing, Missing));
    }

    [Fact]
    public void AttackUnit_when_game_is_over_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new GameEngine().AttackUnit(State(GamePhase.GameOver), Missing, Missing));
    }

    [Fact]
    public void AttackUnit_when_attacker_does_not_exist_throws_InvalidOperationException()
    {
        var def = Tank(2, Side.Red, new HexCoord(1, 0));
        var state = State(GamePhase.PlayerTurn, buildingHitPoints: null, def);

        Assert.Throws<InvalidOperationException>(() => new GameEngine().AttackUnit(state, Missing, def.Id));
    }

    [Fact]
    public void AttackUnit_when_defender_does_not_exist_throws_InvalidOperationException()
    {
        var atk = Tank(1, Side.Blue, new HexCoord(0, 0));
        var state = State(GamePhase.PlayerTurn, buildingHitPoints: null, atk);

        Assert.Throws<InvalidOperationException>(() => new GameEngine().AttackUnit(state, atk.Id, Missing));
    }

    [Fact]
    public void AttackBuilding_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().AttackBuilding(null!, Missing, FactoryHex));
    }

    [Fact]
    public void AttackBuilding_when_game_is_over_throws_InvalidOperationException()
    {
        var atk = Tank(1, Side.Blue, AttackerHex);
        var state = State(GamePhase.GameOver, buildingHitPoints: null, atk);

        Assert.Throws<InvalidOperationException>(() => new GameEngine().AttackBuilding(state, atk.Id, FactoryHex));
    }

    [Fact]
    public void AttackBuilding_when_attacker_does_not_exist_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new GameEngine().AttackBuilding(State(), Missing, FactoryHex));
    }

    [Fact]
    public void AttackBuilding_when_it_is_not_the_attackers_turn_throws_InvalidOperationException()
    {
        var atk = Tank(1, Side.Red, AttackerHex);
        var state = State(GamePhase.PlayerTurn, buildingHitPoints: null, atk);

        Assert.Throws<InvalidOperationException>(() => new GameEngine().AttackBuilding(state, atk.Id, FactoryHex));
    }

    [Fact]
    public void AttackBuilding_when_building_is_not_adjacent_throws_InvalidOperationException()
    {
        var atk = Tank(1, Side.Blue, new HexCoord(0, 0));
        var state = State(GamePhase.PlayerTurn, buildingHitPoints: null, atk);

        Assert.Throws<InvalidOperationException>(() => new GameEngine().AttackBuilding(state, atk.Id, FactoryHex));
    }

    [Fact]
    public void AttackBuilding_when_strike_is_not_fatal_spends_ammo_and_building_self_repairs_to_full()
    {
        var atk = Tank(1, Side.Blue, AttackerHex);
        var state = State(GamePhase.PlayerTurn, buildingHitPoints: null, atk);

        var next = new GameEngine().AttackBuilding(state, atk.Id, FactoryHex);

        Assert.True(next.Units[atk.Id].HasAttacked);
        Assert.Equal(atk.Ammo - 1, next.Units[atk.Id].Ammo);
        // Factories self-repair: only a one-shot kill persists damage (GameEngine.UpdateBuildingHits).
        Assert.Equal(GameState.MaxBuildingHitPoints, next.GetBuildingHitPoints(FactoryHex));
        Assert.NotEqual(state.RandomSeed, next.RandomSeed);
        Assert.Equal(0, next.Funds[Side.Blue]);
    }

    [Fact]
    public void AttackBuilding_when_building_is_destroyed_awards_the_attacker_500()
    {
        var atk = Tank(1, Side.Blue, AttackerHex);
        var state = State(GamePhase.PlayerTurn, buildingHitPoints: new Dictionary<HexCoord, int> { [FactoryHex] = 1 }, atk);

        var next = new GameEngine().AttackBuilding(state, atk.Id, FactoryHex);

        Assert.Equal(0, next.GetBuildingHitPoints(FactoryHex));
        Assert.Equal(500, next.Funds[Side.Blue]);
        Assert.Equal(0, next.Funds[Side.Red]);
    }

    [Fact]
    public void BuildUnit_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().BuildUnit(null!, FactoryHex, "m1a1"));
    }

    [Fact]
    public void BuildUnit_when_game_is_over_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new GameEngine().BuildUnit(State(GamePhase.GameOver), FactoryHex, "m1a1"));
    }

    [Fact]
    public void EndTurn_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().EndTurn(null!));
    }

    [Fact]
    public void SupplyUnit_when_state_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("state", () => new GameEngine().SupplyUnit(null!, Missing));
    }

    [Fact]
    public void SupplyUnit_when_game_is_over_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new GameEngine().SupplyUnit(State(GamePhase.GameOver), Missing));
    }

    [Fact]
    public void SupplyUnit_when_unit_does_not_exist_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new GameEngine().SupplyUnit(State(), Missing));
    }
}
