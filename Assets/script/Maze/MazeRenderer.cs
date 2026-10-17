using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace Maze
{
    public class MazeRenderer : MonoBehaviour
    {
        [Header("Tilemaps")]
        [Tooltip("Tilemap donde se pinta el suelo.")]
        [SerializeField] Tilemap floorTilemap;

        [Tooltip("Tilemap donde se pintan las paredes.")]
        [SerializeField] Tilemap wallTilemap;

        [Tooltip("Tilemap donde se colocan los props de decoración.")]
        [SerializeField] Tilemap decorTilemap;

        [Header("Tiles")]
        [Tooltip("Tile de pared. Debe ser un Rule Tile que elija la forma según los vecinos.")]
        [SerializeField] TileBase wallTile;

        [Tooltip("Tiles de suelo. El primero es la base; el resto son variantes que aparecen según floorVariantChance.")]
        [SerializeField] TileBase[] floorTiles;

        [Tooltip("Probabilidad (0-1) de que un tile de suelo use una variante en vez de la base. Se decide con un hash determinista de la posición del tile, no con Random.")]
        [Range(0f, 1f)] [SerializeField] float floorVariantChance = 0.2f;

        [Header("Escala")]
        [Tooltip("Cuántos tiles ocupa cada celda del laberinto por lado. Las paredes escalan igual.")]
        [Min(1)] [SerializeField] int tilesPerCell = 2;

        [Header("Props")]
        [Tooltip("Tiles de props que pueden aparecer en los callejones sin salida.")]
        [SerializeField] TileBase[] propTiles;

        [Tooltip("Probabilidad (0-1) de que un callejón sin salida reciba un prop.")]
        [Range(0f, 1f)] [SerializeField] float propChance = 0.3f;

        [Header("Entrada y Salida")]
        [Tooltip("Prefab interactuable (con interact_with_npc) que marca la entrada del laberinto, en la celda donde empieza a excavarse.")]
        [SerializeField] GameObject entrancePrefab;

        [Tooltip("Prefab interactuable (con interact_with_npc) que marca la salida del laberinto, en la celda más lejana de la entrada.")]
        [SerializeField] GameObject exitPrefab;

        GameObject entranceInstance;
        GameObject exitInstance;

        /// Se dispara cuando el jugador entra en la zona interactuable de la entrada.
        public event Action OnPlayerEnteredMaze;

        /// Se dispara cuando el jugador entra en la zona interactuable de la salida.
        public event Action OnPlayerExitedMaze;

        public int TilesPerCell => tilesPerCell;

        public void Draw(MazeData maze)
        {
            Clear();

            if (floorTilemap == null || wallTilemap == null || decorTilemap == null || wallTile == null || floorTiles == null || floorTiles.Length == 0)
            {
                Debug.LogError("MazeRenderer: faltan floorTilemap, wallTilemap, decorTilemap, wallTile o floorTiles.", this);
                return;
            }

            DrawTiles(maze);
            DrawProps(maze);
            SpawnInteractables(maze);
        }

        public void Clear()
        {
            if (floorTilemap != null)
                floorTilemap.ClearAllTiles();
            if (wallTilemap != null)
                wallTilemap.ClearAllTiles();
            if (decorTilemap != null)
                decorTilemap.ClearAllTiles();

            if (entranceInstance != null)
                Destroy(entranceInstance);
            if (exitInstance != null)
                Destroy(exitInstance);
            entranceInstance = null;
            exitInstance = null;
        }

        public Vector3Int CellToTileCenter(int cx, int cy)
        {
            GetBlockRange(cx * 2 + 1, out int startX, out int lengthX);
            GetBlockRange(cy * 2 + 1, out int startY, out int lengthY);
            return new Vector3Int(startX + lengthX / 2, startY + lengthY / 2, 0);
        }

        public Vector3 CellToWorldPosition(int cx, int cy)
        {
            return floorTilemap.GetCellCenterWorld(CellToTileCenter(cx, cy));
        }

        // Inversa de CellToWorldPosition: de una posición del mundo a la celda lógica más cercana.
        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3Int tile = floorTilemap.WorldToCell(worldPosition);
            int gx = Mathf.FloorToInt((float)tile.x / tilesPerCell);
            int gy = Mathf.FloorToInt((float)tile.y / tilesPerCell);
            return new Vector2Int((gx - 1) / 2, (gy - 1) / 2);
        }

        void SpawnInteractables(MazeData maze)
        {
            entranceInstance = SpawnInteractable(entrancePrefab, maze.Start, () => OnPlayerEnteredMaze?.Invoke());
            exitInstance = SpawnInteractable(exitPrefab, maze.Exit, () => OnPlayerExitedMaze?.Invoke());
        }

        GameObject SpawnInteractable(GameObject prefab, Vector2Int cell, UnityAction onPlayerEnter)
        {
            if (prefab == null)
                return null;

            Vector3 worldPosition = CellToWorldPosition(cell.x, cell.y);
            GameObject instance = Instantiate(prefab, worldPosition, Quaternion.identity, transform);

            var interactable = instance.GetComponent<interact_with_npc>();
            if (interactable != null)
                interactable.onPlayerEnter.AddListener(onPlayerEnter);
            else
                Debug.LogWarning($"MazeRenderer: {prefab.name} no tiene interact_with_npc, no se pudo asignar el comportamiento de entrada/salida.", instance);

            return instance;
        }

        void DrawTiles(MazeData maze)
        {
            for (int gx = 0; gx < maze.GridWidth; gx++)
            {
                GetBlockRange(gx, out int startX, out int lengthX);

                for (int gy = 0; gy < maze.GridHeight; gy++)
                {
                    GetBlockRange(gy, out int startY, out int lengthY);
                    bool wall = maze.IsWall(gx, gy);

                    for (int ix = 0; ix < lengthX; ix++)
                    {
                        for (int iy = 0; iy < lengthY; iy++)
                        {
                            var position = new Vector3Int(startX + ix, startY + iy, 0);
                            if (wall)
                                wallTilemap.SetTile(position, wallTile);
                            else
                                floorTilemap.SetTile(position, PickFloorTile(position));
                        }
                    }
                }
            }
        }

        void DrawProps(MazeData maze)
        {
            if (propTiles == null || propTiles.Length == 0 || propChance <= 0f)
                return;

            var rng = new System.Random(maze.Seed);
            var deadEnds = maze.GetDeadEnds();

            for (int i = 0; i < deadEnds.Count; i++)
            {
                if (rng.NextDouble() >= propChance)
                    continue;

                Vector3Int position = CellToTileCenter(deadEnds[i].x, deadEnds[i].y);
                decorTilemap.SetTile(position, propTiles[rng.Next(propTiles.Length)]);
            }
        }

        TileBase PickFloorTile(Vector3Int position)
        {
            if (floorTiles.Length == 1)
                return floorTiles[0];

            int hash = PositionHash(position);
            float t = (hash % 100000) / 100000f;
            if (t >= floorVariantChance)
                return floorTiles[0];

            int variantIndex = 1 + hash % (floorTiles.Length - 1);
            return floorTiles[variantIndex];
        }

        void GetBlockRange(int gridIndex, out int start, out int length)
        {
            start = gridIndex * tilesPerCell;
            length = tilesPerCell;
        }

        static int PositionHash(Vector3Int position)
        {
            unchecked
            {
                int hash = position.x * 374761393 + position.y * 668265263;
                hash = (hash ^ (hash >> 13)) * 1274126177;
                hash ^= hash >> 16;
                return hash & 0x7fffffff;
            }
        }
    }
}
