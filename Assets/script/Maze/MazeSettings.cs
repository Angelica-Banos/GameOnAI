using UnityEngine;

namespace Maze
{
    [System.Serializable]
    public class MazeSettings
    {
        [Header("Tamaño")]
        [Tooltip("Número de celdas lógicas en horizontal.")]
        [Range(2, 100)] public int width = 20;

        [Tooltip("Número de celdas lógicas en vertical.")]
        [Range(2, 100)] public int height = 20;

        [Header("Aleatoriedad")]
        [Tooltip("Si está activo, se usa una semilla nueva en cada generación. Si no, se usa la semilla fijada abajo.")]
        public bool useRandomSeed = true;

        [Tooltip("Semilla del generador. La misma semilla produce siempre el mismo laberinto.")]
        public int seed = 0;

        [Header("Forma")]
        [Tooltip("Probabilidad de seguir recto al excavar. 0 = giros constantes, 1 = pasillos largos.")]
        [Range(0f, 1f)] public float straightness = 0.5f;

        [Tooltip("Probabilidad de abrir una pared extra en cada callejón sin salida, creando ciclos. 0 = laberinto perfecto.")]
        [Range(0f, 1f)] public float braiding = 0.3f;
    }
}
