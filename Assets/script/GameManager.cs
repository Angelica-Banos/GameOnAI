using System.Collections.Generic;
using Maze;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MazeController mazeController;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Prefabs")]
    [Tooltip("Se instancia en la celda de entrada del laberinto.")]
    [SerializeField] private GameObject playerPrefab;

    [Tooltip("Se instancia en la celda de salida; se suma uno por etapa.")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Etapas")]
    [SerializeField]
    private StageDefinition[] stages =
    {
        new StageDefinition { seed = 1, width = 8,  height = 8,  straightness = 0.6f, braiding = 0.4f },
        new StageDefinition { seed = 2, width = 10, height = 10, straightness = 0.5f, braiding = 0.3f },
        new StageDefinition { seed = 3, width = 12, height = 12, straightness = 0.4f, braiding = 0.2f },
        new StageDefinition { seed = 4, width = 14, height = 14, straightness = 0.3f, braiding = 0.1f },
        new StageDefinition { seed = 5, width = 16, height = 16, straightness = 0.2f, braiding = 0.05f },
    };

    private GameObject playerInstance;
    private readonly List<GameObject> enemyInstances = new List<GameObject>();
    private int stageIndex;

    /// Datos importantes de la partida actual (etapa, semilla y celdas de entrada/salida).
    public RunData CurrentRun { get; private set; }

    [System.Serializable]
    public class StageDefinition
    {
        public int seed;
        public int width = 10;
        public int height = 10;
        [Range(0f, 1f)] public float straightness = 0.5f;
        [Range(0f, 1f)] public float braiding = 0.3f;
    }

    [System.Serializable]
    public class RunData
    {
        public int stage;
        public int seed;
        public Vector2Int entranceCell;
        public Vector2Int exitCell;
    }

    private void Awake()
    {
        if (mazeController == null)
            Debug.LogError("GameManager: falta asignar el MazeController.", this);
    }

    private void Start()
    {
        LoadStage(0);
    }

    private void OnEnable()
    {
        if (mazeController == null)
            return;

        mazeController.OnMazeGenerated += HandleMazeGenerated;
        mazeController.OnMazeExited += HandleMazeExited;
    }

    private void OnDisable()
    {
        if (mazeController == null)
            return;

        mazeController.OnMazeGenerated -= HandleMazeGenerated;
        mazeController.OnMazeExited -= HandleMazeExited;
    }

    // Aplica las características de la etapa y genera su laberinto (misma seed = mismo laberinto).
    public void LoadStage(int index)
    {
        stageIndex = Mathf.Clamp(index, 0, stages.Length - 1);
        StageDefinition stage = stages[stageIndex];

        MazeSettings settings = mazeController.Settings;
        settings.width = stage.width;
        settings.height = stage.height;
        settings.straightness = stage.straightness;
        settings.braiding = stage.braiding;

        mazeController.GenerateWithSeed(stage.seed);
    }

    /// Reinicia la etapa actual (misma seed) tras morir.
    [ContextMenu("Reiniciar etapa")]
    public void RestartLevel()
    {
        LoadStage(stageIndex);
    }

    private void HandleMazeExited()
    {
        LoadStage(stageIndex + 1);
    }

    private void HandleMazeGenerated(MazeData maze)
    {
        CurrentRun = new RunData
        {
            stage = stageIndex,
            seed = maze.Seed,
            entranceCell = maze.Start,
            exitCell = maze.Exit
        };

        SpawnPlayer(maze.Start);
        SpawnEnemies(maze.Exit, stageIndex + 1);
    }

    private void SpawnPlayer(Vector2Int cell)
    {
        if (playerPrefab == null)
            return;

        Vector3 position = mazeController.CellToWorldPosition(cell);

        if (playerInstance == null)
            playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
        else
            playerInstance.transform.position = position;

        if (cameraFollow != null)
            cameraFollow.SetTarget(playerInstance.transform);
    }

    private void SpawnEnemies(Vector2Int cell, int count)
    {
        if (enemyPrefab == null)
            return;

        while (enemyInstances.Count < count)
            enemyInstances.Add(Instantiate(enemyPrefab));

        while (enemyInstances.Count > count)
        {
            int last = enemyInstances.Count - 1;
            Destroy(enemyInstances[last]);
            enemyInstances.RemoveAt(last);
        }

        Vector3 position = mazeController.CellToWorldPosition(cell);
        foreach (GameObject enemy in enemyInstances)
        {
            enemy.transform.position = position;

            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && playerInstance != null)
                ai.Initialize(mazeController, playerInstance.transform);
        }
    }

    [ContextMenu("Loggear datos de la partida")]
    private void LogRunData()
    {
        if (CurrentRun == null)
        {
            Debug.Log("GameManager: todavía no hay una partida en curso.", this);
            return;
        }

        Debug.Log($"GameManager: etapa={CurrentRun.stage + 1} seed={CurrentRun.seed} entrada={CurrentRun.entranceCell} salida={CurrentRun.exitCell}", this);
    }
}
