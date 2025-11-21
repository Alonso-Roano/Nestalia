using UnityEngine;
using System.Collections;
public class StatueEnemy : MonoBehaviour, AttackHitbox.IEnemyDamageable
{
    [Header("Referencias")]
    public Transform player;
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Configuración de Ataque")]
    public float detectionRange = 10f;
    public float fireRate = 2f;
    private float nextFireTime = 0f;

    [Header("Salud y Fases")]
    public int maxHealth = 100;
    public Sprite[] healthPhases;
    
    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private float originalScaleX;
    
    [Header("Efectos")]
    [Tooltip("Duración total del tambaleo al recibir daño")]
    public float wobbleDuration = 0.3f;
    [Tooltip("Ángulo máximo (en grados) del tambaleo")]
    public float wobbleAmount = 5f;
    [Tooltip("Velocidad de la vibración")]
    public float wobbleSpeed = 50f;

    private bool isWobbling = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("¡La estatua no tiene SpriteRenderer!", this);
        }
        UpdateSpritePhase();
        originalScaleX = transform.localScale.x;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (player != null)
        {
            if (player.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            }
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        Vector2 targetPosition = player.position;
        GameObject arrowGO = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        
        ArrowProjectile arrowScript = arrowGO.GetComponent<ArrowProjectile>();
        if (arrowScript != null)
        {
            arrowScript.SetTarget(targetPosition);
        }
        else
        {
            Debug.LogError("El prefab de la flecha no tiene el script ArrowProjectile!");
        }
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (!isWobbling)
        {
            StartCoroutine(WobbleEffect());
        }
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            UpdateSpritePhase();
        }
    }

    private void UpdateSpritePhase()
    {
        if (spriteRenderer == null || healthPhases.Length != 4)
        {
            Debug.LogWarning("No se pueden actualizar los sprites de la estatua.");
            return;
        }

        float healthPercentage = (float)currentHealth / maxHealth;

        if (healthPercentage > 0.66f)
        {
            spriteRenderer.sprite = healthPhases[0];
        }
        else if (healthPercentage > 0.33f)
        {
            spriteRenderer.sprite = healthPhases[1];
        }
        else
        {
            spriteRenderer.sprite = healthPhases[2];
        }
    }

    private void Die()
    {
        isDead = true;

        if (spriteRenderer != null && healthPhases.Length == 4)
        {
            spriteRenderer.sprite = healthPhases[3];
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Debug.Log("La estatua ha sido destruida.");
    }

    private IEnumerator WobbleEffect()
    {
        isWobbling = true;
        float elapsed = 0f;

        while (elapsed < wobbleDuration)
        {
            elapsed += Time.deltaTime;

            float percent = 1 - (elapsed / wobbleDuration);

            float z = Mathf.Sin(elapsed * wobbleSpeed) * (wobbleAmount * percent);

            transform.rotation = Quaternion.Euler(0, 0, z);

            yield return null;
        }

        transform.rotation = Quaternion.identity;
        isWobbling = false;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}