using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Hex;
using Wolfgang.Conflict.Classic.Engine.Map;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Rules;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Rules;

public class AttackRulesTests
{
    private static Unit Make(int id, Side side, UnitTypeDefinition type, HexCoord coord, bool hasAttacked = false)
        => Unit.FullStrength(new UnitId(id), side, type, coord) with { HasAttacked = hasAttacked };

    [Fact]
    public void CanAttack_adjacent_enemy_with_positive_damage_is_legal()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank,     new HexCoord(0, 0));
        var def = Make(2, Side.Red,  TestCatalog.Infantry, new HexCoord(1, 0));

        Assert.True(AttackRules.CanAttack(atk, def));
    }

    [Fact]
    public void CanAttack_same_side_is_illegal()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank,     new HexCoord(0, 0));
        var def = Make(2, Side.Blue, TestCatalog.Infantry, new HexCoord(1, 0));

        Assert.False(AttackRules.CanAttack(atk, def));
    }

    [Fact]
    public void CanAttack_non_adjacent_is_illegal()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank,     new HexCoord(0, 0));
        var def = Make(2, Side.Red,  TestCatalog.Infantry, new HexCoord(3, 0));

        Assert.False(AttackRules.CanAttack(atk, def));
    }

    [Fact]
    public void CanAttack_already_attacked_is_illegal()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank, new HexCoord(0, 0), hasAttacked: true);
        var def = Make(2, Side.Red,  TestCatalog.Tank, new HexCoord(1, 0));

        Assert.False(AttackRules.CanAttack(atk, def));
    }

    [Fact]
    public void CanAttack_allows_every_adjacent_cross_side_pair()
    {
        // Every unit has a machine gun, so even a Battle Tank may take a
        // (poor) shot at an aircraft. The matchup tier handles effectiveness.
        var atk = Make(1, Side.Blue, TestCatalog.Tank,    new HexCoord(0, 0));
        var def = Make(2, Side.Red,  TestCatalog.Fighter, new HexCoord(1, 0));

        Assert.True(AttackRules.CanAttack(atk, def));
    }

    private static readonly HexCoord AttackerHex = new(2, 1);
    private static readonly HexCoord AdjacentHex = new(3, 1);
    private static readonly HexCoord FarHex = new(6, 1);

    private static GameState BuildingState(
        IReadOnlyDictionary<HexCoord, (BuildingKind Kind, Side? Owner)> buildings,
        IReadOnlyDictionary<HexCoord, int>? buildingHitPoints = null,
        params Unit[] units)
    {
        var tiles = MapDefinition.EnumerateCoords(10, 5)
            .Select(c => buildings.TryGetValue(c, out var b)
                ? new Tile(c, Terrain.Plains, b.Kind, b.Owner)
                : new Tile(c, Terrain.Plains, Building: null, Owner: null));
        var map = new MapDefinition("attack-rules", 10, 5, tiles);

        return new GameState(
            map,
            TestCatalog.Catalog,
            units.ToDictionary(u => u.Id),
            new Dictionary<HexCoord, Side>(),
            Side.Blue,
            turnNumber: 1,
            phase: GamePhase.PlayerTurn,
            funds: new Dictionary<Side, int> { [Side.Blue] = 0, [Side.Red] = 0 },
            winner: null,
            randomSeed: 1,
            buildingHitPoints: buildingHitPoints);
    }

    private static GameState OneBuildingState(HexCoord coord, BuildingKind kind, Side? owner, IReadOnlyDictionary<HexCoord, int>? buildingHitPoints = null)
        => BuildingState(new Dictionary<HexCoord, (BuildingKind, Side?)> { [coord] = (kind, owner) }, buildingHitPoints);

    [Fact]
    public void CanAttack_when_attacker_is_null_throws_ArgumentNullException()
    {
        var def = Make(2, Side.Red, TestCatalog.Infantry, new HexCoord(1, 0));

        Assert.Throws<ArgumentNullException>("attacker", () => AttackRules.CanAttack(null!, def));
    }

    [Fact]
    public void CanAttack_when_defender_is_null_throws_ArgumentNullException()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank, new HexCoord(0, 0));

        Assert.Throws<ArgumentNullException>("defender", () => AttackRules.CanAttack(atk, null!));
    }

    [Fact]
    public void CanAttackBuilding_when_state_is_null_throws_ArgumentNullException()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.Throws<ArgumentNullException>("state", () => AttackRules.CanAttackBuilding(null!, atk, AdjacentHex));
    }

    [Fact]
    public void CanAttackBuilding_when_attacker_is_null_throws_ArgumentNullException()
    {
        var state = OneBuildingState(AdjacentHex, BuildingKind.LandFactory, Side.Red);

        Assert.Throws<ArgumentNullException>("attacker", () => AttackRules.CanAttackBuilding(state, null!, AdjacentHex));
    }

    [Fact]
    public void CanAttackBuilding_when_adjacent_enemy_factory_is_intact_returns_true()
    {
        var state = OneBuildingState(AdjacentHex, BuildingKind.LandFactory, Side.Red);
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.True(AttackRules.CanAttackBuilding(state, atk, AdjacentHex));
    }

    [Fact]
    public void CanAttackBuilding_when_adjacent_factory_is_neutral_returns_true()
    {
        var state = OneBuildingState(AdjacentHex, BuildingKind.LandFactory, owner: null);
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.True(AttackRules.CanAttackBuilding(state, atk, AdjacentHex));
    }

    [Fact]
    public void CanAttackBuilding_when_attacker_has_attacked_returns_false()
    {
        var state = OneBuildingState(AdjacentHex, BuildingKind.LandFactory, Side.Red);
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex, hasAttacked: true);

        Assert.False(AttackRules.CanAttackBuilding(state, atk, AdjacentHex));
    }

    [Fact]
    public void CanAttackBuilding_when_building_is_not_adjacent_returns_false()
    {
        var state = OneBuildingState(FarHex, BuildingKind.LandFactory, Side.Red);
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.False(AttackRules.CanAttackBuilding(state, atk, FarHex));
    }

    [Fact]
    public void CanAttackBuilding_when_hex_has_no_building_returns_false()
    {
        var state = OneBuildingState(FarHex, BuildingKind.LandFactory, Side.Red);
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.False(AttackRules.CanAttackBuilding(state, atk, AdjacentHex));
    }

    [Fact]
    public void CanAttackBuilding_when_building_is_destroyed_returns_false()
    {
        var state = OneBuildingState
        (
            AdjacentHex,
            BuildingKind.LandFactory,
            Side.Red,
            new Dictionary<HexCoord, int> { [AdjacentHex] = 0 }
        );
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.False(AttackRules.CanAttackBuilding(state, atk, AdjacentHex));
    }

    [Fact]
    public void CanAttackBuilding_when_building_belongs_to_attacker_returns_false()
    {
        var state = OneBuildingState(AdjacentHex, BuildingKind.LandFactory, Side.Blue);
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.False(AttackRules.CanAttackBuilding(state, atk, AdjacentHex));
    }

    [Theory]
    [InlineData(BuildingKind.City)]
    [InlineData(BuildingKind.AirFactory)]
    [InlineData(BuildingKind.Port)]
    public void CanAttackBuilding_when_building_is_capturable_returns_false(BuildingKind kind)
    {
        var state = OneBuildingState(AdjacentHex, kind, Side.Red);
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.False(AttackRules.CanAttackBuilding(state, atk, AdjacentHex));
    }

    [Fact]
    public void GetAttackableBuildings_when_state_is_null_throws_ArgumentNullException()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.Throws<ArgumentNullException>("state", () => AttackRules.GetAttackableBuildings(null!, atk));
    }

    [Fact]
    public void GetAttackableBuildings_when_attacker_is_null_throws_ArgumentNullException()
    {
        var state = OneBuildingState(AdjacentHex, BuildingKind.LandFactory, Side.Red);

        Assert.Throws<ArgumentNullException>("attacker", () => AttackRules.GetAttackableBuildings(state, null!));
    }

    [Fact]
    public void GetAttackableBuildings_when_adjacent_factory_adjacent_city_and_distant_factory_exist_returns_only_the_adjacent_factory()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);
        var state = BuildingState(new Dictionary<HexCoord, (BuildingKind, Side?)>
        {
            [AdjacentHex] = (BuildingKind.LandFactory, Side.Red),
            [new HexCoord(1, 1)] = (BuildingKind.City, Side.Red),
            [FarHex] = (BuildingKind.LandFactory, Side.Red),
        });

        var result = AttackRules.GetAttackableBuildings(state, atk);

        Assert.Equal(new[] { AdjacentHex }, result);
    }

    [Fact]
    public void GetLegalTargets_when_state_is_null_throws_ArgumentNullException()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);

        Assert.Throws<ArgumentNullException>("state", () => AttackRules.GetLegalTargets(null!, atk));
    }

    [Fact]
    public void GetLegalTargets_when_attacker_is_null_throws_ArgumentNullException()
    {
        var state = BuildingState(new Dictionary<HexCoord, (BuildingKind, Side?)>());

        Assert.Throws<ArgumentNullException>("attacker", () => AttackRules.GetLegalTargets(state, null!));
    }

    [Fact]
    public void GetLegalTargets_when_friend_adjacent_enemy_and_distant_enemy_exist_returns_only_the_adjacent_enemy()
    {
        var atk = Make(1, Side.Blue, TestCatalog.Tank, AttackerHex);
        var friend = Make(2, Side.Blue, TestCatalog.Infantry, new HexCoord(1, 1));
        var adjacentEnemy = Make(3, Side.Red, TestCatalog.Infantry, AdjacentHex);
        var distantEnemy = Make(4, Side.Red, TestCatalog.Infantry, FarHex);
        var state = BuildingState
        (
            new Dictionary<HexCoord, (BuildingKind, Side?)>(),
            buildingHitPoints: null,
            atk,
            friend,
            adjacentEnemy,
            distantEnemy
        );

        var result = AttackRules.GetLegalTargets(state, atk);

        Assert.Equal(new[] { adjacentEnemy.Id }, result);
    }
}
