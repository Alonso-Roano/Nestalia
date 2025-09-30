using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;

    // Vida del jugador
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    // Evento para UI u otros scripts que reaccionan al % de salud
    public event Action<float> OnHealthPercentChanged;

    // Habilidades desbloqueadas
    public bool hasDoubleJump;
    public bool hasSlowFall;
    public bool hasWallClimb;
    [Header("Knockback")]
    [Tooltip("Fuerza base de empuje horizontal.")]
    public float knockForce = 100f;
    [Tooltip("Fuerza vertical (hacia arriba) del knockback.")]
    public float knockUpForce = 50f;

    private BackgroundChanger backgroundChanger;
    private HealthBar healthBar;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        LoadPlayerData();
    }

    // === GUARDADO ===
    public void SavePlayerData()
    {
        GameData data = new GameData();

        // guardo posición actual como checkpoint
        data.checkpointPos = transform.position;

        // guardo vida
        data.maxHealth = maxHealth;

        // guardo habilidades
        data.hasDoubleJump = hasDoubleJump;
        data.hasSlowFall = hasSlowFall;
        data.hasWallClimb = hasWallClimb;

        // aquí podrías guardar inventario, estadísticas, score, etc.
        DataManager.Instance.SaveGame(data);
        Debug.Log("Datos del jugador guardados");
    }

    // === CARGADO ===
    public void LoadPlayerData()
    {
        GameData loadedData = DataManager.Instance.LoadGame();
        if (loadedData != null)
        {
            // restauramos posición
            transform.position = loadedData.checkpointPos;

            // restauramos vida
            SetHealth(maxHealth);

            // restauramos habilidades
            hasDoubleJump = loadedData.hasDoubleJump;
            hasSlowFall = loadedData.hasSlowFall;
            hasWallClimb = loadedData.hasWallClimb;

            Debug.Log("Datos del jugador cargados");
        }
    }

    public void ResetPlayerData()
    {
        DataManager.Instance.ResetGame();
        transform.position = Vector3.zero;
        maxHealth = 100;
        SetHealth(maxHealth);

        hasDoubleJump = false;
        hasSlowFall = false;
        hasWallClimb = false;

        Debug.Log("Juego y datos del jugador restablecidos.");
    }

    // === VIDA / DAÑO ===
    public void TakeDamage(int amount, Transform source = null)
    {
        Debug.Log("Recibí " + amount + " de daño");

        if (source != null)
        {
            float horizontalDirection = (transform.position.x - source.position.x) > 0 ? 1 : -1;

            Vector2 knockDir = new Vector2(horizontalDirection * knockForce, knockUpForce);

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockDir, ForceMode2D.Impulse);
        }

        SetHealth(currentHealth - amount);

        if (currentHealth <= 0)
        {
            GetComponent<PlayerRespawn>().Respawn();
            SetHealth(maxHealth);
        }
    }

    public void Heal(int amount)
    {
        SetHealth(currentHealth + amount);
    }

    public void SetHealth(int newHealth)
    {
        int clamped = Mathf.Clamp(newHealth, 0, maxHealth);
        if (clamped == currentHealth) return;

        currentHealth = clamped;
        OnHealthPercentChanged?.Invoke((float)currentHealth / maxHealth);

        backgroundChanger = FindAnyObjectByType<BackgroundChanger>();
        healthBar = FindAnyObjectByType<HealthBar>();

        if (backgroundChanger == null)
        {
            Debug.LogError("No se encontró el script BackgroundChanger en la escena.");
        }

        if (backgroundChanger != null)
        {
            backgroundChanger.UpdateBackground(currentHealth);
        }
        
        if (healthBar == null)
        {
            Debug.LogError("No se encontró el script BackgroundChanger en la escena.");
        }
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }

    [ContextMenu("Kill")]
    void Kill() => SetHealth(0);
}
