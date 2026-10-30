using Wolfgang.Conflict.Classic.Engine.Game;
using Wolfgang.Conflict.Classic.Engine.Players;
using Wolfgang.Conflict.Classic.Engine.Strategy;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests.Strategy;

public class GameRunnerSmokeTests
{
    [Fact]
    public async Task RunAsync_when_random_strategies_play_mission01_each_game_ends_with_a_winner_or_at_the_turn_cap()
    {
        // Modest count keeps net462 test runtimes reasonable while still
        // exercising the engine across many board states.
        const int games = 25;
        const int maxFullTurns = 100;

        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();

        for (var i = 0; i < games; i++)
        {
            var state = engine.StartGame(mission, randomSeed: 1000 + i);

            var final = await GameRunner.RunAsync(
                state,
                new RandomTestStrategy(seed: i * 31 + 1),
                new RandomTestStrategy(seed: i * 31 + 2),
                maxFullTurns: maxFullTurns);

            // A game either ends with a winner, or plays every allowed full
            // turn: the runner stops after maxFullTurns, when Blue is about to
            // start turn maxFullTurns + 1. Anything else means it stalled.
            if (final.Phase == GamePhase.GameOver)
            {
                Assert.NotNull(final.Winner);
            }
            else
            {
                Assert.Equal(maxFullTurns + 1, final.TurnNumber);
                Assert.Equal(Side.Blue, final.NextToAct);
            }
        }
    }

    [Fact]
    public async Task RunAsync_when_greedy_strategies_play_mission01_with_seed_42_blue_wins_on_turn_8()
    {
        var engine = new GameEngine();
        var mission = await MissionLoader.LoadMission01Async();
        var state = engine.StartGame(mission, randomSeed: 42);

        var final = await GameRunner.RunAsync(
            state,
            new GreedyAiStrategy(),
            new GreedyAiStrategy(),
            maxFullTurns: 500);

        // Greedy play and the engine seed are both deterministic, so seed 42
        // pins one exact outcome. A change here means the AI or the rules
        // changed behaviour; update the expected values only if that was
        // intended.
        Assert.Equal(GamePhase.GameOver, final.Phase);
        Assert.Equal(Side.Blue, final.Winner);
        Assert.Equal(8, final.TurnNumber);
    }
}
