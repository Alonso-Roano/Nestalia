using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    private int currentHealth;

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
        currentHealth = maxHealth;
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
        Debug.Log("Recibí " + amount + " de daño");

        if (damageSource != null)
        {
            // Calcula la dirección del empuje
            float horizontalDirection = (transform.position.x - damageSource.position.x) > 0 ? 1 : -1;
            Vector2 knockDir = new Vector2(horizontalDirection * knockForce, knockUpForce);

            // Aplica el knockback
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockDir, ForceMode2D.Impulse);
        }

        SetHealth(currentHealth - amount);

        if (currentHealth <= 0)
        {
            controller.RespawnController.Respawn();
            SetHealth(maxHealth);
        }
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
}