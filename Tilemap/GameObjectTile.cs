using UnityEngine;
using UnityEngine.Tilemaps;
using OneTon.Logging;

namespace OneTon.Tilemap
{
    [CreateAssetMenu(fileName = "_New GameObject Tile", menuName = "nosenfield/Tilemap/GameObjectTile")]
    public class GameObjectTile : Tile
    {
        private static readonly LogService logger = LogService.Get<GameObjectTile>();
        public GameObject prefab; // Assign this in the Tile Palette
        public bool HideTilemapRender; // hide the tile in the tilemap renderer

        public GameObject CreatePrefab(Vector3Int position, UnityEngine.Tilemaps.Tilemap tilemap)
        {
            GameObject instance = null;
            if (prefab != null)
            {
                Vector3 worldPos = tilemap.GetCellCenterWorld(position);
                instance = Instantiate(prefab, worldPos, Quaternion.identity);
                instance.transform.parent = tilemap.transform; // Keep it organized under Tilemap
            }

            return instance;
        }
    }
}