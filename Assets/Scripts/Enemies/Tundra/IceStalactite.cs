using System.Collections;
using UnityEngine;

public class IceStalactite : MonoBehaviour
{
    [Header("Configuración de Detección")]
    [SerializeField] private LayerMask playerLayer; // La capa donde está el jugador
    [SerializeField] private float detectionDistance = 10f; // Qué tan largo es el rayo
    [SerializeField] private float fallDelay = 0.3f; // Un pequeño retraso antes de caer

    [Header("Configuración de Daño")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float gravityScaleOnFall = 2.5f; // Para que caiga más rápido

    [Header("Configuración de Temblor")] // <--- NUEVO HEADER
    [SerializeField] private float shakeDuration = 0.2f; // Cuánto tiempo dura el temblor
    [SerializeField] private float shakeMagnitude = 0.05f; // Qué tan fuerte se mueve
    [SerializeField] private float shakeSpeed = 10f; // Qué tan rápido tiembla
    private Vector3 initialPosition; // Para guardar la posición original

    private Rigidbody2D rb;
    private bool isFalling = false;
    private bool isShaking = false; // <--- NUEVA VARIABLE

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        initialPosition = transform.position; // Guardamos la posición inicial
    }

    void Update()
    {
        if (isFalling || isShaking) // <--- Añadido isShaking
        {
            return;
        }

        CheckForPlayer();
    }

    private void CheckForPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, detectionDistance, playerLayer);

        if (hit.collider != null)
        {
            StartCoroutine(PrepareToFall()); // <--- Cambiado a una nueva corrutina
        }
    }

    // <--- NUEVA CORRUTINA PARA MANEJAR EL TEMBLOR Y LUEGO LA CAÍDA
    private IEnumerator PrepareToFall()
    {
        isShaking = true; // Marcamos que está temblando
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Mathf.Sin(Time.time * shakeSpeed) * shakeMagnitude;
            transform.position = new Vector3(x, originalPosition.y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null; // Espera un frame
        }

        transform.position = originalPosition; // Vuelve a la posición original después del temblor
        isShaking = false; // Ya no está temblando

        // Ahora iniciamos la caída después del retraso
        FallStalactite();
    }


    private void FallStalactite()
    {
        isFalling = true;
        // El retraso 'fallDelay' ya se considera parte del 'shakeDuration' o puede ser extra si lo deseas
        // Por simplicidad, ya que el temblor es el "aviso", no necesitamos un delay adicional aquí a menos que quieras más tiempo.
        // Si quieres un delay extra DESPUÉS del temblor, podrías añadir:
        // yield return new WaitForSeconds(fallDelay); 

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScaleOnFall;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                }
                Destroy(gameObject); // Destruye el objeto principal de la estalactita
            }
            if (collision.gameObject.CompareTag("Ground"))
            {
                Destroy(gameObject); // Destruye el objeto principal de la estalactita
            }
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(gameObject); // Destruye el objeto principal de la estalactita
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.down * detectionDistance);
    }
}