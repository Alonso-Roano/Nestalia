using UnityEngine;
using System.Collections;

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
    private bool isDead = false;
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
        ApplyKnockback(knockbackDirection, knockbackForce, knockbackUpForce);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvulnerable());
        }
    }

public void ApplyKnockback(Vector2 direction, float force, float upForce)
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Detener cualquier movimiento actual para que el knockback sea efectivo
        rb.linearVelocity = Vector2.zero;
        
        Vector2 forceVector = new Vector2(direction.x * force, upForce);
        rb.AddForce(forceVector, ForceMode2D.Impulse);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        DisableAllEnemyScripts();
        DeactivateChildren();

        var DamageableScript = GetComponent<DamageHitBox>();
        if (DamageableScript != null) Destroy(DamageableScript);
        var betleAI = GetComponent<BlettleAI>();
        if (betleAI != null) Destroy(betleAI);
        var enemyAttack_Lunge = GetComponent<EnemyAttack_Lunge>();
        if (enemyAttack_Lunge != null) Destroy(enemyAttack_Lunge);
        var enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement != null) Destroy(enemyMovement);

        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            Destroy(rb2d);
        }

        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Destroy(collider);
        }

        StartCoroutine(DieRoutine());
    }

    private void DisableAllEnemyScripts()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }
    }
    
    private void DeactivateChildren()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private IEnumerator DieRoutine()
    {
        transform.rotation = Quaternion.Euler(0, 0, 180f);

        Vector3 currentPosition = transform.position;
        currentPosition.y -= 20f;
        transform.position = currentPosition;

        yield return new WaitForSeconds(deathAnimationDelay);

        if (deathParticlePrefab != null)
        {
            ParticleSystem deathParticles = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            
            float particleDuration = deathParticles.main.duration + deathParticles.main.startLifetime.constantMax;
            
            yield return new WaitForSeconds(particleDuration);
        }

        this.enabled = false;
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