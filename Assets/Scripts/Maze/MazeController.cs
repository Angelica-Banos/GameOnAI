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

        public MazeSettings Settings => settings;
        public MazeData Current { get; private set; }

        void Awake()
        {
            if (mazeRenderer == null)
                Debug.LogError("MazeController: falta asignar el MazeRenderer.", this);
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
    }
}
