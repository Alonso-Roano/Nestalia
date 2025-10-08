using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class MenuCarrusel : MonoBehaviour
{
    [Header("Referencias de UI")]
    public RectTransform contentRect;
    public GameObject itemPrefab;

    [Header("Datos del Menu")]
    public PlayerController controller;

    [Header("Configuración del Carrusel")]
    public float scaleFactor = 1.3f;
    public float scrollSpeed = 0.2f;
    public float fadeDistance = 1.5f;

    [Header("Configuración de Rebote")]
    public float bounceDistance = 50f;
    public float bounceDuration = 0.2f;

    [Header("UI Adicional")]
    public Color selectedColor = new Color(1f, 1f, 1f, 0.3f);
    public GameObject arrowLeft;
    public GameObject arrowRight;
    public GameObject indexDisplayContainer;
    public TextMeshProUGUI indexDisplayText;
    public float indexDisplayTime = 1.5f;
    public CanvasGroup indexDisplayCanvasGroup;
    public float indexFadeOutDuration = 0.5f;

    [Header("Animator de Bubo")]
    [SerializeField] public Animator animator;

    private List<GameObject> items = new List<GameObject>();
    private List<Image> itemMasks = new List<Image>();

    public Sprite[] backgroundImages = new Sprite[24];
    public GameObject backgroundImageComponent;
    private int currentIndex = 0;
    private float itemSize;
    private bool inputDelay = false;
    private Coroutine showIndexCoroutine;
    private bool isBouncing = false;
    private Coroutine showBackgroundCoroutine;
    public float backgroundDisplayTime = 2f;

    void OnEnable()
    {
        PlayerInventory.OnItemAdded += AddItemToMenu;
    }

    void OnDisable()
    {
        PlayerInventory.OnItemAdded -= AddItemToMenu;
    }

    void Start()
    {
        if (indexDisplayContainer != null)
        {
            indexDisplayContainer.SetActive(false);
        }
        if (backgroundImageComponent != null)
        {
            backgroundImageComponent.gameObject.SetActive(false);
        }
        PopulateMenuFromInventory();
    }

    void Update()
    {
        UpdateScrollPosition();
        UpdateItemVisuals();
    }

    void PopulateMenuFromInventory()
    {
        foreach (var item in items)
        {
            Destroy(item);
        }
        items.Clear();
        itemMasks.Clear();

        if (controller.Inventory == null)
        {
            Debug.LogError("PlayerInventory no está asignado en el MenuCarrusel.");
            return;
        }

        foreach (int itemID in controller.Inventory.itemIDs)
        {
            CreateMenuItem(itemID);
        }

        UpdateLayoutAndVisuals();
    }

    private void AddItemToMenu(int itemID)
    {
        CreateMenuItem(itemID);

        currentIndex = items.Count - 1;

        if (showBackgroundCoroutine != null)
        {
            StopCoroutine(showBackgroundCoroutine);
        }
        showBackgroundCoroutine = StartCoroutine(ShowBackgroundImageTemporarily(itemID));

        ShowIndexIndicator();

        UpdateLayoutAndVisuals();
    }

    private void CreateMenuItem(int itemID)
    {
        if (itemPrefab == null) return;

        GameObject newItem = Instantiate(itemPrefab, contentRect);

        ItemBlueprint blueprint = ItemFactory.GetBlueprint(itemID);
        if (blueprint != null)
        {
            newItem.name = blueprint.itemName;

            Image iconImage = newItem.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = ItemFactory.GetSpriteForItem(itemID);
            }
            else
            {
                Debug.LogWarning("El prefab del ítem no tiene un hijo llamado 'Icon' con un componente Image.");
            }
        }

        newItem.transform.localScale = Vector3.one;
        items.Add(newItem);

        Image mask = newItem.transform.Find("Mask")?.GetComponent<Image>();
        if (mask != null)
        {
            itemMasks.Add(mask);
        }
        else
        {
            itemMasks.Add(null);
        }
    }

    private void UpdateLayoutAndVisuals()
    {
        Canvas.ForceUpdateCanvases();

        HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
        if (layout != null && items.Count > 0 && items[0] != null)
        {
            itemSize = items[0].GetComponent<RectTransform>().rect.width + layout.spacing;
        }

        UpdateScrollPosition();
        UpdateItemVisuals();
        UpdateArrowVisibility();
    }

    public void OnNavigateRight(InputAction.CallbackContext context)
    {
        if (context.performed && !inputDelay)
        {
            Navigate(1);
        }
    }

    public void OnNavigateLeft(InputAction.CallbackContext context)
    {
        if (context.performed && !inputDelay)
        {
            Navigate(-1);
        }
    }

    private void Navigate(int direction)
    {
        if (isBouncing || items.Count == 0) return;

        int newIndex = currentIndex + direction;

        if (newIndex >= 0 && newIndex < items.Count)
        {
            currentIndex = newIndex;
            StartCoroutine(InputCooldown());
            UpdateArrowVisibility();
            ShowIndexIndicator();
        }
        else
        {
            StartCoroutine(BounceEffectCoroutine(direction));
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SelectItem(currentIndex);
        }
    }

    IEnumerator InputCooldown()
    {
        inputDelay = true;
        yield return new WaitForSeconds(scrollSpeed);
        inputDelay = false;
    }

    void SelectItem(int index)
    {
        if (index < 0 || index >= controller.Inventory.itemIDs.Count)
        {
            Debug.LogWarning("Índice de ítem inválido o inventario vacío.");
            return;
        }

        int selectedItemID = controller.Inventory.itemIDs[index];
        ItemBlueprint blueprint = ItemFactory.GetBlueprint(selectedItemID);
        if (blueprint == null)
        {
            Debug.LogError($"No se encontró el Blueprint para el ID de ítem: {selectedItemID}");
            return;
        }
        string selectedItemName = blueprint.itemName;
        int selectedItemHealth = blueprint.healthToRestore;

        if (selectedItemHealth > 0)
        {
            PlayerHealth playerHealth = controller.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("No se encontró el componente PlayerHealth en el jugador.");
                return;
            }

            if (showBackgroundCoroutine != null)
            {
                StopCoroutine(showBackgroundCoroutine);
            }
            showBackgroundCoroutine = StartCoroutine(ShowBackgroundImageTemporarily(selectedItemID));

            // Si el jugador ya tiene la vida al máximo, no hacer nada.
            if (playerHealth.GetCurrentHealth() >= playerHealth.maxHealth)
            {
                Debug.Log("La vida ya está al máximo. No se puede usar el objeto.");
                // Aquí podrías añadir un sonido o efecto visual para indicar que no se puede usar.
                return;
            }

            if (blueprint.isAbilityBooster)
            {
                // Buscamos nuestro nuevo gestor
                PlayerStatusEffects statusManager = controller.GetComponent<PlayerStatusEffects>();
                if (statusManager != null)
                {
                    statusManager.ApplyEffect(blueprint);
                }
                else
                {
                    Debug.LogError("No se encontró el componente PlayerStatusEffects en el jugador.");
                }
            }

            playerHealth.Heal(selectedItemHealth);
            Debug.Log($"Se usó '{selectedItemName}'. Vida del jugador restaurada.");

            controller.Inventory.RemoveAt(index);

            GameObject itemToDestroy = items[index];
            items.RemoveAt(index);
            itemMasks.RemoveAt(index);
            Destroy(itemToDestroy);

            if (items.Count > 0)
            {
                currentIndex = Mathf.Clamp(currentIndex, 0, items.Count - 1);
            }
            else
            {
                currentIndex = 0;
            }

            UpdateLayoutAndVisuals();

            if (items.Count > 0)
            {
                ShowIndexIndicator();
            }
            else
            {
                if (indexDisplayContainer != null)
                {
                    if (showIndexCoroutine != null) StopCoroutine(showIndexCoroutine);
                    indexDisplayContainer.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log($"Ítem seleccionado: {selectedItemName} (ID: {selectedItemID}) - No es un consumible de vida.");
        }
    }


    void UpdateScrollPosition()
    {
        if (isBouncing) return;

        float targetX = -currentIndex * itemSize;
        Vector2 targetPosition = new Vector2(targetX, contentRect.anchoredPosition.y);

        contentRect.anchoredPosition = Vector2.Lerp(
            contentRect.anchoredPosition,
            targetPosition,
            Time.deltaTime * (1f / scrollSpeed)
        );
    }

    void UpdateItemVisuals()
    {
        for (int i = 0; i < items.Count; i++)
        {
            Transform itemTransform = items[i].transform;
            float distanceToCenter = Mathf.Abs(i - currentIndex);

            float targetScale = 1f;
            if (distanceToCenter < 1f)
            {
                targetScale = Mathf.Lerp(scaleFactor, 1f, distanceToCenter);
            }
            else if (distanceToCenter >= fadeDistance)
            {
                targetScale = 0f;
            }
            else
            {
                float fadeProgress = (distanceToCenter - 1f) / (fadeDistance - 1f);
                targetScale = Mathf.Lerp(1f, 0f, fadeProgress);
            }

            itemTransform.localScale = Vector3.Lerp(
                itemTransform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * 10f
            );

            if (itemMasks[i] != null)
            {
                Color targetColor = (i == currentIndex) ? selectedColor : Color.clear;
                itemMasks[i].color = Color.Lerp(itemMasks[i].color, targetColor, Time.deltaTime * 10f);
            }
        }
    }

    IEnumerator BounceEffectCoroutine(int direction)
    {
        isBouncing = true;
        inputDelay = true;

        Vector2 startPos = contentRect.anchoredPosition;
        Vector2 overshootPos = startPos - new Vector2(direction * bounceDistance, 0);
        float elapsedTime = 0f;
        float halfDuration = bounceDuration / 2f;

        while (elapsedTime < halfDuration)
        {
            contentRect.anchoredPosition = Vector2.Lerp(startPos, overshootPos, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            contentRect.anchoredPosition = Vector2.Lerp(overshootPos, startPos, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        contentRect.anchoredPosition = startPos;

        isBouncing = false;
        inputDelay = false;
    }

    void UpdateArrowVisibility()
    {
        if (arrowLeft != null)
        {
            arrowLeft.SetActive(currentIndex > 0);
        }
        if (arrowRight != null)
        {
            arrowRight.SetActive(currentIndex < items.Count - 1);
        }
    }

    void ShowIndexIndicator()
    {
        if (indexDisplayContainer == null || indexDisplayText == null || items.Count == 0) return;

        if (showIndexCoroutine != null)
        {
            StopCoroutine(showIndexCoroutine);
        }
        showIndexCoroutine = StartCoroutine(IndexIndicatorCoroutine());
    }

    IEnumerator IndexIndicatorCoroutine()
    {
        if (currentIndex < 0 || currentIndex >= controller.Inventory.itemIDs.Count)
        {
            yield break;
        }

        int currentItemID = controller.Inventory.itemIDs[currentIndex];

        ItemBlueprint blueprint = ItemFactory.GetBlueprint(currentItemID);
        string itemName = (blueprint != null) ? blueprint.itemName : "Desconocido";

        indexDisplayContainer.SetActive(true);
        indexDisplayCanvasGroup.alpha = 1f;
        indexDisplayText.text = itemName;

        yield return new WaitForSeconds(indexDisplayTime);

        float elapsedTime = 0f;
        while (elapsedTime < indexFadeOutDuration)
        {
            indexDisplayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / indexFadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        indexDisplayCanvasGroup.alpha = 0f;
        indexDisplayContainer.SetActive(false);
    }
    private IEnumerator ShowBackgroundImageTemporarily(int itemID)
    {
        animator.SetTrigger("Fruit");
        // Primero, revisa que el objeto y los datos sean válidos
        if (backgroundImageComponent == null || itemID < 0 || itemID >= backgroundImages.Length)
        {
            yield break;
        }

        // Busca el componente SpriteRenderer en el objeto
        SpriteRenderer sRenderer = backgroundImageComponent.GetComponent<SpriteRenderer>();
        if (sRenderer == null)
        {
            Debug.LogError("¡Error! El 'backgroundImageComponent' no tiene un componente SpriteRenderer.");
            yield break;
        }

        // Asigna el sprite y activa el GameObject
        sRenderer.sprite = backgroundImages[itemID];
        backgroundImageComponent.SetActive(true);

        // Espera el tiempo definido
        yield return new WaitForSeconds(backgroundDisplayTime);

        // Desactiva el GameObject
        backgroundImageComponent.SetActive(false);
    }
}