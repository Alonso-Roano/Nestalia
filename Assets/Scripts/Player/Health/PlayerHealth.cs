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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Start()
    {
        OnHealthPercentChanged?.Invoke((float)currentHealth / maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void TakeDamage(int amount, Transform damageSource = null)
    {
        if (isInvulnerable) return;

        Debug.Log("Recibí " + amount + " de daño");

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

        SetHealth(currentHealth - amount);
        StartCoroutine(DamageFlashRoutine());

        if (currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            controller.RespawnController.Respawn();
            SetHealth(maxHealth);
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

    public void SetHealth(int newHealth)
    {
        int clampedHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        if (clampedHealth == currentHealth) return;
        currentHealth = clampedHealth;
        OnHealthPercentChanged?.Invoke((float)currentHealth / maxHealth);
    }
    
    private IEnumerator WaitAndTeleport(int damageAmount)
    {
        yield return new WaitForSeconds(0.15f);

        TeleportToErrorCheckpoint();
        
        SetHealth(currentHealth - damageAmount); 

        if (currentHealth <= 0)
        {
            controller.RespawnController.Respawn();
            SetHealth(maxHealth);
        }

        Debug.Log("Teletransporte completado después de la espera.");
    }
}