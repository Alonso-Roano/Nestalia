using UnityEngine;

public class CustomParallaxController : MonoBehaviour
{
    [Header("Objetivo y Etiqueta")]
    [Tooltip("El objeto que la cámara de Cinemachine está siguiendo (normalmente el jugador). Se usa solo como referencia en este script.")]
    public Transform followTarget;

    [Tooltip("La etiqueta (Tag) que, si el objeto de fondo la tiene, ignora completamente el movimiento vertical (Y) de la cámara.")]
    public string tagToLockY = "FondoFijo";

    [Header("Fuerza Parallax")]
    [Tooltip("La fuerza del efecto Parallax en X. 0 = Se mueve igual que la cámara (cercano). 1 = Se mueve muy poco/estático (lejano).")]
    [Range(0f, 1f)]
    public float parallaxStrengthX = 0.5f;

    [Tooltip("La fuerza del efecto Parallax en Y. 0 = Se mueve igual que la cámara. 1 = Se mueve muy poco/estático.")]
    [Range(0f, 1f)]
    public float parallaxStrengthY = 0.5f;

    // Variables privadas para la lógica
    private Vector3 startPosition;
    private Vector3 cameraStartPosition;
    private Transform mainCameraTransform;
    private bool isYLocked = false;

    void Start()
    {
        // 1. Encontrar la cámara principal y su Transform. ⚠️ Importante para evitar NullReference.
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraTransform = mainCamera.transform;
        }
        else
        {
            // Error de seguridad si la cámara no tiene la etiqueta 'MainCamera'
            Debug.LogError("ParallaxController: No se encontró la cámara principal. Asegúrate de que un objeto Camera en la escena tenga el Tag 'MainCamera'.");
            enabled = false;
            return;
        }

        // 2. Determinar si se debe aplicar el bloqueo vertical (Eje Y)
        if (gameObject.tag == tagToLockY)
        {
            isYLocked = true;
            Debug.Log(gameObject.name + ": Bloqueo vertical (Y) activado.");
        }

        // 3. Guardar posiciones de referencia
        startPosition = transform.position;
        cameraStartPosition = mainCameraTransform.position;
    }

    void LateUpdate()
    {
        // Si la cámara no está asignada (por el error en Start), salimos para evitar la excepción.
        if (mainCameraTransform == null)
        {
            return;
        }

        // 1. Calcular cuánto se ha movido la CÁMARA desde el inicio del juego
        Vector3 deltaMovement = mainCameraTransform.position - cameraStartPosition;

        // 2. Calcular el desplazamiento del Parallax
        // (1 - Strength) define la 'cantidad' de movimiento. 0 = 100% movimiento (sigue la cámara)
        float parallaxX = deltaMovement.x * (1 - parallaxStrengthX);
        float parallaxY = deltaMovement.y * (1 - parallaxStrengthY);

        // --- Aplicar nueva posición ---

        // Calculamos la nueva posición X
        float newX = startPosition.x + parallaxX;
        
        // Calculamos la nueva posición Y
        float newY;
        if (isYLocked)
        {
            // Si tiene la etiqueta de bloqueo, la Y se queda en su posición inicial fija.
            newY = startPosition.y; 
        }
        else
        {
            // Si NO tiene la etiqueta, aplicamos el Parallax Y normal.
            newY = startPosition.y + parallaxY;
        }
        
        // Establecer la posición final
        // La línea de error '58' estaba probablemente aquí. Al comprobar 'mainCameraTransform'
        // al inicio de LateUpdate, eliminamos la posibilidad de que sea null aquí.
        transform.position = new Vector3(newX, newY, startPosition.z);
    }
}