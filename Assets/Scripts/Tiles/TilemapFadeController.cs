using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections; // Necesario para Coroutines

public class TilemapFadeController : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField]
    private float fadeDuration = 0.5f; // Duración de la animación en segundos
    [SerializeField]
    private string playerTag = "Player"; // Etiqueta (Tag) de tu jugador

    private Coroutine currentFadeRoutine;

    void Start()
    {
        // Obtiene el componente Tilemap
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("El componente Tilemap no se encontró en este GameObject.");
            enabled = false;
        }
    }

    // Se llama cuando un Collider (con Rigidbody) entra en el Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // El jugador ha entrado: iniciar Fade Out (desaparecer)
            if (currentFadeRoutine != null)
                StopCoroutine(currentFadeRoutine);

            currentFadeRoutine = StartCoroutine(FadeTilemap(1f, 0f, fadeDuration));
        }
    }

    // Se llama cuando un Collider (con Rigidbody) sale del Trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // El jugador ha salido: iniciar Fade In (aparecer)
            if (currentFadeRoutine != null)
                StopCoroutine(currentFadeRoutine);

            currentFadeRoutine = StartCoroutine(FadeTilemap(0f, 1f, fadeDuration));
        }
    }

    // Coroutine para la animación de Fade
    IEnumerator FadeTilemap(float startAlpha, float endAlpha, float duration)
    {
        float startTime = Time.time;
        Color currentColor = tilemap.color;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            // Interpolar (Lerp) el alpha entre el valor inicial y el final
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
            tilemap.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        // Asegurarse de que el color final sea exactamente el que se quiere
        tilemap.color = new Color(currentColor.r, currentColor.g, currentColor.b, endAlpha);
        currentFadeRoutine = null;
    }
}