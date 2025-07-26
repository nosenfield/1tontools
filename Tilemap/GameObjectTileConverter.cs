using UnityEngine;
using UnityEngine.Tilemaps;
using OneTon.Logging;

namespace OneTon.Tilemap
{
    public class GameObjectTileConverter : MonoBehaviour
    {
        private static LogService logger = LogService.Get<GameObjectTileConverter>();
        // Replace all game object tiles with their associated game object prefabs
        void Awake()
        {
            UnityEngine.Tilemaps.Tilemap tilemap = GetComponent<UnityEngine.Tilemaps.Tilemap>();
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

            Vector3Int tilePosition;
            TileBase tile;
            GameObjectTile gameObjectTile;
            GameObject instantiatedObject;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    tilePosition = new Vector3Int(x, y, 0);
                    tile = tilemap.GetTile(tilePosition);
                    if (tile is GameObjectTile)
                    {
                        gameObjectTile = tile as GameObjectTile;
                        instantiatedObject = gameObjectTile.CreatePrefab(tilePosition, tilemap);
                        if (instantiatedObject != null)
                        {
                            instantiatedObject.transform.localScale = tilemap.GetTransformMatrix(tilePosition).lossyScale;
                            if (gameObjectTile.HideTilemapRender)
                            {
                                tilemap.SetColor(tilePosition, new Color(1, 1, 1, 0));
                            }
                        }
                    }
                }
            }
        }
    }
}