using System;
using System.Collections.Generic;

namespace Assets.Scripts.Infrastructure.Config
{
    [Serializable]
    public class FurnitureDescriptor
    {
        public IEnumerable<FurnitureJson> Furnitures { get; set; }
    }

    [Serializable]
    public class FurnitureJson
    {
        public string Name { get; set; }
        public string ObjectType { get; set; }
        public float? MovementCost { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool? CanLinkToNeighbour { get; set; }
        public bool? CanEncloseRooms { get; set; }
        public IEnumerable<string> UpdateLUAFunctionNames { get; set; }
        public IDictionary<string, object> FurnitureParameters { get; set; }
        public int? JobWorkSpotOffsetX { get; set; }
        public int? JobWorkSpotOffsetY { get; set; }
        public int? JobSpawnSpotOffsetX { get; set; }
        public int? JobSpawnSpotOffsetY { get; set; }
    }
}