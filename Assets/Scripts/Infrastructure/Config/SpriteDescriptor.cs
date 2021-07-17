using System;
using System.Collections.Generic;

namespace Assets.Scripts.Infrastructure.Config
{
    [Serializable]
    public class SpriteDescriptor
    {
        public IEnumerable<JsonSprite> Sprites { get; set; }
    }

    [Serializable]
    public class JsonSprite
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int pixelsPerUnit { get; set; }
    }
}