using Xunit;

namespace Wolfgang.Conflict.Classic.Engine.Tests;

/// <summary>
/// xunit class fixture that loads the embedded unit catalog asynchronously
/// before any test in the class runs, so <see cref="TestCatalog"/> never has
/// to block on <see cref="Units.UnitCatalog.LoadEmbeddedAsync"/>.
/// Every test class that reads <see cref="TestCatalog"/> implements
/// <see cref="IClassFixture{TFixture}"/> of this type.
/// </summary>
public sealed class CatalogFixture : IAsyncLifetime
{
    /// <inheritdoc/>
    public Task InitializeAsync() => TestCatalog.EnsureLoadedAsync();



    /// <inheritdoc/>
    public Task DisposeAsync() => Task.CompletedTask;
}
