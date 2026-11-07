using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Rules;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Rules;

public class SupplyRulesTests : IClassFixture<CatalogFixture>
{
    private static GameState BuildState(IEnumerable<Tile> tiles, int width, int height, params Unit[] units)
    {
        var map = new MapDefinition("test", width, height, tiles);
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

    [Fact]
    public void AvailableSource_when_ground_unit_is_on_friendly_city_returns_building()
    {
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == new HexCoord(1, 0)
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.City, Owner: Side.Blue)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(1, 0));
        var state = BuildState(tiles, 3, 1, tank);

        Assert.Equal(SupplyRules.SupplySource.Building, SupplyRules.AvailableSource(state, tank));
    }

    [Fact]
    public void AvailableSource_when_air_unit_is_on_friendly_airbase_returns_building()
    {
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == new HexCoord(1, 0)
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.AirFactory, Owner: Side.Blue)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var fighter = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Fighter, new HexCoord(1, 0));
        var state = BuildState(tiles, 3, 1, fighter);

        Assert.Equal(SupplyRules.SupplySource.Building, SupplyRules.AvailableSource(state, fighter));
    }

    [Fact]
    public void AvailableSource_when_ground_unit_is_on_airbase_returns_null()
    {
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == new HexCoord(1, 0)
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.AirFactory, Owner: Side.Blue)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(1, 0));
        var state = BuildState(tiles, 3, 1, tank);

        Assert.Null(SupplyRules.AvailableSource(state, tank));
    }

    [Fact]
    public void AvailableSource_when_ground_unit_is_on_neutral_city_returns_building()
    {
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == new HexCoord(1, 0)
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.City, Owner: null)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(1, 0));
        var state = BuildState(tiles, 3, 1, tank);

        Assert.Equal(SupplyRules.SupplySource.Building, SupplyRules.AvailableSource(state, tank));
    }

    [Fact]
    public void AvailableSource_when_ground_unit_is_on_enemy_city_returns_building()
    {
        // Per the original game, a unit refuels/repairs at any city
        // regardless of ownership (and captures it by holding it).
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == new HexCoord(1, 0)
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.City, Owner: Side.Red)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(1, 0));
        var state = BuildState(tiles, 3, 1, tank);

        Assert.Equal(SupplyRules.SupplySource.Building, SupplyRules.AvailableSource(state, tank));
    }

    [Fact]
    public void AvailableSource_when_unit_already_supplied_this_turn_returns_null()
    {
        var tiles = MapDefinition.EnumerateCoords(3, 1)
            .Select(c => c == new HexCoord(1, 0)
                ? new Tile(c, Terrain.Plains, Building: BuildingKind.City, Owner: Side.Blue)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(1, 0)) with { HasSupplied = true };
        var state = BuildState(tiles, 3, 1, tank);

        Assert.Null(SupplyRules.AvailableSource(state, tank));
    }

    [Fact]
    public void ApplySupply_when_source_is_building_refuels_rearms_and_repairs()
    {
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, HexCoord.Zero) with
        {
            Fuel = 1,
            Ammo = 1,
            HitPoints = 4,
        };

        var supplied = SupplyRules.ApplySupply(tank, SupplyRules.SupplySource.Building);

        var stats = TestCatalog.Tank;
        Assert.Equal(stats.MaxFuel, supplied.Fuel);
        Assert.Equal(stats.MaxAmmo, supplied.Ammo);
        Assert.Equal(4 + SupplyRules.RepairAmount, supplied.HitPoints);
        Assert.True(supplied.HasSupplied);
    }

    [Fact]
    public void ApplySupply_when_source_is_vehicle_does_not_repair()
    {
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, HexCoord.Zero) with
        {
            Fuel = 1,
            Ammo = 1,
            HitPoints = 4,
        };

        var supplied = SupplyRules.ApplySupply(tank, SupplyRules.SupplySource.Vehicle);

        Assert.Equal(TestCatalog.Tank.MaxFuel, supplied.Fuel);
        Assert.Equal(4, supplied.HitPoints);
        Assert.True(supplied.HasSupplied);
    }

    [Fact]
    public void ApplySupply_when_repair_would_exceed_max_caps_at_max_hit_points()
    {
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, HexCoord.Zero) with
        {
            HitPoints = UnitStats.MaxHitPoints - 1,
        };

        var supplied = SupplyRules.ApplySupply(tank, SupplyRules.SupplySource.Building);

        Assert.Equal(UnitStats.MaxHitPoints, supplied.HitPoints);
    }

    [Fact]
    public void AvailableSource_when_state_is_null_throws_ArgumentNullException()
    {
        var tank = Unit.FullStrength(new UnitId(1), Side.Blue, TestCatalog.Tank, new HexCoord(0, 0));

        var ex = Assert.Throws<ArgumentNullException>(() => SupplyRules.AvailableSource(null!, tank));

        Assert.Equal("state", ex.ParamName);
    }

    [Fact]
    public void AvailableSource_when_unit_is_null_throws_ArgumentNullException()
    {
        var state = BuildState(
            MapDefinition.EnumerateCoords(1, 1).Select(c => new Tile(c, Terrain.Plains, Building: null, Owner: null)),
            1,
            1);

        var ex = Assert.Throws<ArgumentNullException>(() => SupplyRules.AvailableSource(state, null!));

        Assert.Equal("unit", ex.ParamName);
    }

    [Fact]
    public void ApplySupply_when_unit_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => SupplyRules.ApplySupply(null!, SupplyRules.SupplySource.Building));

        Assert.Equal("unit", ex.ParamName);
    }
}
