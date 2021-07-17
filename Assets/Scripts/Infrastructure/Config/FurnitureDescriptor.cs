using Assets.Scripts.Models;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Infrastructure.Config
{
    [Serializable]
    public class FurnitureDescriptor
    {
        public IEnumerable<Furniture> Furnitures { get; set; }
    }
}