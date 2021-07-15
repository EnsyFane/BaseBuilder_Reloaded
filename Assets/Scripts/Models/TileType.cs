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

    public static class TileTypeMethods
    {
        public static bool CanBuildOnTileType(this TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Grass:
                    return true;

                case TileType.Water:
                default:
                    return false;
            }
        }
    }
}