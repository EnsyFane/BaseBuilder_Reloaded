namespace Assets.Scripts.Models
{
    /// <summary>
    /// Represents all the tile types supported by the game.
    /// </summary>
    public enum TileType
    {
        Grass,
        Water
    }

    /// <summary>
    /// Extension class for <see cref="TileType"/>.
    /// </summary>
    public static class TileTypeMethods
    {
        public static bool CanBuildOnTileType(this TileType tileType)
        {
            return tileType switch
            {
                TileType.Grass => true,
                _ => false,
            };
        }
    }
}