using UnityEngine;
using Unity.Cinemachine; // Asegúrate de tener Cinemachine instalado y este using activo
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class TriggerFocoCamaraConZoom : MonoBehaviour
{
    [Header("Configuración de Cinemachine")]
    [Tooltip("Arrastra aquí la 'Cinemachine Virtual Camera' que quieres controlar. Debe tener un componente 'Cinemachine Transposer' en el Body.")]
    [SerializeField] private CinemachineCamera camaraVirtual;
    private CinemachineFollow transposer;
    
    [Header("Configuración de Objetos")]
    [Tooltip("Arrastra aquí el GameObject que quieres DESACTIVAR durante la cinemática (Ej. UI del jugador).")]
    [SerializeField] private GameObject objetoADesactivar;

    [Header("Configuración de Foco Secuencial")]
    [Tooltip("Lista de objetivos que la cámara debe enfocar en secuencia.")]
    [SerializeField] private Transform[] objetivosSecuencia;
    [Tooltip("Duración TOTAL de cada foco en segundos (incluyendo zoom in/out).")]
    [SerializeField] private float duracionPorObjetivo = 3.0f;
    [Tooltip("Permitir que el trigger se active más de una vez.")]
    [SerializeField] private bool puedeRepetirse = false;

    [Header("Configuración de Zoom (Offset Z)")]
    [Tooltip("Cuánto se alejará la cámara en el eje Z. Un valor negativo más grande (ej. -50) aleja más la cámara.")]
    [SerializeField] private float zoomOutOffsetZ = -50.0f;
    [Tooltip("Porcentaje de la 'duracionPorObjetivo' usado para hacer zoom out (0.0 a 0.5).")]
    [SerializeField] [Range(0.1f, 0.5f)] private float porcentajeTiempoZoomOut = 0.4f;
    [Tooltip("Porcentaje de la 'duracionPorObjetivo' usado para hacer zoom in (0.0 a 0.5).")]
    [SerializeField] [Range(0.1f, 0.5f)] private float porcentajeTiempoZoomIn = 0.4f;
    
    private Transform objetivoOriginal_Follow;
    private Transform objetivoOriginal_LookAt;
    private Vector3 offsetOriginal;
    private bool yaEstaEnfocando = false;


    private void Awake()
    {
        if (camaraVirtual == null)
        {
            Debug.LogError("❌ No has asignado una 'Cinemachine Virtual Camera' a este trigger.");
            return;
        }

        transposer = camaraVirtual.GetComponent<CinemachineFollow>();
        if (transposer == null)
            Debug.LogError("❌ La cámara virtual asignada no tiene un componente 'CinemachineTransposer'. El zoom por offset no funcionará.");

        if (objetivosSecuencia == null || objetivosSecuencia.Length == 0)
            Debug.LogError("❌ No has asignado ningún objetivo en la secuencia de enfoque.");
        
        // Esta lógica es de tu script original:
        if (objetoADesactivar) objetoADesactivar.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !yaEstaEnfocando)
        {
            StartCoroutine(RutinaDeFocoSecuencial(other.gameObject));
        }
    }

    private IEnumerator RutinaDeFocoSecuencial(GameObject playerObjeto)
    {
        yaEstaEnfocando = true;

        // --- Obtener componentes del jugador ---
        // ¡IMPORTANTE! Cambia 'PlayerMovement' por el nombre real de tu script de movimiento
        PlayerMovement playerMovement = playerObjeto.GetComponent<PlayerMovement>();
        Rigidbody2D playerRb = playerObjeto.GetComponent<Rigidbody2D>();

        // --- Guardar estado original de la cámara ---
        objetivoOriginal_Follow = camaraVirtual.Follow;
        objetivoOriginal_LookAt = camaraVirtual.LookAt;
        if (transposer != null) offsetOriginal = transposer.FollowOffset;

        // --- Desactivar control y objetos ---
        if (objetoADesactivar != null)
            objetoADesactivar.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        // --- Recorre todos los objetivos en orden ---
        foreach (Transform objetivo in objetivosSecuencia)
        {
            if (objetivo == null) continue;

            camaraVirtual.Follow = objetivo;
            camaraVirtual.LookAt = objetivo;

            // --- INICIO: Lógica de Zoom ---

            // Si no hay transposer, no podemos hacer el zoom por offset.
            if (transposer == null)
            {
                yield return new WaitForSeconds(duracionPorObjetivo);
                continue; // Saltar al siguiente objetivo
            }

            float zOriginal = offsetOriginal.z;
            float tiempoZoomOut = duracionPorObjetivo * porcentajeTiempoZoomOut;
            float tiempoZoomIn = duracionPorObjetivo * porcentajeTiempoZoomIn;
            float tiempoEspera = duracionPorObjetivo - tiempoZoomOut - tiempoZoomIn;

            // 1. Animar Zoom Out
            float tiempoPasado = 0f;
            while (tiempoPasado < tiempoZoomOut)
            {
                float t = tiempoPasado / tiempoZoomOut;
                float nuevoZ = Mathf.Lerp(zOriginal, zoomOutOffsetZ, 1 - (1 - t) * (1 - t)); // Ease Out
                transposer.FollowOffset = new Vector3(offsetOriginal.x, offsetOriginal.y, nuevoZ);
                tiempoPasado += Time.deltaTime;
                yield return null;
            }
            transposer.FollowOffset = new Vector3(offsetOriginal.x, offsetOriginal.y, zoomOutOffsetZ); // Asegurar valor final

            // 2. Espera (si hay tiempo)
            if (tiempoEspera > 0)
                yield return new WaitForSeconds(tiempoEspera);

            // 3. Animar Zoom In (acercar la cámara en Z)
            tiempoPasado = 0f;
            while (tiempoPasado < tiempoZoomIn)
            {
                float t = tiempoPasado / tiempoZoomIn;
                float nuevoZ = Mathf.Lerp(zoomOutOffsetZ, zOriginal, t * t); // Ease In
                transposer.FollowOffset = new Vector3(offsetOriginal.x, offsetOriginal.y, nuevoZ);
                tiempoPasado += Time.deltaTime;
                yield return null;
            }
            transposer.FollowOffset = offsetOriginal; // Asegurar valor final

            // --- FIN: Lógica de Zoom ---
        }

        // --- Restaurar cámara ---
        camaraVirtual.Follow = objetivoOriginal_Follow;
        camaraVirtual.LookAt = objetivoOriginal_LookAt;
        if (transposer != null) transposer.FollowOffset = offsetOriginal;

        // SceneManager.LoadScene(5);
        // --- Restaurar control y objetos ---
        if (objetoADesactivar != null)
            objetoADesactivar.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true;

        // --- Finalizar ---
        if (!puedeRepetirse)
            gameObject.SetActive(false);

        yaEstaEnfocando = false;
    }
}