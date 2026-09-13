using UnityEngine;
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
        }

        public void Clear()
        {
            if (floorTilemap != null)
                floorTilemap.ClearAllTiles();
            if (wallTilemap != null)
                wallTilemap.ClearAllTiles();
            if (decorTilemap != null)
                decorTilemap.ClearAllTiles();
        }

        public Vector3Int CellToTileCenter(int cx, int cy)
        {
            GetBlockRange(cx * 2 + 1, out int startX, out int lengthX);
            GetBlockRange(cy * 2 + 1, out int startY, out int lengthY);
            return new Vector3Int(startX + lengthX / 2, startY + lengthY / 2, 0);
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
