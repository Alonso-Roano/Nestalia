using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events; // Necesario para UnityEvent
using System.Collections;

public class SaveDialog : MonoBehaviour
{
    // Se ha eliminado el 'TriggerType' enum, el diálogo siempre es manual y de guardado.

    [Header("UI Canvas (El Diálogo a Mostrar)")]
    [Tooltip("El Canvas o GameObject que contiene el diálogo de confirmación estático.")]
    [SerializeField] private GameObject saveDialogCanvas;


    public UnityEvent onSaveConfirmed;

    public UnityEvent onCancelConfirmed;

    [Header("Behavior")]
    [Tooltip("Si el punto de guardado solo debe ser interactuable una vez por sesión.")]
    [SerializeField] private bool triggerOnlyOnce = false;

    [Header("Manual Interaction")]
    [Tooltip("La acción de Input System que el jugador debe presionar para activar el diálogo.")]
    [SerializeField] private InputActionReference interactActionReference;
    [Tooltip("El prompt visual (ej: 'Presiona E') que se muestra en el área.")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceSpeed = 1f;

    private GameObject playerInArea;
    private bool dialogueWasTriggeredThisSession = false;
    private InputAction interactAction;
    private CanvasGroup promptCanvasGroup;
    private Vector3 initialPromptPosition;
    private Coroutine bounceCoroutine;
    private PlayerController controller;

    private void Awake()
    {
        if (interactActionReference != null)
        {
            interactAction = interactActionReference.action;
        }

        if (interactionPrompt != null)
        {
            promptCanvasGroup = interactionPrompt.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = interactionPrompt.AddComponent<CanvasGroup>();
            }
            promptCanvasGroup.alpha = 0f;
            interactionPrompt.SetActive(false);
            initialPromptPosition = interactionPrompt.transform.position;
        }

        // Aseguramos que el diálogo esté oculto al inicio
        if (saveDialogCanvas != null)
        {
            saveDialogCanvas.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.Disable();
        }
    }

    private void Update()
    {
        // Solo se activa si el jugador está en el área, presiona la tecla y el diálogo NO está activo.
        if (playerInArea != null && interactAction != null && interactAction.triggered && !IsDialogueActive())
        {
            TriggerSaveDialog();
        }
    }

    /// <summary>
    /// Verifica si el Canvas del diálogo de guardado está visible.
    /// </summary>
    private bool IsDialogueActive()
    {
        return saveDialogCanvas != null && saveDialogCanvas.activeSelf;
    }

    /// <summary>
    /// Muestra el diálogo de confirmación de guardado.
    /// </summary>
    public void TriggerSaveDialog()
    {
        if ((triggerOnlyOnce && dialogueWasTriggeredThisSession) || IsDialogueActive()) return;

        if (saveDialogCanvas == null)
        {
            Debug.LogError("Save Dialog Canvas no está asignado. No se puede mostrar el diálogo.");
            return;
        }

        if (triggerOnlyOnce)
        {
            dialogueWasTriggeredThisSession = true;
        }

        Time.timeScale = 0f;

        // 1. Mostrar el Canvas de Diálogo
        saveDialogCanvas.SetActive(true);

        // 2. Ocultar el prompt de interacción si estaba activo
        if (interactionPrompt != null && interactionPrompt.activeSelf)
        {
            if (bounceCoroutine != null)
            {
                StopCoroutine(bounceCoroutine);
            }
            FadeOutPrompt();
        }
    }

    // --- MÉTODOS PÚBLICOS PARA ASIGNAR A LOS BOTONES DEL CANVAS ---

    /// <summary>
    /// Ejecuta la acción de guardado y cierra el diálogo.
    /// ASIGNAR ESTE MÉTODO AL EVENTO OnClick() del botón 'GUARDAR'.
    /// </summary>
    public void OnSaveClicked()
    {

        Debug.Log("Juego Guardado");
        Debug.Log(controller != null);
        if (controller != null)
        {
            controller.State.SavePlayerData();
            controller.RespawnController.SetCheckpoint(transform.position);
            controller.Health.SetHealth(controller.Health.maxHealth);
        }


        // 2. Cerrar el diálogo
        CloseDialog();
    }

    /// <summary>
    /// Cierra el diálogo sin ejecutar la acción de guardado.
    /// ASIGNAR ESTE MÉTODO AL EVENTO OnClick() del botón 'CANCELAR'.
    /// </summary>
    public void OnCancelClicked()
    {
        // 1. Ejecutar la acción opcional de cancelar
        if (onCancelConfirmed != null)
        {
            onCancelConfirmed.Invoke();
        }

        // 2. Cerrar el diálogo
        CloseDialog();
    }

    /// <summary>
    /// Oculta el diálogo de confirmación.
    /// </summary>
    private void CloseDialog()
    {
        Time.timeScale = 1f;
        if (saveDialogCanvas != null)
        {
            saveDialogCanvas.SetActive(false);
        }

        // Si el jugador sigue en el área Y no es un evento de una sola vez, restaurar el prompt
        if (playerInArea != null && interactionPrompt != null && !(triggerOnlyOnce && dialogueWasTriggeredThisSession))
        {
            interactionPrompt.SetActive(true);
            StartCoroutine(FadeInPrompt(promptCanvasGroup, fadeDuration));
            bounceCoroutine = StartCoroutine(BouncePrompt());
        }
    }

    // --- Lógica de Detección de Colisión ---

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
            controller = collision.GetComponent<PlayerController>();

            // Solo muestra el prompt si NO se ha activado ya Y el diálogo NO está activo.
            if (triggerOnlyOnce && dialogueWasTriggeredThisSession) return;
            if (IsDialogueActive()) return;

            playerInArea = collision.gameObject;

            // Mostrar el prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                StartCoroutine(FadeInPrompt(promptCanvasGroup, fadeDuration));
                bounceCoroutine = StartCoroutine(BouncePrompt());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInArea = null;

            // Ocultar el prompt
            if (interactionPrompt != null && interactionPrompt.activeSelf)
            {
                if (bounceCoroutine != null)
                {
                    StopCoroutine(bounceCoroutine);
                }
                // Reseteamos la posición para evitar un glitch visual al salir.
                interactionPrompt.transform.position = initialPromptPosition;
                FadeOutPrompt();
            }
        }
    }

    // --- Corutinas de Fade y Bounce ---

    private IEnumerator FadeInPrompt(CanvasGroup canvasGroup, float duration)
    {
        canvasGroup.alpha = 0f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / duration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void FadeOutPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    private IEnumerator BouncePrompt()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.unscaledDeltaTime * bounceSpeed;
            float yOffset = Mathf.Sin(timer) * bounceHeight;
            interactionPrompt.transform.position = initialPromptPosition + new Vector3(0, yOffset, 0);
            yield return null;
        }
    }
}
