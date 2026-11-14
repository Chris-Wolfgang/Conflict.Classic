namespace Wolfgang.Conflict.Classic.UI.Blazor.Rendering;

/// <summary>
/// NES-inspired colour palette. Original — designed to evoke the 1991 Conflict
/// look without reproducing the game's actual palette or sprites.
/// </summary>
public static class NesPalette
{
    /// <summary>Game-board background (outside the hex grid).</summary>
    public const string Background = "#0a0a18";

    // Terrain
    /// <summary>Plains hex base colour.</summary>
    public const string Plains = "#7ca84c";
    /// <summary>Plains hex shading.</summary>
    public const string PlainsDark = "#5c8030";
    /// <summary>Forest hex base colour.</summary>
    public const string Forest = "#2a5a24";
    /// <summary>Forest hex shadow.</summary>
    public const string ForestDark = "#143010";
    /// <summary>Forest hex highlight.</summary>
    public const string ForestLight = "#3e7e30";
    /// <summary>Mountain hex base colour.</summary>
    public const string Mountain = "#b06838";
    /// <summary>Mountain hex shadow.</summary>
    public const string MountainDark = "#6e3a1c";
    /// <summary>Mountain hex highlight.</summary>
    public const string MountainLight = "#cc8a52";
    // Steel girder bridge — cool grey deck with darker rivets/shadow.
    /// <summary>Bridge deck colour (steel girder, cool grey).</summary>
    public const string Bridge = "#9aa4b0";
    /// <summary>Bridge rivets and shadow.</summary>
    public const string BridgeDark = "#4c5663";
    /// <summary>Bridge deck highlight.</summary>
    public const string BridgeLight = "#cfd6df";
    /// <summary>River hex colour.</summary>
    public const string River = "#3870b8";
    /// <summary>Shoal (shallow water / sandbar) hex colour.</summary>
    public const string Shoal = "#e8d0a0";
    /// <summary>Sea hex colour.</summary>
    public const string Sea = "#1c4c98";
    /// <summary>Reef hex colour.</summary>
    public const string Reef = "#143058";

    // Sides
    /// <summary>Blue side unit and building colour.</summary>
    public const string BlueSide = "#3868c0";
    /// <summary>Blue side shading.</summary>
    public const string BlueSideDark = "#203070";
    /// <summary>Red side unit and building colour.</summary>
    public const string RedSide = "#c83030";
    /// <summary>Red side shading.</summary>
    public const string RedSideDark = "#702018";
    /// <summary>Colour of a building no side owns.</summary>
    public const string NeutralBuilding = "#a89060";

    // Grid + UI
    /// <summary>Hex grid outline colour.</summary>
    public const string Grid = "#1c2030";
    /// <summary>Highlight for the selected hex or unit.</summary>
    public const string Selection = "#fce18c";
    /// <summary>Damage indicator colour.</summary>
    public const string Damage = "#e02c2c";
    /// <summary>Default UI text colour.</summary>
    public const string Text = "#e8e8e8";
}
