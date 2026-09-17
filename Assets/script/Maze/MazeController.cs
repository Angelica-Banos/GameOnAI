using System;
using UnityEngine;

namespace Maze
{
    public class MazeController : MonoBehaviour
    {
        [SerializeField] MazeSettings settings = new MazeSettings();
        [SerializeField] MazeRenderer mazeRenderer;
        [SerializeField] bool generateOnStart = true;

        readonly MazeGenerator generator = new MazeGenerator();

        public event Action<MazeData> OnMazeGenerated;

        /// Se dispara cuando el jugador entra en la zona interactuable de la entrada del laberinto.
        public event Action OnMazeEntered;

        /// Se dispara cuando el jugador entra en la zona interactuable de la salida del laberinto.
        public event Action OnMazeExited;

        public MazeSettings Settings => settings;
        public MazeData Current { get; private set; }

        /// Convierte una celda lógica del laberinto a una posición del mundo.
        public Vector3 CellToWorldPosition(Vector2Int cell)
        {
            return mazeRenderer.CellToWorldPosition(cell.x, cell.y);
        }

        /// Convierte una posición del mundo a la celda lógica del laberinto más cercana.
        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            return mazeRenderer.WorldToCell(worldPosition);
        }

        void Awake()
        {
            if (mazeRenderer == null)
            {
                Debug.LogError("MazeController: falta asignar el MazeRenderer.", this);
                return;
            }

            mazeRenderer.OnPlayerEnteredMaze += () => OnMazeEntered?.Invoke();
            mazeRenderer.OnPlayerExitedMaze += () => OnMazeExited?.Invoke();
        }

        void Start()
        {
            if (generateOnStart && mazeRenderer != null)
                Generate();
        }

        [ContextMenu("Regenerar")]
        public void Generate()
        {
            if (settings.useRandomSeed)
                settings.seed = Environment.TickCount;

            Current = generator.Generate(settings.width, settings.height, settings.seed, settings);

            if (mazeRenderer != null)
                mazeRenderer.Draw(Current);

            OnMazeGenerated?.Invoke(Current);
        }

        [ContextMenu("Nueva semilla aleatoria")]
        public void NewRandomSeed()
        {
            settings.seed = Environment.TickCount;
        }

        /// Regenera el laberinto con una semilla fija, para reiniciar el mismo laberinto en vez de crear uno nuevo.
        public void GenerateWithSeed(int seed)
        {
            settings.useRandomSeed = false;
            settings.seed = seed;
            Generate();
        }
    }
}
