using Assets.Scripts.Infrastructure;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class FurnitureSpriteController : MonoBehaviour
    {
        private Dictionary<Furniture, GameObject> _furnitureGameObjectMap;

        private World World
        {
            get => WorldController.Instance.World;
        }

        public void Start()
        {
            _furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

            World.SubscribeFurnitureCreated(OnFurnitureCreated);
            World.Furnitures.ForEach(furniture => OnFurnitureCreated(furniture));
        }

        private void OnFurnitureCreated(Furniture furniture)
        {
            var furnitureGameObject = new GameObject();
            _furnitureGameObjectMap.Add(furniture, furnitureGameObject);

            furnitureGameObject.name = $"{furniture.ObjectType}_{furniture.Tile.X}_{furniture.Tile.Y}";
            furnitureGameObject.transform.position = new Vector3(
                    furniture.Tile.X + (furniture.Width - 1) / 2,
                    furniture.Tile.Y + (furniture.Height - 1) / 2,
                    0
            );
            furnitureGameObject.transform.SetParent(transform, true);

            // FIXME: this is bad, find another way to do this.
            if (furniture.ObjectType.ToLower() == "door")
            {
                var northTile = furniture.Tile.NorthTile;
                var southTile = furniture.Tile.SouthTile;

                if (northTile != null &&
                    southTile != null &&
                    northTile.Furniture != null &&
                    southTile.Furniture != null &&
                    northTile.Furniture.ObjectType.ToLower().Contains("wall") &&
                    southTile.Furniture.ObjectType.ToLower().Contains("wall"))
                {
                    furnitureGameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }

            var spriteRenderer = furnitureGameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetSpriteForFurniture(furniture);
            spriteRenderer.sortingLayerName = "Furniture";
            spriteRenderer.color = furniture.Tint;

            furniture.SubscribeFurnitureChanged(OnFurnitureChanged);
            furniture.SubscribeFurnitureRemoved(OnFurnitureRemoved);
        }

        public Sprite GetSpriteForFurniture(Furniture furniture)
        {
            var spriteName = string.Empty;

            if (!furniture.CanLinkToNeighbour)
            {
                // FIXME: Remove hardcoding
                if (furniture.ObjectType.ToLower() == "door")
                {
                    var openPercent = (float)furniture.GetParameter("openPercent", 0f);
                    if (openPercent < 0.1f)
                    {
                        spriteName = "Door";
                    }
                    else if (openPercent < 0.5f)
                    {
                        spriteName = "Door_opening_1";
                    }
                    else if (openPercent < 0.9f)
                    {
                        spriteName = "Door_opening_2";
                    }
                    else
                    {
                        spriteName = "Door_open";
                    }
                }
            }
            else
            {
                spriteName = furniture.ObjectType + "_";

                var tile = furniture.Tile.NorthTile;
                if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
                {
                    spriteName += "N";
                }
                tile = furniture.Tile.EastTile;
                if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
                {
                    spriteName += "E";
                }
                tile = furniture.Tile.SouthTile;
                if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
                {
                    spriteName += "S";
                }
                tile = furniture.Tile.WestTile;
                if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
                {
                    spriteName += "W";
                }
            }

            return SpriteManager.Instance.GetSprite("Furniture", spriteName);
        }

        public Sprite GetSpriteForFurniture(string objectType)
        {
            var sprite = SpriteManager.Instance.GetSprite("Furniture", objectType);

            if (sprite == SpriteManager.ErrorSprite)
            {
                sprite = SpriteManager.Instance.GetSprite("Furniture", objectType + "_");
            }

            return sprite;
        }

        private void OnFurnitureChanged(Furniture furniture)
        {
            if (!_furnitureGameObjectMap.ContainsKey(furniture))
            {
                Debug.LogError("Trying to change furniture that is not tracked.");
                return;
            }

            var furnitureSpriteRenderer = _furnitureGameObjectMap[furniture].GetComponent<SpriteRenderer>(); ;
            furnitureSpriteRenderer.sprite = GetSpriteForFurniture(furniture);
            furnitureSpriteRenderer.color = furniture.Tint;
        }

        private void OnFurnitureRemoved(Furniture furniture)
        {
            if (!_furnitureGameObjectMap.ContainsKey(furniture))
            {
                Debug.LogError("Trying to remove furniture that is not tracked.");
                return;
            }

            var furnitureGameObject = _furnitureGameObjectMap[furniture];
            Destroy(furnitureGameObject);
            _furnitureGameObjectMap.Remove(furniture);
        }
    }
}