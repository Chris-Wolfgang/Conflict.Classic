using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Game;

public class EndTurnTests : IClassFixture<CatalogFixture>
{
    [Fact]
    public async Task EndTurn_when_called_swaps_active_side()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 1);

        var next = engine.EndTurn(state);

        Assert.Equal(Side.Red, next.NextToAct);
        Assert.Equal(1, next.TurnNumber);
    }

    [Fact]
    public async Task EndTurn_when_called_twice_increments_turn_number_for_blue()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 1);

        var afterRed = engine.EndTurn(engine.EndTurn(state));

        Assert.Equal(Side.Blue, afterRed.NextToAct);
        Assert.Equal(2, afterRed.TurnNumber);
    }

    [Fact]
    public async Task EndTurn_when_called_refreshes_per_turn_fields_for_incoming_side()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 1);

        // Mark a Red unit as having moved/attacked/supplied; EndTurn from
        // Blue should still leave Red unchanged. EndTurn from Red should refresh.
        var redUnit = state.Units.Values.First(u => u.Side == Side.Red);
        var modified = state.Units[redUnit.Id] with { HasMoved = true, HasAttacked = true, HasSupplied = true };
        var modifiedUnits = new Dictionary<UnitId, Unit>();
        foreach (var kv in state.Units)
        {
            modifiedUnits[kv.Key] = kv.Key == redUnit.Id ? modified : kv.Value;
        }
        var withRedFlagged = new GameState(
            state.Map, state.Catalog, modifiedUnits, state.BuildingOwners,
            state.NextToAct, state.TurnNumber, state.Phase, state.Funds, state.Winner, state.RandomSeed);

        var afterBlueEnded = engine.EndTurn(withRedFlagged);

        var redAfter = afterBlueEnded.Units[redUnit.Id];
        Assert.False(redAfter.HasMoved);
        Assert.False(redAfter.HasAttacked);
        Assert.False(redAfter.HasSupplied);
    }

    [Fact]
    public async Task EndTurn_when_side_owns_city_and_airbase_grants_income()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 1);

        // Blue ends turn. Red gains income from its airbase. Mission01 has 1
        // Red Airbase (= 100 F.P. per turn) and no Red-owned City at start.
        var after = engine.EndTurn(state);

        // Starting war-chest is 5000; Red's lone Air Factory adds 100 per turn.
        Assert.Equal(5100, after.Funds[Side.Red]);
        Assert.Equal(5000, after.Funds[Side.Blue]);
    }

    [Fact]
    public async Task EndTurn_when_unit_moved_this_turn_drains_one_fuel()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 1);

        // Move Blue's infantry one hex to its east.
        var infantry = state.Units.Values.Single(u => u.Side == Side.Blue && u.Type == TestCatalog.Infantry);
        var fuelBefore = infantry.Fuel;
        state = engine.MoveUnit(state, infantry.Id, new HexCoord(3, 6));
        Assert.Equal(fuelBefore, state.Units[infantry.Id].Fuel); // not yet drained

        state = engine.EndTurn(state); // Blue ends -> drain happens for Blue movers

        Assert.Equal(fuelBefore - 1, state.Units[infantry.Id].Fuel);
    }

    [Fact]
    public async Task EndTurn_when_unit_did_not_move_does_not_drain_fuel()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 1);

        var blueIdle = state.Units.Values.Single(u => u.Side == Side.Blue && u.Type == TestCatalog.Fighter);
        var fuelBefore = blueIdle.Fuel;

        state = engine.EndTurn(state);

        Assert.Equal(fuelBefore, state.Units[blueIdle.Id].Fuel);
    }

    [Fact]
    public async Task EndTurn_when_game_over_throws()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 1);
        var over = new GameState(state.Map, state.Catalog, state.Units, state.BuildingOwners,
            state.NextToAct, state.TurnNumber, GamePhase.GameOver,
            state.Funds, Side.Blue, state.RandomSeed);

        Assert.Throws<InvalidOperationException>(() => engine.EndTurn(over));
    }
}
