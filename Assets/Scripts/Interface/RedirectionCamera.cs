using UnityEngine;
using Unity.Cinemachine; // Asegúrate de tener Cinemachine instalado y este using activo
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class TriggerFocoCamara : MonoBehaviour
{
    [Header("Configuración de Cinemachine")]
    [Tooltip("Arrastra aquí la 'Cinemachine Virtual Camera' que quieres controlar.")]
    [SerializeField] private CinemachineCamera camaraVirtual;

    // --- NUEVO ---
    [Header("Configuración de Objetos")]
    [Tooltip("Arrastra aquí el GameObject que quieres DESACTIVAR durante la cinemática (Ej. UI del jugador).")]
    [SerializeField] private GameObject objetoADesactivar;
    // -------------

    [Header("Configuración de Foco Secuencial")]
    [Tooltip("Lista de objetivos que la cámara debe enfocar en secuencia.")]
    [SerializeField] private Transform[] objetivosSecuencia;
    [Tooltip("Duración de cada foco en segundos.")]
    [SerializeField] private float duracionPorObjetivo = 3.0f;
    [Tooltip("Permitir que el trigger se active más de una vez.")]
    [SerializeField] private bool puedeRepetirse = false;

    private Transform objetivoOriginal_Follow;
    private Transform objetivoOriginal_LookAt;
    private bool yaEstaEnfocando = false;

    // (Variables de TimeScale, Brain y Animator eliminadas)

    private void Awake()
    {
        if (camaraVirtual == null)
            Debug.LogError("❌ No has asignado una 'Cinemachine Virtual Camera' a este trigger.");

        if (objetivosSecuencia == null || objetivosSecuencia.Length == 0)
            Debug.LogError("❌ No has asignado ningún objetivo en la secuencia de enfoque.");
        if (objetoADesactivar) objetoADesactivar.SetActive(false);
        // (Búsqueda de CinemachineBrain eliminada)
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pasamos el GameObject del jugador (other.gameObject) a la corutina
        if (other.CompareTag("Player") && !yaEstaEnfocando)
        {
            StartCoroutine(RutinaDeFocoSecuencial(other.gameObject));
        }
    }

    // La corutina ahora recibe el GameObject del jugador
    private IEnumerator RutinaDeFocoSecuencial(GameObject playerObjeto)
    {
        yaEstaEnfocando = true;

        // --- NUEVO: Obtener componentes del jugador ---
        // ¡IMPORTANTE! Cambia 'PlayerMovement' por el nombre real de tu script de movimiento
        PlayerMovement playerMovement = playerObjeto.GetComponent<PlayerMovement>();
        Rigidbody2D playerRb = playerObjeto.GetComponent<Rigidbody2D>();
        // ----------------------------------------------

        // Guardar estado original de la cámara
        objetivoOriginal_Follow = camaraVirtual.Follow;
        objetivoOriginal_LookAt = camaraVirtual.LookAt;

        // --- NUEVO: Desactivar control y objetos ---
        if (objetoADesactivar != null)
            objetoADesactivar.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Detener completamente al jugador
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
        // -------------------------------------------

        // (Bloque de 'detenerTiempoDuranteFoco' eliminado)

        // Recorre todos los objetivos en orden
        foreach (Transform objetivo in objetivosSecuencia)
        {
            if (objetivo == null) continue;

            camaraVirtual.Follow = objetivo;
            camaraVirtual.LookAt = objetivo;

            // --- MODIFICADO: Usar WaitForSeconds normal ---
            // La corutina ahora espera en tiempo de juego normal
            yield return new WaitForSeconds(duracionPorObjetivo);
        }

        // Restaurar cámara
        camaraVirtual.Follow = objetivoOriginal_Follow;
        camaraVirtual.LookAt = objetivoOriginal_LookAt;

        // --- NUEVO: Restaurar control y objetos ---
        if (objetoADesactivar != null)
            objetoADesactivar.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true; // Reactiva el script de movimiento
        // -----------------------------------------

        // (Bloque de 'Restaurar tiempo' eliminado)

        // Si no puede repetirse, desactiva el objeto trigger
        if (!puedeRepetirse)
            gameObject.SetActive(false);

        yaEstaEnfocando = false;
    }

}