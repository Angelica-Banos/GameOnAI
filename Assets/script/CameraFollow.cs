using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Transform del objeto que la cámara debe seguir (el jugador).")]
    [SerializeField] private Transform target;

    [Header("Ajustes de Seguimiento")]
    [Tooltip("Distancia relativa entre la cámara y el jugador (Z suele ser -10 en 2D).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);

    [Tooltip("Tiempo que tarda la cámara en alcanzar al jugador. Menor número = más rápida.")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothTime = 0.25f;

    private Vector3 currentVelocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada sumando el desfase (offset)
        Vector3 targetPosition = target.position + offset;

        // Desplazamiento suave con amortiguación
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}