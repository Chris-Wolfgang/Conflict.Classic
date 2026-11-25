using Wolfgang.Conflict.Classic.Engine.Hex;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Hex;

public class HexPathfinderTests
{
    private static int? UniformCost(HexCoord _) => 1;

    [Fact]
    public void FindPath_when_start_equals_goal_returns_single_hex_path()
    {
        var path = HexPathfinder.FindPath(HexCoord.Zero, HexCoord.Zero, UniformCost);

        Assert.NotNull(path);
        Assert.Single(path);
        Assert.Equal(0, path!.TotalCost);
    }

    [Fact]
    public void FindPath_when_goal_is_straight_east_returns_path_with_correct_cost_and_length()
    {
        var goal = new HexCoord(3, 0);

        var path = HexPathfinder.FindPath(HexCoord.Zero, goal, UniformCost);

        Assert.NotNull(path);
        Assert.Equal(4, path!.Count);
        Assert.Equal(3, path.TotalCost);
        Assert.Equal(HexCoord.Zero, path.Start);
        Assert.Equal(goal, path.Destination);
    }

    [Fact]
    public void FindPath_when_goal_is_unreachable_returns_null()
    {
        // Everything is impassable except the start.
        Func<HexCoord, int?> stepCost = h => h == HexCoord.Zero ? 0 : null;

        var path = HexPathfinder.FindPath(HexCoord.Zero, new HexCoord(2, 0), stepCost);

        Assert.Null(path);
    }

    [Fact]
    public void FindPath_when_impassable_hex_blocks_the_line_routes_around_it()
    {
        // Block the direct east-east cell (1,0); the path must detour.
        var blocked = new HexCoord(1, 0);
        Func<HexCoord, int?> stepCost = h => h == blocked ? null : 1;

        var path = HexPathfinder.FindPath(HexCoord.Zero, new HexCoord(2, 0), stepCost);

        Assert.NotNull(path);
        Assert.DoesNotContain(blocked, path!);
        Assert.True(path.TotalCost >= 3);
    }

    [Fact]
    public void FindPath_when_direct_hex_is_expensive_takes_cheaper_detour()
    {
        // (1,0) costs 10; detour through (1,-1) and (0,1) etc. costs 1 each.
        // Direct path: 0,0 -> 1,0 -> 2,0 = 10+1 = 11
        // Detour:      0,0 -> 1,-1 -> 2,-1 -> 2,0 = 1+1+1 = 3
        var expensive = new HexCoord(1, 0);
        Func<HexCoord, int?> stepCost = h => h == expensive ? 10 : 1;

        var path = HexPathfinder.FindPath(HexCoord.Zero, new HexCoord(2, 0), stepCost);

        Assert.NotNull(path);
        Assert.DoesNotContain(expensive, path!);
        Assert.Equal(3, path!.TotalCost);
    }

    [Fact]
    public void FindPath_when_cost_exceeds_maxCost_returns_null()
    {
        var path = HexPathfinder.FindPath(HexCoord.Zero, new HexCoord(5, 0), UniformCost, maxCost: 3);

        Assert.Null(path);
    }

    [Fact]
    public void FindPath_when_stepCost_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            HexPathfinder.FindPath(HexCoord.Zero, new HexCoord(1, 0), stepCost: null!));
    }
}
