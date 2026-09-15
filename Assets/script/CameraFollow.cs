using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Transform del objeto que la c�mara debe seguir (el jugador).")]
    [SerializeField] private Transform target;

    [Header("Ajustes de Seguimiento")]
    [Tooltip("Distancia relativa entre la c�mara y el jugador (Z suele ser -10 en 2D).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);

    [Tooltip("Tiempo que tarda la c�mara en alcanzar al jugador. Menor n�mero = m�s r�pida.")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothTime = 0.25f;

    private Vector3 currentVelocity = Vector3.zero;

    /// Permite reasignar el objetivo en tiempo de ejecución (por ejemplo, cuando el GameManager instancia al jugador).
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Posici�n deseada sumando el desfase (offset)
        Vector3 targetPosition = target.position + offset;

        // Desplazamiento suave con amortiguaci�n
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}