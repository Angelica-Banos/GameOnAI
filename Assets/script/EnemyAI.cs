using System.Collections.Generic;
using Maze;
using UnityEngine;

// Persigue al jugador por los pasillos del laberinto (BFS), moviéndose cinemáticamente.
public class EnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MazeController mazeController;
    [SerializeField] private Transform player;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float repathInterval = 0.4f;

    [Header("Predicción y aleatoriedad")]
    [SerializeField] private float predictionTime = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float wanderChance = 0.15f;
    [SerializeField] private int wanderRadius = 3;

    private Rigidbody2D rb;
    private Rigidbody2D playerRb;
    private GameManager gameManager;
    private Animator animator;

    // currentCell es la fuente de verdad: solo avanza al llegar a una celda,
    // nunca se recalcula desde la posición flotante (eso causaba el tembleque).
    private Vector2Int currentCell;
    private List<Vector2Int> path;
    private int pathIndex;
    private float repathTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        animator = GetComponent<Animator>();

        gameManager = FindFirstObjectByType<GameManager>();
        moveSpeed *= Random.Range(0.85f, 1.15f);

        FindPlayerIfMissing();
        if (mazeController != null && player != null)
            SnapCurrentCell();
    }

    // Llamado por el GameManager tras instanciar el enemigo.
    public void Initialize(MazeController controller, Transform playerTransform)
    {
        mazeController = controller;
        player = playerTransform;
        playerRb = player.GetComponent<Rigidbody2D>();
        SnapCurrentCell();
    }

    private void SnapCurrentCell()
    {
        currentCell = mazeController.WorldToCell(transform.position);
        path = null;
        pathIndex = 0;
        repathTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (DialogueManager.IsDialogueActive)
        {
            UpdateAnimation(Vector2.zero);
            return;
        }

        if (mazeController == null || mazeController.Current == null)
            return;

        if (player == null)
        {
            FindPlayerIfMissing();
            if (player == null)
                return;
        }

        MazeData maze = mazeController.Current;
        repathTimer -= Time.fixedDeltaTime;

        bool pathFinished = path == null || pathIndex >= path.Count;
        bool timeToReconsider = repathTimer <= 0f;

        if (pathFinished || timeToReconsider)
        {
            Vector2Int target = ComputeTargetCell(maze);
            bool targetChanged = path == null || path.Count == 0 || path[path.Count - 1] != target;

            if (pathFinished || targetChanged)
            {
                path = maze.FindPath(currentCell, target);
                pathIndex = 1; // path[0] es la celda actual, no hace falta "llegar" a ella
            }

            if (timeToReconsider)
                repathTimer = repathInterval;
        }

        MoveAlongPath();
    }

    private Vector2Int ComputeTargetCell(MazeData maze)
    {
        Vector2 velocity = playerRb != null ? playerRb.linearVelocity : Vector2.zero;
        Vector2 predicted = (Vector2)player.position + velocity * predictionTime;
        Vector2Int cell = ClampToMaze(mazeController.WorldToCell(predicted), maze);

        if (Random.value < wanderChance)
        {
            Vector2Int offset = new Vector2Int(Random.Range(-wanderRadius, wanderRadius + 1), Random.Range(-wanderRadius, wanderRadius + 1));
            cell = ClampToMaze(cell + offset, maze);
        }

        return cell;
    }

    private void MoveAlongPath()
    {
        if (path == null || pathIndex >= path.Count)
        {
            UpdateAnimation(Vector2.zero);
            return;
        }

        Vector2Int step = path[pathIndex];
        Vector3 waypoint = mazeController.CellToWorldPosition(step);
        Vector3 next = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        Vector2 direction = (next - transform.position).normalized;
        UpdateAnimation(direction);

        if (Vector3.Distance(next, waypoint) < 0.05f)
        {
            currentCell = step;
            pathIndex++;
        }
    }

    private void UpdateAnimation(Vector2 dir)
    {
        if (animator != null)
        {
            if (dir != Vector2.zero)
            {
                animator.SetFloat("MoveX", dir.x);
                animator.SetFloat("MoveY", dir.y);
                animator.SetBool("IsMoving", true);
            }
            else
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    private static Vector2Int ClampToMaze(Vector2Int cell, MazeData maze)
    {
        return new Vector2Int(Mathf.Clamp(cell.x, 0, maze.Width - 1), Mathf.Clamp(cell.y, 0, maze.Height - 1));
    }

    private void FindPlayerIfMissing()
    {
        if (player != null)
            return;

        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found == null)
            return;

        player = found.transform;
        playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryCatchPlayer(collision.gameObject);
    private void OnTriggerEnter2D(Collider2D collision) => TryCatchPlayer(collision.gameObject);

    private void TryCatchPlayer(GameObject other)
    {
        if (other.CompareTag("Player") && gameManager != null)
            gameManager.RestartLevel();
    }
}
