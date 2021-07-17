using Assets.Scripts.Infrastructure.LUAParsing;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Models
{
    /// <summary>
    /// Class representing installed objects like walls, dors, chairs etc.
    /// </summary>
    [MoonSharpUserData]
    public class Furniture
    {
        /// <summary>
        /// Custom parameters for this piece of furniture.
        /// </summary>
        /// <remarks>
        /// LUA code will bind to this dictionary to get the parameters.
        /// </remarks>
        protected IDictionary<string, object> furnitureParameters;

        /// <summary>
        /// LUA functions called every update tick.
        /// </summary>
        /// <remarks>
        /// The functions will receive: the furniture object and the time passed since last update tick.
        /// </remarks>
        protected IList<string> updateLUAFunctionNames;

        protected string isEnterableLUAFunctionName;

        /// <summary>
        /// Offset based on the bottom left tile of the sprite that indicates
        /// where the character will stand while working on a job for this furniture.
        /// </summary>
        public Vector2 JobWorkSpotOffset { get; set; } = Vector2.zero;

        public Tile JobWorkSpotTile
        {
            get => World.Instance.GetTileAt(Tile.X + (int)JobWorkSpotOffset.x, Tile.Y + (int)JobWorkSpotOffset.y);
        }

        /// <summary>
        /// Offset based on the bottom left tile of the sprite that indicates
        /// where items spawned by this furniture will be spawned.
        /// </summary>
        public Vector2 JobSpawnSpotOffset { get; set; } = Vector2.zero;

        public Tile JobSpawnSpotTile
        {
            get => World.Instance.GetTileAt(Tile.X + (int)JobSpawnSpotOffset.x, Tile.Y + (int)JobSpawnSpotOffset.y);
        }

        /// <summary>
        /// See <see cref="Enterability"/>.
        /// </summary>
        public Enterability Enterability
        {
            get
            {
                if (string.IsNullOrEmpty(isEnterableLUAFunctionName))
                {
                    return Enterability.Enterable;
                }

                var result = FurnitureActions.CallFunction(isEnterableLUAFunctionName, this);
                return (Enterability)result.Number;
            }
        }

        /// <summary>
        /// The base tile of the furniture.
        /// </summary>
        public Tile Tile { get; protected set; }

        /// <summary>
        /// The type of the object. Will be used to identify the sprite to be used for this object.
        /// </summary>
        public string ObjectType { get; protected set; }

        private string _name = null;

        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? ObjectType : _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// The cost of moving through a tile with this furniture.
        /// </summary>
        /// <remarks>
        /// The cost is added to the tile movement cost.
        /// Setting a value of <code>Infinity</code> means the tile is impassible.
        /// </remarks>
        public float MovementCost { get; protected set; }

        /// <summary>
        /// Determines if a room recalculation should be done after the furniture is placed.
        /// </summary>
        public bool CanEncloseRooms { get; protected set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public Color Tint { get; set; } = Color.white;

        public bool CanLinkToNeighbour { get; protected set; }

        public Action<Furniture> cbFurnitureChanged { get; protected set; }
        public Action<Furniture> cbFurnitureRemoved { get; protected set; }

        private readonly Func<Tile, bool> isPositionValidFunction;

        public bool IsPositionValid(Tile tile)
        {
            return isPositionValidFunction(tile);
        }

        public Furniture()
        {
            updateLUAFunctionNames = new List<string>();
            furnitureParameters = new Dictionary<string, object>();

            isPositionValidFunction = DefaultIsPositionValid;
            Width = 1;
            Height = 1;
        }

        private Furniture(Furniture other)
        {
            ObjectType = other.ObjectType;
            Name = other.Name;
            MovementCost = other.MovementCost;
            CanEncloseRooms = other.CanEncloseRooms;
            Width = other.Width;
            Height = other.Height;
            Tint = other.Tint;
            CanLinkToNeighbour = other.CanLinkToNeighbour;

            JobWorkSpotOffset = other.JobWorkSpotOffset;
            JobSpawnSpotOffset = other.JobSpawnSpotOffset;

            furnitureParameters = new Dictionary<string, object>(other.furnitureParameters);
            if (other.updateLUAFunctionNames != null)
            {
                updateLUAFunctionNames = new List<string>(other.updateLUAFunctionNames);
            }
            if (other.isPositionValidFunction != null)
            {
                isPositionValidFunction = (Func<Tile, bool>)other.isPositionValidFunction.Clone();
            }
            isEnterableLUAFunctionName = other.isEnterableLUAFunctionName;
        }

        /// <summary>
        /// Makes a copy of this furniture instance.
        /// </summary>
        /// <returns></returns>
        public virtual Furniture Clone()
        {
            return new Furniture(this);
        }

        public void Update(float deltaTime)
        {
            if (updateLUAFunctionNames != null)
            {
                FurnitureActions.CallLUAUpdateFuntions(updateLUAFunctionNames, this, deltaTime);
            }
        }

        /// <summary>
        /// Places an instance of a furniture prototype object on a given tile.
        /// </summary>
        /// <param name="prototype">The furniture prototype.</param>
        /// <param name="tile">The tile on which to place the creted furniture.</param>
        /// <returns>The created furniture.</returns>
        public static Furniture PlaceInstance(Furniture prototype, Tile tile)
        {
            if (!prototype.isPositionValidFunction(tile))
            {
                Debug.LogError("Tried to place furniture in invalid spot.");
                return null;
            }

            var createdFurniture = prototype.Clone();
            createdFurniture.Tile = tile;

            // FIXME: This assumes the furniture is 1x1.
            if (!tile.PlaceFurnitureInside(createdFurniture))
            {
                Debug.LogError("Could not place furniture inside tile.");
                return null;
            }

            if (createdFurniture.CanLinkToNeighbour)
            {
                Tile t;
                t = createdFurniture.Tile.NorthTile;
                if (t != null && t.Furniture != null && t.Furniture.cbFurnitureChanged != null && t.Furniture.ObjectType == createdFurniture.ObjectType)
                {
                    t.Furniture.cbFurnitureChanged(t.Furniture);
                }

                t = createdFurniture.Tile.EastTile;
                if (t != null && t.Furniture != null && t.Furniture.cbFurnitureChanged != null && t.Furniture.ObjectType == createdFurniture.ObjectType)
                {
                    t.Furniture.cbFurnitureChanged(t.Furniture);
                }

                t = createdFurniture.Tile.SouthTile;
                if (t != null && t.Furniture != null && t.Furniture.cbFurnitureChanged != null && t.Furniture.ObjectType == createdFurniture.ObjectType)
                {
                    t.Furniture.cbFurnitureChanged(t.Furniture);
                }

                t = createdFurniture.Tile.WestTile;
                if (t != null && t.Furniture != null && t.Furniture.cbFurnitureChanged != null && t.Furniture.ObjectType == createdFurniture.ObjectType)
                {
                    t.Furniture.cbFurnitureChanged(t.Furniture);
                }
            }

            return createdFurniture;
        }

        public void SubscribeFurnitureChanged(Action<Furniture> callback) => cbFurnitureChanged += callback;

        public void UnsubscribeFurnitureChanged(Action<Furniture> callback) => cbFurnitureChanged -= callback;

        public void SubscribeFurnitureRemoved(Action<Furniture> callback) => cbFurnitureRemoved += callback;

        public void UnsubscribeFurnitureRemoved(Action<Furniture> callback) => cbFurnitureRemoved -= callback;

        public void SubscribeLUAUpdateAction(string luaFunctionName) => updateLUAFunctionNames.Add(luaFunctionName);

        public void UnsubscribeLUAUpdateAction(string luaFunctionName) => updateLUAFunctionNames.Remove(luaFunctionName);

        protected bool DefaultIsPositionValid(Tile baseTile)
        {
            for (var x = baseTile.X; x < baseTile.X + Width; x++)
            {
                for (var y = baseTile.Y; y < baseTile.Y + Height; y++)
                {
                    var tile = World.Instance.GetTileAt(x, y);

                    if (!tile.Type.CanBuildOnTileType() || tile.Furniture != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public object GetParameter(string key, object defaultValue)
        {
            return furnitureParameters.ContainsKey(key) ? furnitureParameters[key] : defaultValue;
        }

        public void SetParameter(string key, object value)
        {
            furnitureParameters[key] = value;
        }

        public void Deconstruct()
        {
            Tile.UnplaceFurniture();

            cbFurnitureRemoved?.Invoke(this);

            if (CanEncloseRooms)
            {
                // TODO: recalculate rooms.
            }
        }
    }
}