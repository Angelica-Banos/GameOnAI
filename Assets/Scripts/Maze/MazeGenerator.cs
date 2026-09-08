using System.Collections.Generic;
using UnityEngine;

namespace Maze
{
    public class MazeGenerator
    {
        static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        readonly Vector2Int[] candidates = new Vector2Int[4];

        System.Random rng;
        bool[,] visited;
        Vector2Int[,] incoming;

        public MazeData Generate(int width, int height, int seed, MazeSettings settings)
        {
            rng = new System.Random(seed);
            visited = new bool[width, height];
            incoming = new Vector2Int[width, height];

            var maze = new MazeData(width, height);
            var stack = new Stack<Vector2Int>();

            var start = new Vector2Int(rng.Next(width), rng.Next(height));
            visited[start.x, start.y] = true;
            maze.CarveCell(start.x, start.y);
            stack.Push(start);

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();

                if (!TryPickNeighbour(current, width, height, settings.straightness, out Vector2Int next))
                {
                    stack.Pop();
                    continue;
                }

                maze.CarveBetween(current.x, current.y, next.x, next.y);
                visited[next.x, next.y] = true;
                incoming[next.x, next.y] = next - current;
                stack.Push(next);
            }

            if (settings.braiding > 0f)
                Braid(maze, settings.braiding);

            return maze;
        }

        bool TryPickNeighbour(Vector2Int cell, int width, int height, float straightness, out Vector2Int next)
        {
            int count = 0;
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int candidate = cell + Directions[i];
                if (candidate.x < 0 || candidate.y < 0 || candidate.x >= width || candidate.y >= height)
                    continue;
                if (visited[candidate.x, candidate.y])
                    continue;
                candidates[count++] = candidate;
            }

            if (count == 0)
            {
                next = Vector2Int.zero;
                return false;
            }

            Vector2Int straightAhead = cell + incoming[cell.x, cell.y];
            if (straightness > 0f && rng.NextDouble() < straightness)
            {
                for (int i = 0; i < count; i++)
                {
                    if (candidates[i] != straightAhead)
                        continue;
                    next = straightAhead;
                    return true;
                }
            }

            next = candidates[rng.Next(count)];
            return true;
        }

        // Un laberinto perfecto está lleno de callejones sin salida; abrir
        // algunos crea ciclos, y eso se juega mucho mejor que un árbol donde
        // solo existe una ruta posible entre cada par de puntos.
        void Braid(MazeData maze, float braiding)
        {
            for (int cx = 0; cx < maze.Width; cx++)
            {
                for (int cy = 0; cy < maze.Height; cy++)
                {
                    int gx = cx * 2 + 1;
                    int gy = cy * 2 + 1;

                    int exits = 0;
                    int closed = 0;
                    for (int i = 0; i < Directions.Length; i++)
                    {
                        Vector2Int d = Directions[i];
                        if (!maze.IsWall(gx + d.x, gy + d.y))
                        {
                            exits++;
                            continue;
                        }
                        Vector2Int neighbour = new Vector2Int(cx + d.x, cy + d.y);
                        if (neighbour.x < 0 || neighbour.y < 0 || neighbour.x >= maze.Width || neighbour.y >= maze.Height)
                            continue;
                        candidates[closed++] = neighbour;
                    }

                    if (exits != 1 || closed == 0 || rng.NextDouble() >= braiding)
                        continue;

                    Vector2Int opened = candidates[rng.Next(closed)];
                    maze.CarveBetween(cx, cy, opened.x, opened.y);
                }
            }
        }
    }
}
