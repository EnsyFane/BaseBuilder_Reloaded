using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class used to retrieve sprites from game files.
/// </summary>
public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    private Dictionary<string, Sprite> _sprites;

    public void OnEnable()
    {
        Instance = this;

        LoadSprites();
    }

    public Sprite GetSprite(string categoryName, string spriteName)
    {
        var finalSpriteName = categoryName + "/" + spriteName;
        if (_sprites.ContainsKey(finalSpriteName))
        {
            return _sprites[finalSpriteName];
        }

        // TODO: Return an error texture.
        return null;
    }

    private void LoadSprites()
    {
        _sprites = new Dictionary<string, Sprite>();

        var spritesDirectoryPath = Path.Combine(Application.streamingAssetsPath, "Images");

        LoadSpritesFromDirectory(spritesDirectoryPath);
    }

    private void LoadSpritesFromDirectory(string directoryPath)
    {
        Debug.Log($"Loading Sprites from directory: {directoryPath}.");

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
        // TODO: Unity's LoadImage is returning true when loading files that end with .meta or .xml
        if (filePath.EndsWith(".xml") || filePath.EndsWith(".meta"))
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
                // TODO: Parse the json file to get tile map sprites
            }
            else
            {
                // If no json file is found for texture assue it is a simple 32x32 texture.
                LoadSprite(spriteCategory, baseSpriteName, imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), 32);
            }
        }
    }

    private void LoadSprite(string spriteCategory, string spriteName, Texture2D texture, Rect spriteCoordinates, int pixelsPerUnit)
    {
        var finalSpriteName = spriteCategory + "/" + spriteName;

        var pivotPoint = new Vector2(0.5f, 0.5f);
        var sprite = Sprite.Create(texture, spriteCoordinates, pivotPoint, pixelsPerUnit);

        _sprites.Add(finalSpriteName, sprite);
    }
}