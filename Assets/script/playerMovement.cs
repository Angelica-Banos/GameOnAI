using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [Tooltip("Velocidad de desplazamiento por el laberinto.")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // En un juego de vista cenital (Top-Down) desactivamos la gravedad
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        // Reiniciamos el vector de entrada cada frame
        movementInput = Vector2.zero;

        // 1. Lectura de Teclado (WASD y Flechas) con el nuevo Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                movementInput.y += 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                movementInput.y -= 1f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                movementInput.x -= 1f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                movementInput.x += 1f;
        }

        // 2. Soporte opcional para Joystick / Gamepad
        if (Gamepad.current != null && movementInput == Vector2.zero)
        {
            movementInput = Gamepad.current.leftStick.ReadValue();
        }

        // 3. Normalizar para que no se mueva más rápido en diagonal
        if (movementInput.magnitude > 1f)
        {
            movementInput = movementInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        // Aplicar velocidad en los ejes X e Y usando la API de Unity 6
        rb.linearVelocity = movementInput * moveSpeed;
    }
}
