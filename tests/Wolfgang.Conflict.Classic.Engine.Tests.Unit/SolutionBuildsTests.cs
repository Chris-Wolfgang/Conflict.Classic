using Wolfgang.Conflict.Classic.Engine.Game;
using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests;

public class SolutionBuildsTests
{
    [Fact]
    public void Constructor_when_called_creates_GameEngine()
    {
        var sut = new GameEngine();

        Assert.NotNull(sut);
    }
}
