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

    private int currentHealth;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private EnemyMovement enemyMovement; // Referencia al componente de movimiento

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        
        // Notifica a otros componentes que se ha recibido daño (útil para interrumpir ataques)
        // Por ejemplo, puedes enviar un mensaje: BroadcastMessage("OnDamageTaken", SendMessageOptions.DontRequireReceiver);

        // Calcula la dirección del knockback
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;
        
        // Llama al componente de movimiento para aplicar el knockback
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
        // Aquí podrías añadir efectos de muerte, drop de items, etc.
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