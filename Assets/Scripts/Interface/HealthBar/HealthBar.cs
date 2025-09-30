using UnityEngine;
using UnityEngine.UI; // Necesario para acceder al componente Image

public class HealthBar : MonoBehaviour
{
    private Image healthBarImage;

    [Tooltip("La vida máxima del personaje, debe coincidir con la de PlayerController.")]
    public float maxHealth = 100f; 

    void Awake()
    {
        healthBarImage = GetComponent<Image>();

        if (healthBarImage != null && healthBarImage.type != Image.Type.Filled)
        {
            Debug.LogError("El componente Image de la barra de vida DEBE estar configurado como 'Filled' (Relleno) para que este script funcione correctamente.");
        }
        
        SetHealth(maxHealth);
    }

    public void SetHealth(float currentHealth)
    {
        if (healthBarImage == null) return;

        float fillRatio = currentHealth / maxHealth;

        healthBarImage.fillAmount = fillRatio;
    }
}