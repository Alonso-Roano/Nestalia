using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SimpleProjectile : MonoBehaviour
{
    [Tooltip("Tiempo en segundos antes de que el proyectil se autodestruya si no golpea nada.")]
    [SerializeField] private float lifetime = 5.0f;
    [SerializeField] private int damage = 5;
    
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Autodestrucción para no llenar la escena de proyectiles perdidos
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Inicia el movimiento del proyectil.
    /// </summary>
    public void Launch(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * speed;

        // Opcional: Rotar el proyectil para que "mire" hacia donde se mueve
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Maneja las colisiones
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si golpea al jugador
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        
        // Si golpea el escenario (ej. una capa "Ground" o "Wall")
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Destruye el proyectil
            Destroy(gameObject);
        }
    }
}