using System.Collections.Generic;
using UnityEngine;

namespace Maze
{
    // Rejilla doble: las coordenadas impares son celdas y las pares son las
    // paredes entre ellas. La celda (cx, cy) vive en (2*cx+1, 2*cy+1), así que
    // la pared que separa dos celdas vecinas es su punto medio.
    public class MazeData
    {
        static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        readonly bool[,] grid;

        public int Width { get; }
        public int Height { get; }
        public int GridWidth => Width * 2 + 1;
        public int GridHeight => Height * 2 + 1;
        public int Seed { get; set; }

        public MazeData(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new bool[GridWidth, GridHeight];

            for (int x = 0; x < GridWidth; x++)
                for (int y = 0; y < GridHeight; y++)
                    grid[x, y] = true;
        }

        public bool InBounds(int gx, int gy)
        {
            return gx >= 0 && gy >= 0 && gx < GridWidth && gy < GridHeight;
        }

        public bool IsWall(int gx, int gy)
        {
            return !InBounds(gx, gy) || grid[gx, gy];
        }

        public void CarveCell(int cx, int cy)
        {
            grid[cx * 2 + 1, cy * 2 + 1] = false;
        }

        public void CarveBetween(int ax, int ay, int bx, int by)
        {
            CarveCell(ax, ay);
            CarveCell(bx, by);
            grid[ax + bx + 1, ay + by + 1] = false;
        }

        public bool IsDeadEnd(int cx, int cy)
        {
            int exits = 0;
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int d = Directions[i];
                if (!IsWall(cx * 2 + 1 + d.x, cy * 2 + 1 + d.y))
                    exits++;
            }
            return exits == 1;
        }

        public List<Vector2Int> GetDeadEnds()
        {
            var deadEnds = new List<Vector2Int>();
            for (int cx = 0; cx < Width; cx++)
                for (int cy = 0; cy < Height; cy++)
                    if (IsDeadEnd(cx, cy))
                        deadEnds.Add(new Vector2Int(cx, cy));
            return deadEnds;
        }

        public Vector2Int FindFarthestCell(Vector2Int from)
        {
            var visited = new bool[Width, Height];
            var queue = new Queue<Vector2Int>();
            var neighbours = new List<Vector2Int>();

            visited[from.x, from.y] = true;
            queue.Enqueue(from);
            Vector2Int farthest = from;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                farthest = current;

                GetOpenNeighbours(current.x, current.y, neighbours);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    Vector2Int next = neighbours[i];
                    if (visited[next.x, next.y])
                        continue;
                    visited[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }

            return farthest;
        }

        public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
        {
            var visited = new bool[Width, Height];
            var cameFrom = new Vector2Int[Width, Height];
            var queue = new Queue<Vector2Int>();
            var neighbours = new List<Vector2Int>();
            var path = new List<Vector2Int>();

            visited[from.x, from.y] = true;
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current == to)
                    break;

                GetOpenNeighbours(current.x, current.y, neighbours);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    Vector2Int next = neighbours[i];
                    if (visited[next.x, next.y])
                        continue;
                    visited[next.x, next.y] = true;
                    cameFrom[next.x, next.y] = current;
                    queue.Enqueue(next);
                }
            }

            if (!visited[to.x, to.y])
                return path;

            Vector2Int step = to;
            while (step != from)
            {
                path.Add(step);
                step = cameFrom[step.x, step.y];
            }
            path.Add(from);
            path.Reverse();
            return path;
        }

        void GetOpenNeighbours(int cx, int cy, List<Vector2Int> neighbours)
        {
            neighbours.Clear();
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int d = Directions[i];
                int nx = cx + d.x;
                int ny = cy + d.y;
                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height)
                    continue;
                if (!IsWall(cx * 2 + 1 + d.x, cy * 2 + 1 + d.y))
                    neighbours.Add(new Vector2Int(nx, ny));
            }
        }
    }
}
