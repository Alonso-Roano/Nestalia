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

    private bool isWobbling = false;

    // Activado solo cuando es visible
    private bool aiEnabled = false;

    // Guardamos componentes opcionales para apagarlos
    private Collider2D col;
    private Rigidbody2D rb;
    private Animator anim;
    private ParticleSystem[] particles;

    [Header("Efectos")]
    public float wobbleDuration = 0.3f;
    public float wobbleAmount = 5f;
    public float wobbleSpeed = 50f;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (!spriteRenderer)
            Debug.LogError("La estatua no tiene SpriteRenderer!", this);

        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        particles = GetComponentsInChildren<ParticleSystem>();

        UpdateSpritePhase();
        originalScaleX = transform.localScale.x;
    }

    // ---------------- VISIBILIDAD ----------------

    private void OnBecameVisible()
    {
        aiEnabled = true;

        if (col) col.enabled = true;
        if (rb) rb.simulated = true;
        if (anim) anim.enabled = true;

        foreach (var p in particles)
            p.Play();
    }

    private void OnBecameInvisible()
    {
        aiEnabled = false;

        StopWobble();
        transform.rotation = Quaternion.identity;

        // Apagamos TODO
        if (col) col.enabled = false;
        if (rb) rb.simulated = false;
        if (anim) anim.enabled = false;

        foreach (var p in particles)
            p.Pause();
    }

    // ----------------- UPDATE ---------------------

    void FixedUpdate()
    {
        if (!aiEnabled || isDead) return;

        if (player != null)
        {
            // Girar hacia el jugador
            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    // ----------------- ATAQUE ----------------------

    void Fire()
    {
        if (!aiEnabled) return;

        Vector2 targetPosition = player.position;
        GameObject arrowGO = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        ArrowProjectile arrowScript = arrowGO.GetComponent<ArrowProjectile>();
        if (arrowScript != null)
            arrowScript.SetTarget(targetPosition);
        else
            Debug.LogError("El prefab de flecha no tiene ArrowProjectile!");
    }

    // ---------------- DAÑO -------------------------

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        if (isDead || !aiEnabled) return;

        currentHealth -= damage;

        if (!isWobbling)
            StartCoroutine(WobbleEffect());

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
        if (!spriteRenderer || healthPhases.Length != 4) return;

        float hp = (float)currentHealth / maxHealth;

        if (hp > 0.66f)
            spriteRenderer.sprite = healthPhases[0];
        else if (hp > 0.33f)
            spriteRenderer.sprite = healthPhases[1];
        else
            spriteRenderer.sprite = healthPhases[2];
    }

    private void Die()
    {
        isDead = true;

        if (spriteRenderer && healthPhases.Length == 4)
            spriteRenderer.sprite = healthPhases[3];

        if (col) col.enabled = false;
        if (rb) rb.simulated = false;
        if (anim) anim.enabled = false;

        Debug.Log("La estatua ha sido destruida.");
    }

    // ---------------- EFECTO WOBBLE ----------------

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

    private void StopWobble()
    {
        StopAllCoroutines();
        isWobbling = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}