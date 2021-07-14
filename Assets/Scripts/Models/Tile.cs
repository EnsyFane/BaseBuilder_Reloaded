using System;

namespace BaseBuilder_Reloaded.Scripts.Models
{
    /// <summary>
    /// Model for a tile, the building blocks of the world.
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// A list of functions to be called when the <see cref="Type"/> of the tile is changed.
        /// </summary>
        private Action<Tile> cbTileChanged;

        private TileType _type = TileType.Water;

        public TileType Type
        {
            get => _type;
            set
            {
                var oldType = _type;
                _type = value;

                if (cbTileChanged != null && oldType != _type)
                {
                    cbTileChanged(this);
                }
            }
        }

        public int X { get; private set; }
        public int Y { get; private set; }

        // TODO: This is just hardcoded for now. Basically just a reminder of something we
        // might want to do more with in the future.
        private const float baseTileMovementCost = 1;

        /// <summary>
        /// The cost of moving to this tile. 0 means the tile is not walkable.
        /// </summary>
        public float MovementCost
        {
            get
            {
                // TODO: Return 0 for unwalkable tiles types.
                // TODO: Modify speed based on furniture/objects placed on the tile.
                return baseTileMovementCost;
            }
        }

        /// <summary>
        /// See <see cref="Enterability"/>.
        /// </summary>
        public Enterability Enterability
        {
            get
            {
                if (MovementCost == 0)
                {
                    return Enterability.NotEnterable;
                }

                // TODO: Add logic for when furniture/objects/characters are on the tile.

                return Enterability.Enterable;
            }
        }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Register a function to be called back when the <see cref="Type"/> changes.
        /// </summary>
        public void SubscribeTileTypeChanged(Action<Tile> callback) => cbTileChanged += callback;

        /// <summary>
        /// Unregister a function from <see cref="cbTileChanged"/> callback.
        /// </summary>
        public void UnsubscribeTileTypeChanged(Action<Tile> callback) => cbTileChanged -= callback;
    }

    /// <summary>
    /// Represents all the tile types supported by the game.
    /// </summary>
    public enum TileType
    {
        Grass,
        Water
    }

    /// <summary>
    /// Represents wether a character can enter the tile or not.
    /// </summary>
    /// <remarks>
    /// <see cref="Enterability.Enterable"/> The tile can be entered right now.
    /// <see cref="Enterability.NotEnterable"/> The tile can never be entered.
    /// <see cref="Enterability.Busy"/> The tile can be entered after the furniture/object/character on the tile has finished its job.
    /// </remarks>
    public enum Enterability
    {
        Enterable,
        NotEnterable,
        Busy
    }
}