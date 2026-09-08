using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Maze
{
    public enum MazeDrawMode
    {
        Tilemap,
        Sprites
    }

    public class MazeRenderer : MonoBehaviour
    {
        [Header("Modo")]
        [SerializeField] MazeDrawMode drawMode = MazeDrawMode.Tilemap;
        [SerializeField] float cellSize = 1f;

        [Header("Tilemap")]
        [SerializeField] Tilemap tilemap;
        [SerializeField] TileBase wallTile;
        [SerializeField] TileBase floorTile;

        [Header("Sprites")]
        [SerializeField] GameObject wallPrefab;
        [SerializeField] GameObject floorPrefab;
        [SerializeField] Transform spriteContainer;

        readonly List<GameObject> spawned = new List<GameObject>();

        public float CellSize => cellSize;
        public Vector3 Origin => transform.position;

        public void Draw(MazeData maze)
        {
            Clear();

            if (drawMode == MazeDrawMode.Tilemap)
                DrawTilemap(maze);
            else
                DrawSprites(maze);
        }

        public void Clear()
        {
            if (tilemap != null)
                tilemap.ClearAllTiles();

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;
                if (Application.isPlaying)
                    Destroy(spawned[i]);
                else
                    DestroyImmediate(spawned[i]);
            }
            spawned.Clear();
        }

        void DrawTilemap(MazeData maze)
        {
            if (tilemap == null || wallTile == null || floorTile == null)
            {
                Debug.LogError("MazeRenderer: en modo Tilemap hacen falta un Tilemap, wallTile y floorTile.", this);
                return;
            }

            for (int gx = 0; gx < maze.GridWidth; gx++)
            {
                for (int gy = 0; gy < maze.GridHeight; gy++)
                {
                    var position = new Vector3Int(gx, gy, 0);
                    tilemap.SetTile(position, maze.IsWall(gx, gy) ? wallTile : floorTile);
                }
            }
        }

        void DrawSprites(MazeData maze)
        {
            if (wallPrefab == null || floorPrefab == null)
            {
                Debug.LogError("MazeRenderer: en modo Sprites hacen falta wallPrefab y floorPrefab.", this);
                return;
            }

            Transform parent = spriteContainer != null ? spriteContainer : transform;

            for (int gx = 0; gx < maze.GridWidth; gx++)
            {
                for (int gy = 0; gy < maze.GridHeight; gy++)
                {
                    GameObject prefab = maze.IsWall(gx, gy) ? wallPrefab : floorPrefab;
                    Vector3 position = Origin + new Vector3((gx + 0.5f) * cellSize, (gy + 0.5f) * cellSize, 0f);
                    GameObject instance = Instantiate(prefab, position, Quaternion.identity, parent);
                    instance.transform.localScale = Vector3.one * cellSize;
                    spawned.Add(instance);
                }
            }
        }
    }
}
