using BaseBuilder_Reloaded.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;

namespace BaseBuilder_Reloaded.Scripts.Controllers
{
    public class TileSpriteController : MonoBehaviour
    {
        private Dictionary<Tile, GameObject> _tileGameObjectMap;

        private World World
        {
            get => WorldController.Instance.World;
        }

        public void Start()
        {
            _tileGameObjectMap = new Dictionary<Tile, GameObject>();

            for (var x = 0; x < World.Width; x++)
            {
                for (var y = 0; y < World.Height; y++)
                {
                    var tileData = World.GetTileAt(x, y);
                    var tileGameObject = new GameObject
                    {
                        name = $"Tile_{x}_{y}",
                    };
                    tileGameObject.transform.position = new Vector3(tileData.X, tileData.Y, 0);
                    tileGameObject.transform.SetParent(transform, true);

                    var spriteRenderer = tileGameObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = SpriteManager.Instance.GetSprite("Tile", tileData.Type.ToString());
                    spriteRenderer.sortingLayerName = "Tiles";

                    _tileGameObjectMap.Add(tileData, tileGameObject);

                    OnTileChanged(tileData);
                }
            }

            World.SubscribeTileChanged(OnTileChanged);
        }

        private void OnTileChanged(Tile tileData)
        {
            if (!_tileGameObjectMap.ContainsKey(tileData))
            {
                Debug.LogError("Unrecognized Tile.");
                return;
            }

            var tileGameObject = _tileGameObjectMap[tileData];
            if (tileGameObject == null)
            {
                Debug.LogError("No GameObject for given tile.");
                return;
            }

            tileGameObject.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.GetSprite("Tile", tileData.Type.ToString());
        }
    }
}