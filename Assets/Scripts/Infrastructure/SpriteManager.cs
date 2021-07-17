using Assets.Scripts.Infrastructure.Config;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// Class used to load and offer sprites from game files.
    /// </summary>
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance { get; private set; }

        private Dictionary<string, Sprite> _sprites;

        public static Sprite ErrorSprite
        {
            get => Instance._sprites["Error/Error"];
        }

        public void OnEnable()
        {
            Instance = this;

            LoadSprites();
        }

        /// <summary>
        /// Retrieves a <see cref="Sprite"/> with the given category and name. If the Sprite doesn't exist it returns an error sprite.
        /// </summary>
        /// <param name="categoryName">The category of the sprite. (Usually the subdirectory in which the sprite is placed)</param>
        /// <param name="spriteName">The name of the sprite.</param>
        public Sprite GetSprite(string categoryName, string spriteName)
        {
            var finalSpriteName = categoryName + "/" + spriteName;
            if (_sprites.ContainsKey(finalSpriteName))
            {
                return _sprites[finalSpriteName];
            }

            Debug.Log($"No sprite with identifier {finalSpriteName} was loaded.");
            return ErrorSprite;
        }

        private void LoadSprites()
        {
            _sprites = new Dictionary<string, Sprite>();

            var spritesDirectoryPath = Path.Combine(Application.streamingAssetsPath, "Images");

            LoadSpritesFromDirectory(spritesDirectoryPath);
        }

        private void LoadSpritesFromDirectory(string directoryPath)
        {
            //Debug.Log($"Loading Sprites from directory: {directoryPath}.");

            var subDirectories = Directory.GetDirectories(directoryPath);
            foreach (var subDirectory in subDirectories)
            {
                LoadSpritesFromDirectory(subDirectory);
            }

            var filesInDirectory = Directory.GetFiles(directoryPath);
            foreach (var filePath in filesInDirectory)
            {
                var spriteCategory = new DirectoryInfo(directoryPath).Name;
                LoadImage(spriteCategory, filePath);
            }
        }

        private void LoadImage(string spriteCategory, string filePath)
        {
            // TEMP: Unity's LoadImage is returning true when loading files that end with .meta or .xml
            if (filePath.EndsWith(".xml") || filePath.EndsWith(".meta") || filePath.EndsWith(".json"))
            {
                return;
            }

            var imageBytes = File.ReadAllBytes(filePath);
            var imageTexture = new Texture2D(1, 1);

            if (imageTexture.LoadImage(imageBytes))
            {
                var baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
                var basePath = Path.GetDirectoryName(filePath);

                var jsonPath = Path.Combine(basePath, baseSpriteName + ".json");
                if (File.Exists(jsonPath))
                {
                    var spriteJson = File.ReadAllText(jsonPath);
                    var spriteDescriptor = JsonUtility.FromJson<SpriteDescriptor>(spriteJson);
                    LoadSpriteMapFromJson(spriteCategory, imageTexture, spriteDescriptor);
                }
                else
                {
                    // If no json file is found for texture assue it is a simple 32x32 texture.
                    LoadSprite(spriteCategory, baseSpriteName, imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), 32);
                }
            }
        }

        private void LoadSpriteMapFromJson(string spriteCategory, Texture2D imageTexture, SpriteDescriptor spriteDescriptor)
        {
            foreach (var sprite in spriteDescriptor.Sprites)
            {
                LoadSprite(spriteCategory, sprite.Name, imageTexture, new Rect(sprite.X, sprite.Y, sprite.Width, sprite.Height), sprite.pixelsPerUnit);
            }
        }

        private void LoadSprite(string spriteCategory, string spriteName, Texture2D texture, Rect spriteCoordinates, int pixelsPerUnit)
        {
            var finalSpriteName = spriteCategory + "/" + spriteName;
            //Debug.Log(finalSpriteName);

            var pivotPoint = new Vector2(0.5f, 0.5f);
            var sprite = Sprite.Create(texture, spriteCoordinates, pivotPoint, pixelsPerUnit);

            _sprites.Add(finalSpriteName, sprite);
        }
    }
}