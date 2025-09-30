using UnityEngine;
using UnityEngine.UI;

public class BackgroundChanger : MonoBehaviour
{
    public Sprite[] backgroundImages = new Sprite[10];
    [Tooltip("Arrastra aquí el componente Image que se usará como fondo.")]
    public Image backgroundImageComponent;
    [Tooltip("La vida máxima que el jugador puede tener (ej: 100).")]
    public float maxHealth = 100f; 

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