using UnityEngine;
using UnityEngine.UI;

public class BackgroundChanger : MonoBehaviour
{
    public Sprite[] backgroundImages = new Sprite[10];
    public Image backgroundImageComponent;
    public float maxHealth = 100f;

    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto del Jugador que tiene el script PlayerHealth.")]
    public PlayerHealth playerHealth;

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthPercentChanged += HandleHealthChanged;
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthPercentChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(float healthPercent)
    {
        float currentHealthValue = healthPercent * maxHealth;
        UpdateBackground(currentHealthValue);
    }

    public void UpdateBackground(float currentHealth)
    {
        if (backgroundImages.Length != 10)
        {
            Debug.LogError("El arreglo 'backgroundImages' debe tener exactamente 10 Sprites asignados.");
            return;
        }

        if (backgroundImageComponent == null)
        {
            Debug.LogError("La referencia a 'backgroundImageComponent' no está asignada.");
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        float segmentSize = maxHealth / 10f;
        int imageIndex = Mathf.FloorToInt(currentHealth / segmentSize);

        if (imageIndex >= 10)
        {
            imageIndex = 9;
        }
        else if (imageIndex != 0)
        {
            imageIndex--;
        }
        
        backgroundImageComponent.sprite = backgroundImages[imageIndex];
        
        Debug.Log($"Vida: {currentHealth} (Max: {maxHealth}). Índice de imagen seleccionado: {imageIndex}");
    }
}