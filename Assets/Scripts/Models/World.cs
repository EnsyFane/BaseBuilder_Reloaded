using Assets.Scripts.Infrastructure.Config;
using Assets.Scripts.Infrastructure.LUAParsing;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

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
        public List<Furniture> Furnitures { get; private set; }

        private Dictionary<string, Furniture> furniturePrototypes;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private Action<Tile> cbTileChanged;
        private Action<Furniture> cbFurnitureCreated;

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
            Furnitures.ForEach(f => f.Update(deltaTime));
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

        public void SubscribeFurnitureCreated(Action<Furniture> callback) => cbFurnitureCreated += callback;

        public void UnsubscribeFurnitureCreated(Action<Furniture> callback) => cbFurnitureCreated -= callback;

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

        public bool IsFurniturePlacementValid(string objectType, Tile tile) => furniturePrototypes[objectType]?.IsPositionValid(tile) ?? false;

        public Furniture GetFurniturePrototype(string objectType)
        {
            if (!furniturePrototypes.ContainsKey(objectType))
            {
                Debug.LogError($"No furniture prototype for {objectType}.");
                return null;
            }

            return furniturePrototypes[objectType];
        }

        public Furniture PlaceFurniture(string objectType, Tile tile, bool doRoomFloodFill = true)
        {
            if (!furniturePrototypes.ContainsKey(objectType))
            {
                Debug.LogError($"No furniture prototype for {objectType}.");
                return null;
            }

            var placedFurniture = Furniture.PlaceInstance(furniturePrototypes[objectType], tile);
            if (placedFurniture == null)
            {
                return null;
            }

            Furnitures.Add(placedFurniture);
            placedFurniture.SubscribeFurnitureRemoved(OnFurnitureRemoved);

            if (doRoomFloodFill && placedFurniture.CanEncloseRooms)
            {
                // TODO: Do room flood fill
            }

            if (cbFurnitureCreated != null)
            {
                cbFurnitureCreated.Invoke(placedFurniture);

                if (placedFurniture.MovementCost != 1)
                {
                    // TODO: Invalidate the tile graph
                }
            }

            return placedFurniture;
        }

        private void OnFurnitureRemoved(Furniture furniture) => Furnitures.Remove(furniture);

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

            Furnitures = new List<Furniture>();
            CreateFurniturePrototypes();
        }

        private void CreateFurniturePrototypes()
        {
            LoadFurnitureLua();

            furniturePrototypes = new Dictionary<string, Furniture>();
            var furnitureJsonPath = Path.Combine(Application.streamingAssetsPath, "Data", "Furniture.json");
            var furnitureJson = File.ReadAllText(furnitureJsonPath);

            var furnitureDescriptor = JsonConvert.DeserializeObject<FurnitureDescriptor>(furnitureJson);

            foreach (var furniture in furnitureDescriptor.Furnitures)
            {
                furniturePrototypes.Add(furniture.ObjectType, furniture);
            }

            Debug.Log($"Read {furnitureDescriptor.Furnitures.Count()} furniture prototypes.");
        }

        private void LoadFurnitureLua()
        {
            var filePath = Path.Combine(Application.streamingAssetsPath, "LUA", "Furniture.lua");
            var furnitureLuaCode = File.ReadAllText(filePath);

            // FIXME: I don't like this
            new FurnitureActions(furnitureLuaCode);
        }

        private void OnTileChanged(Tile t)
        {
            cbTileChanged?.Invoke(t);
        }
    }
}