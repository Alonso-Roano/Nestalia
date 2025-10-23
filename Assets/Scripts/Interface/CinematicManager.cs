using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class CinematicManager : MonoBehaviour
{
    [Header("Configuración de la Cinemática")]
    [Tooltip("El componente de Imagen UI donde se mostrarán los sprites.")]
    public Image displayImage;

    [Tooltip("La secuencia de imágenes que se mostrarán en la cinemática.")]
    public Sprite[] cinematicImages;

    [Header("Tiempos y Transiciones")]
    [Tooltip("Duración (en segundos) que cada imagen permanece en pantalla.")]
    public float displayDuration = 3.0f;

    [Tooltip("Duración (en segundos) del efecto de fade in y fade out.")]
    public float fadeDuration = 1.0f;

    [Tooltip("Curva que define la forma del fade (por defecto, Ease In-Out).")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Transición Final")]
    [Tooltip("Imagen negra para el fade final a negro (opcional).")]
    public Image blackOverlay;

    [Header("Escena a Cargar")]
    [Tooltip("El índice o nombre de la escena que se cargará al finalizar la cinemática.")]
    public int sceneToLoad;

    [SerializeField] private InputActionReference nextDialogueActionReference;
    private InputAction nextDialogueAction;

    private bool isSkipping = false;

    // --- NUEVO MÉTODO: Se llama cuando el objeto se activa ---
    private void OnEnable()
    {
        if (nextDialogueAction != null)
        {
            nextDialogueAction.Enable();
        }
    }

    // --- NUEVO MÉTODO: Se llama cuando el objeto se desactiva ---
    private void OnDisable()
    {
        if (nextDialogueAction != null)
        {
            nextDialogueAction.Disable();
        }
    }

    private void Start()
    {
        if (nextDialogueActionReference != null)
        {
            nextDialogueAction = nextDialogueActionReference.action;
            // Habilitamos la acción aquí también por si el objeto ya estaba activo
            // antes de que Start() se ejecutara.
            nextDialogueAction.Enable(); 
        }
        if (displayImage == null)
        {
            Debug.LogError("Error: No se ha asignado un componente 'Image' al CinematicManager.");
            return;
        }

        if (cinematicImages == null || cinematicImages.Length == 0)
        {
            Debug.LogWarning("Advertencia: No hay imágenes en la secuencia. Cargando escena directamente...");
            LoadNextScene();
            return;
        }
        
        if (blackOverlay != null)
        {
            Color c = blackOverlay.color;
            blackOverlay.color = new Color(c.r, c.g, c.b, 0);
        }

        StartCoroutine(PlayCinematic());
    }
    
    private void Update()
    {
        // Esta comprobación ahora debería funcionar correctamente.
        if (nextDialogueAction != null && nextDialogueAction.triggered && !isSkipping)
        {
            isSkipping = true;
            StopAllCoroutines();
            StartCoroutine(SkipAndLoad());
        }
    }

    // ... (El resto de tu código es correcto y no necesita cambios) ...
    
    private IEnumerator PlayCinematic()
    {
        SetImageAlpha(0f);
        foreach (var image in cinematicImages)
        {
            displayImage.sprite = image;
            yield return StartCoroutine(FadeImage(1f, fadeDuration));
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeImage(0f, fadeDuration));
        }
        yield return StartCoroutine(FadeOverlay(1f, fadeDuration));
        LoadNextScene();
    }
    
    private IEnumerator SkipAndLoad()
    {
        yield return StartCoroutine(FadeOverlay(1f, 0.5f));
        LoadNextScene();
    }

    private IEnumerator FadeImage(float targetAlpha, float duration)
    {
        float startAlpha = displayImage.color.a;
        float elapsedTime = 0f;
        Color baseColor = displayImage.color;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float curvedT = fadeCurve.Evaluate(t);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, curvedT);
            displayImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, newAlpha);
            yield return null;
        }
        displayImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, targetAlpha);
    }
    
    private IEnumerator FadeOverlay(float targetAlpha, float duration)
    {
        if (blackOverlay == null) 
        {
            yield break;
        }
        float startAlpha = blackOverlay.color.a;
        float elapsedTime = 0f;
        Color baseColor = blackOverlay.color;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float curvedT = fadeCurve.Evaluate(t);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, curvedT);
            blackOverlay.color = new Color(baseColor.r, baseColor.g, baseColor.b, newAlpha);
            yield return null;
        }
        blackOverlay.color = new Color(baseColor.r, baseColor.g, baseColor.b, targetAlpha);
    }

    private void SetImageAlpha(float alpha)
    {
        if (displayImage == null) return;
        Color c = displayImage.color;
        displayImage.color = new Color(c.r, c.g, c.b, alpha);
    }
    
    private void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}