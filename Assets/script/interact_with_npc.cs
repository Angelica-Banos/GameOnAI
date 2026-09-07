using UnityEngine;
using UnityEngine.Events;

public class interact_with_npc : MonoBehaviour
{
    [Header("Detección")]
    [Tooltip("Tag que debe tener el jugador para activar la zona.")]
    [SerializeField] private string targetTag = "Player";

    [Tooltip("Si está activo, el trigger solo funcionará una vez (útil para metas, checkpoints, etc.).")]
    [SerializeField] private bool triggerOnce = false;

    [Header("Acción a Ejecutar")]
    [Tooltip("Eventos que se dispararán cuando el jugador entre a la caja.")]
    public UnityEvent onPlayerEnter;

    private bool hasTriggered = false;

    private void Awake()
    {
        // Asegura que el BoxCollider2D esté configurado como Trigger automáticamente
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si ya se usó y está marcado para una sola vez, no hace nada
        if (triggerOnce && hasTriggered) return;

        // Comprobamos si el objeto que entró tiene la etiqueta asignada
        if (collision.CompareTag(targetTag))
        {
            hasTriggered = true;
            EjecutarAccion(collision.gameObject);
        }
    }

    /// <summary>
    /// Aquí se ejecuta la acción. Puedes agregar código directo aquí
    /// o configurarlo visualmente en el Inspector mediante onPlayerEnter.
    /// </summary>
    private void EjecutarAccion(GameObject player)
    {
        // 1. Mensaje de prueba en consola
        Debug.Log($"¡El jugador entró en la zona de: {gameObject.name}!");

        // 2. Dispara cualquier función conectada desde el Inspector
        onPlayerEnter?.Invoke();

        // 3. (Opcional) Código C# que quieras agregar en el futuro:
        // Por ejemplo:
        // - player.GetComponent<PlayerMovement>().enabled = false;
        // - Destroy(gameObject);
    }
}
