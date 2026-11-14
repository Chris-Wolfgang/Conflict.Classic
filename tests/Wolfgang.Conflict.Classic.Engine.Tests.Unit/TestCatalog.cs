using Wolfgang.Conflict.Classic.Engine.Units;

namespace Wolfgang.Conflict.Classic.Engine.Tests;

/// <summary>
/// Shared test helper exposing the embedded unit catalog plus convenient
/// accessors for representative unit types, so tests need not thread the
/// catalog or unit-type ids everywhere. The catalog is loaded asynchronously
/// by <see cref="CatalogFixture"/>; test classes that use this helper
/// implement <c>IClassFixture&lt;CatalogFixture&gt;</c>.
/// </summary>
internal static class TestCatalog
{
    private static readonly Lazy<Task<UnitCatalog>> Loader = new(() => UnitCatalog.LoadEmbeddedAsync());



    /// <summary>Loads the embedded catalog once; later calls return the same task.</summary>
    public static Task EnsureLoadedAsync() => Loader.Value;



    /// <summary>
    /// The full embedded catalog. Valid once <see cref="CatalogFixture"/> has
    /// run for the calling test class.
    /// </summary>
    /// <exception cref="InvalidOperationException">The catalog has not been loaded yet.</exception>
    public static UnitCatalog Catalog
        => Loader.IsValueCreated && Loader.Value.Status == TaskStatus.RanToCompletion
            ? Loader.Value.Result
            : throw new InvalidOperationException("TestCatalog used before CatalogFixture loaded it. Add IClassFixture<CatalogFixture> to the test class.");

    /// <summary>US Infantry — Blue foot soldier.</summary>
    public static UnitTypeDefinition Infantry => Catalog.Get("us-infantry");

    /// <summary>M60A3 — Blue battle tank (the factory-buildable MBT; M1A1 is HQ-only).</summary>
    public static UnitTypeDefinition Tank => Catalog.Get("m60a3");

    /// <summary>F-4E Phantom II — Blue fighter.</summary>
    public static UnitTypeDefinition Fighter => Catalog.Get("f4e");

    /// <summary>Looks up any unit type by catalog id.</summary>
    public static UnitTypeDefinition Get(string id) => Catalog.Get(id);
}
