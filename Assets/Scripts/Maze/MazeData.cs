namespace Maze
{
    // Rejilla doble: las coordenadas impares son celdas y las pares son las
    // paredes entre ellas. La celda (cx, cy) vive en (2*cx+1, 2*cy+1), así que
    // la pared que separa dos celdas vecinas es su punto medio.
    public class MazeData
    {
        readonly bool[,] grid;

        public int Width { get; }
        public int Height { get; }
        public int GridWidth => Width * 2 + 1;
        public int GridHeight => Height * 2 + 1;

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
    }
}
