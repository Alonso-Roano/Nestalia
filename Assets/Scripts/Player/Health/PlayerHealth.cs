using System;
using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    private const string ERROR_CHECKPOINT_TAG = "EnemyStatic";
    [Header("Vida")]
    public int maxHealth = 100;
    public int currentHealth;
    [Header("Animación de Daño")]
    [Tooltip("Duración total del efecto de parpadeo y color rojo.")]
    public float damageFlashDuration = 0.5f;
    [Tooltip("Intervalo de tiempo entre cada parpadeo.")]
    public float flashInterval = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isInvulnerable = false;
   
    private Color softRed = new Color(1.0f, 0.5f, 0.5f, 1.0f);
    [Header("Knockback")]
    [Tooltip("Fuerza base de empuje horizontal.")]
    public float knockForce = 100f;
    [Tooltip("Fuerza vertical (hacia arriba) del knockback.")]
    public float knockUpForce = 50f;
    public event Action<float> OnHealthPercentChanged;
    private Rigidbody2D rb;
    private PlayerController controller;

    // === NUEVO: Referencia a Stats Manager (se auto-asigna) ===
    private PlayerStatsManager statsManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // === NUEVO: Encuentra StatsManager ===
        statsManager = PlayerStatsManager.Instance;
    }

    void Start()
    {
        currentHealth = maxHealth; // === MEJORADO: Inicializa health ===
        OnHealthPercentChanged?.Invoke((float)currentHealth / maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(int amount, Transform damageSource = null)
    {
        if (isInvulnerable) return;

        // === NUEVO: Incrementa hitsTaken INMEDIATAMENTE y guarda ===
        if (statsManager != null)
        {
            statsManager.IncrementHitsTaken();
            Debug.Log($"¡Daño recibido! ({amount}) | Hits totales: {statsManager.hitsTaken} | Health: {currentHealth - amount}/{maxHealth}");
        }
        else
        {
            Debug.Log("Recibí " + amount + " de daño (StatsManager no encontrado)");
        }

        bool willDie = (currentHealth - amount) <= 0;
        if (damageSource != null && damageSource.CompareTag(ERROR_CHECKPOINT_TAG))
        {
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(WaitAndTeleport(amount));
            StartCoroutine(DamageFlashRoutine());
            Debug.Log("Checkpoint Error Activado! Esperando para teletransportar");
            return;
        }
      
        if (damageSource != null && !willDie)
        {
            float horizontalDirection = (transform.position.x - damageSource.position.x) > 0 ? 1 : -1;
            Vector2 knockDir = new Vector2(horizontalDirection * knockForce, knockUpForce);
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockDir, ForceMode2D.Impulse);
        }

        SetHealth(currentHealth - amount); // === MOVIDO: Ahora SetHealth maneja muerte ===

        if (currentHealth <= 0)
        {
            // === ELIMINADO: Ya se maneja en SetHealth ===
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        isInvulnerable = true;
        float startTime = Time.time;
        while (Time.time < startTime + damageFlashDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = (spriteRenderer.color == softRed) ? originalColor : softRed;
            }
            yield return new WaitForSeconds(flashInterval);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        isInvulnerable = false;
    }

    private void TeleportToErrorCheckpoint()
    {
        Vector3 respawnPos = CheckpointErrorManager.Instance.GetErrorRespawnPosition();
        transform.position = respawnPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void Heal(int amount)
    {
        SetHealth(currentHealth + amount);
    }

    // === MODIFICADO: SetHealth ahora detecta muertes y guarda stats ===
    public void SetHealth(int newHealth)
    {
        int previousHealth = currentHealth;
        int clampedHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        
        if (clampedHealth == currentHealth) return;

        // === NUEVO: Detecta si murió (health <= 0) ===
        bool justDied = (previousHealth > 0 && clampedHealth <= 0);
        currentHealth = clampedHealth;
        OnHealthPercentChanged?.Invoke((float)currentHealth / maxHealth);

        // === NUEVO: Si murió, incrementa deaths ANTES del respawn ===
        if (justDied && statsManager != null)
        {
            statsManager.IncrementDeaths();
            Debug.Log($"¡MUERTE! | Deaths totales: {statsManager.deaths} | Respawneando...");
        }
        else if (statsManager != null)
        {
            Debug.Log($"Health actualizado: {currentHealth}/{maxHealth} ({(float)currentHealth/maxHealth:P0})");
        }

        // === MEJORADO: Respawn solo si murió ===
        if (currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            controller.RespawnController.Respawn();
            SetHealth(maxHealth); // Recursivo pero seguro (clampedHealth ya es 0)
        }
    }

    private IEnumerator WaitAndTeleport(int damageAmount)
    {
        yield return new WaitForSeconds(0.15f);
        TeleportToErrorCheckpoint();
        
        SetHealth(currentHealth - damageAmount); // Aplica daño DESPUÉS del teletransporte
        
        Debug.Log("Teletransporte completado después de la espera.");
    }
}