using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image healthBarImage;

    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto del Jugador que tiene el script PlayerHealth.")]
    public PlayerHealth playerHealth;

    [Header("Sprites por Rango de Salud (0-100%)")]
    public Sprite spriteLowHealth;
    public Sprite spriteMidLowHealth;
    public Sprite spriteMidHighHealth;
    public Sprite spriteFullHealth;

    [Header("Colores de Apoyo (opcional, para colorear el sprite)")]
    public Color colorLow = Color.red;
    public Color colorMidLow = Color.yellow;
    public Color colorMidHigh = Color.green;
    public Color colorFull = Color.blue;

    private int currentHealthTier = -1;

    void Awake()
    {
        healthBarImage = GetComponent<Image>();

        if (healthBarImage == null)
        {
            Debug.LogError("El GameObject de la barra de vida NO tiene un componente Image.");
            return;
        }

        if (healthBarImage.type != Image.Type.Filled || healthBarImage.fillMethod != Image.FillMethod.Vertical)
        {
            healthBarImage.type = Image.Type.Filled;
            healthBarImage.fillMethod = Image.FillMethod.Vertical;
            Debug.LogWarning("Se ha ajustado el componente Image a 'Filled' con 'Vertical' Fill Method. Verifique el 'Fill Origin'.");
        }
    }
    
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

    private void HandleHealthChanged(float fillRatio)
    {
        if (healthBarImage == null) return;

        healthBarImage.fillAmount = fillRatio;

        int newHealthTier = GetHealthTier(fillRatio);

        if (newHealthTier != currentHealthTier)
        {
            currentHealthTier = newHealthTier;
            ApplyHealthTierVisuals(currentHealthTier);
        }
    }

    private int GetHealthTier(float ratio)
    {
        if (ratio <= 0.25f) return 0;
        else if (ratio <= 0.50f) return 1;
        else if (ratio <= 0.75f) return 2;
        else return 3;
    }

    private void ApplyHealthTierVisuals(int tier)
    {
        Sprite targetSprite = null;
        Color targetColor = Color.white;

        switch (tier)
        {
            case 0:
                targetSprite = spriteLowHealth;
                targetColor = colorLow;
                break;
            case 1:
                targetSprite = spriteMidLowHealth;
                targetColor = colorMidLow;
                break;
            case 2:
                targetSprite = spriteMidHighHealth;
                targetColor = colorMidHigh;
                break;
            case 3:
                targetSprite = spriteFullHealth;
                targetColor = colorFull;
                break;
        }

        if (targetSprite != null)
        {
            healthBarImage.sprite = targetSprite;
            healthBarImage.color = targetColor;
        }
        else
        {
            Debug.LogWarning($"El sprite para el rango {tier} no está asignado.");
        }
    }
}