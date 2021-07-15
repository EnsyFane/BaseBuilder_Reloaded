using MoonSharp.Interpreter;
using System;

namespace Assets.Scripts.Models
{
    /// <summary>
    /// Model for the world.
    /// </summary>
    [MoonSharpUserData]
    public class World
    {
        public static World Instance { get; private set; }

        private Tile[,] _tiles;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private Action<Tile> cbTileChanged;

        public World(int width, int height)
        {
            Instance = this;

            Width = width;
            Height = height;

            InitializeWorld();
        }

        /// <summary>
        /// Updates all the entities in the world.
        /// </summary>
        /// <param name="deltaTime">Time since the last update.</param>
        public void Update(float deltaTime)
        {
            // TODO: Update all entities.
        }

        /// <summary>
        /// Returns the tile at the given coordinates.
        /// </summary>
        public Tile GetTileAt(int x, int y)
        {
            if (x > Width || x < 0 || y >= Height || y < 0)
            {
                return null;
            }

            return _tiles[x, y];
        }

        /// <summary>
        /// Register a function to be called back when a <see cref="Tile"/> changes.
        /// </summary>
        public void SubscribeTileChanged(Action<Tile> callback) => cbTileChanged += callback;

        /// <summary>
        /// Unregister a function from <see cref="cbTileChanged"/> callback.
        /// </summary>
        public void UnsubscribeTileChanged(Action<Tile> callback) => cbTileChanged -= callback;

        [Obsolete]
        public void RandomizeTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        _tiles[x, y].Type = TileType.Water;
                    }
                    else
                    {
                        _tiles[x, y].Type = TileType.Grass;
                    }
                }
            }
        }

        private void InitializeWorld()
        {
            _tiles = new Tile[Width, Height];

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    _tiles[x, y] = new Tile(x, y);
                    _tiles[x, y].SubscribeTileTypeChanged(OnTileChanged);
                }
            }

            // TODO: Add better logging.
            //Debug.Log($"World created with {Width * Height} tiles.");
        }

        private void OnTileChanged(Tile t)
        {
            cbTileChanged?.Invoke(t);
        }
    }
}