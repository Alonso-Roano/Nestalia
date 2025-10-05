using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    public enum TriggerType { Manual, Automatic }

    [Header("Dialogue Settings")]
    [SerializeField] private DialogeRound dialogue;
    [SerializeField] private TriggerType triggerType = TriggerType.Manual;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnlyOnce = false;
    
    [Header("Manual Interaction")]
    [SerializeField] private InputActionReference interactActionReference;
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
        if (triggerType == TriggerType.Manual && playerInArea != null && interactAction.triggered)
        {
            TriggerDialogue();
        }
    }
    
    public void TriggerDialogue()
    {
        if ((triggerOnlyOnce && dialogueWasTriggeredThisSession) || DialogManager.isDialogueActive) return;

        if (triggerOnlyOnce)
        {
            dialogueWasTriggeredThisSession = true;
        }
        
        DialogManager.instance.startDialogue(dialogue);
        
        if (interactionPrompt != null && interactionPrompt.activeSelf)
        {
            if (bounceCoroutine != null)
            {
                StopCoroutine(bounceCoroutine);
            }
            StartCoroutine(FadeOutPrompt(promptCanvasGroup, fadeDuration));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (triggerOnlyOnce && dialogueWasTriggeredThisSession) return;
            
            playerInArea = collision.gameObject;

            if (triggerType == TriggerType.Automatic)
            {
                TriggerDialogue();
            }
            else if (triggerType == TriggerType.Manual && interactionPrompt != null)
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
            
            if (interactionPrompt != null && interactionPrompt.activeSelf)
            {
                if (bounceCoroutine != null)
                {
                    StopCoroutine(bounceCoroutine);
                }
                StartCoroutine(FadeOutPrompt(promptCanvasGroup, fadeDuration));
            }
        }
    }

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

    private IEnumerator FadeOutPrompt(CanvasGroup canvasGroup, float duration)
    {
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / duration;
            yield return null;
        }
        canvasGroup.alpha = 0f;
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