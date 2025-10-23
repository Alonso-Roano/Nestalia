using UnityEngine;
using System.Collections;

// Asegura que los otros componentes necesarios estén presentes
[RequireComponent(typeof(EnemyMovement))]
public class EnemyHealth : MonoBehaviour, AttackHitbox.IEnemyDamageable
{
    [Header("Estadísticas de Salud")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    [Header("Knockback al Recibir Daño")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackUpForce = 5f;

    [Header("Efectos de Muerte")]
    [SerializeField] private ParticleSystem deathParticlePrefab;
    [SerializeField] private float deathAnimationDelay = 0.3f; 

    private int currentHealth;
    private bool isInvulnerable = false;
    private bool isDead = false; // Flag para evitar que la muerte se ejecute varias veces
    private SpriteRenderer spriteRenderer;
    private EnemyMovement enemyMovement;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;

        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;
        enemyMovement.ApplyKnockback(knockbackDirection, knockbackForce, knockbackUpForce);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvulnerable());
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Desactivamos componentes para que el enemigo deje de interactuar
        // Esto incluye la IA, colliders y el movimiento.
        var aiScript = GetComponent<BlettleAI>(); // Asume que el script de IA se llama BlettleAI
        if (aiScript != null) aiScript.enabled = false;
        var DamageableScript = GetComponent<DamageHitBox>();
        if (DamageableScript != null) DamageableScript.enabled = false;
        
        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.useFullKinematicContacts = true; // Evita que la gravedad o fuerzas lo afecten
        }

        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        
        enemyMovement.Stop();
        
        // Iniciamos la corrutina que maneja la animación y los efectos
        StartCoroutine(DieRoutine());
    }

    /// <summary>
    /// Corrutina que maneja la animación de muerte, partículas y destrucción del objeto.
    /// </summary>
    private IEnumerator DieRoutine()
    {
        // 1. Voltear el modelo 180 grados sobre el eje Z
        transform.rotation = Quaternion.Euler(0, 0, 180f);

        // 2. Esperar un momento para que el volteo sea visible
        yield return new WaitForSeconds(deathAnimationDelay);

        // 3. Activar partículas y ocultar el sprite
        if (deathParticlePrefab != null)
        {
            // Ocultar el sprite original
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Crear la instancia del sistema de partículas en la posición del enemigo
            ParticleSystem deathParticles = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            
            // Calculamos la duración total del efecto para saber cuánto esperar
            float particleDuration = deathParticles.main.duration + deathParticles.main.startLifetime.constantMax;
            
            // Esperamos a que las partículas terminen
            yield return new WaitForSeconds(particleDuration);
        }

        // 4. Finalmente, destruir el objeto del enemigo
        Destroy(gameObject);
    }

    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(invulnerabilityTime / 6);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(invulnerabilityTime / 6);
            }
        }
        isInvulnerable = false;
    }
}