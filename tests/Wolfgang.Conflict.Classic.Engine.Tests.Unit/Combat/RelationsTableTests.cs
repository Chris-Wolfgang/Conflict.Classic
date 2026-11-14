using Wolfgang.Conflict.Classic.Engine.Combat;
using Wolfgang.Conflict.Classic.Engine.Units;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Combat;

public class RelationsTableTests
{
    // Every unit carries at least a machine gun, so every pair can engage —
    // effectiveness is governed by the matchup tier, not by an engagement gate.
    [Theory]
    [InlineData(UnitCategory.Fighter, UnitCategory.Helicopter)]
    [InlineData(UnitCategory.FlakPanzer, UnitCategory.Fighter)]
    [InlineData(UnitCategory.BattleTank, UnitCategory.Infantry)]
    [InlineData(UnitCategory.BattleTank, UnitCategory.Fighter)]
    [InlineData(UnitCategory.Infantry, UnitCategory.Helicopter)]
    [InlineData(UnitCategory.Commando, UnitCategory.Helicopter)]
    [InlineData(UnitCategory.Helicopter, UnitCategory.Fighter)]
    [InlineData(UnitCategory.SupplyVehicle, UnitCategory.Infantry)]
    [InlineData(UnitCategory.SupplyPlane, UnitCategory.Fighter)]
    [InlineData(UnitCategory.Attacker, UnitCategory.Helicopter)]
    public void CanEngage_when_any_category_pair_is_given_returns_true(UnitCategory attacker, UnitCategory defender)
    {
        // CanEngage is constant true by design: every unit carries at least a
        // machine gun, so any pairing may engage and the matchup tier decides
        // effectiveness. Kept deliberately: it fails if an engagement gate is
        // ever reintroduced without updating this doctrine.
        Assert.True(RelationsTable.CanEngage(attacker, defender));
    }

    [Theory]
    [InlineData(UnitCategory.BattleTank, UnitCategory.Infantry, MatchupTier.TotalVictory)]
    [InlineData(UnitCategory.Fighter, UnitCategory.Helicopter, MatchupTier.TotalVictory)]
    [InlineData(UnitCategory.Fighter, UnitCategory.Fighter, MatchupTier.Equal)]
    [InlineData(UnitCategory.BattleMissileLauncher, UnitCategory.BattleTank, MatchupTier.AtAdvantage)]
    [InlineData(UnitCategory.Infantry, UnitCategory.BattleTank, MatchupTier.CompleteDefeat)]
    public void Outlook_when_category_pair_is_given_returns_designed_tier(UnitCategory attacker, UnitCategory defender, MatchupTier expected)
    {
        Assert.Equal(expected, RelationsTable.Outlook(attacker, defender));
    }

    [Fact]
    public void BaseAttack_when_any_category_pair_is_given_returns_positive_value()
    {
        // Every unit carries at least a machine gun, so every matchup yields
        // a positive (if sometimes very small) base attack value.
        var categories = (UnitCategory[])Enum.GetValues(typeof(UnitCategory));
        foreach (var atk in categories)
        {
            foreach (var def in categories)
            {
                Assert.True(RelationsTable.BaseAttack(atk, def) > 0,
                    $"BaseAttack({atk}, {def}) should be > 0");
            }
        }
    }

    [Fact]
    public void BaseAttack_when_matchup_is_strong_returns_more_than_weak_matchup()
    {
        var strong = RelationsTable.BaseAttack(UnitCategory.BattleTank, UnitCategory.Infantry);
        var weak = RelationsTable.BaseAttack(UnitCategory.Infantry, UnitCategory.Fighter);
        Assert.True(strong > weak);
    }

    [Theory]
    [InlineData(UnitCategory.Fighter, true)]
    [InlineData(UnitCategory.Helicopter, true)]
    [InlineData(UnitCategory.Attacker, true)]
    [InlineData(UnitCategory.SupplyPlane, true)]
    [InlineData(UnitCategory.BattleTank, false)]
    [InlineData(UnitCategory.Infantry, false)]
    public void IsAir_when_category_is_given_returns_whether_it_is_air(UnitCategory category, bool expected)
    {
        Assert.Equal(expected, RelationsTable.IsAir(category));
    }
}
