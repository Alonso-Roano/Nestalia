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

    private List<GameObject> items = new List<GameObject>();
    private List<Image> itemMasks = new List<Image>();
    private int currentIndex = 0;
    private float itemSize;
    private bool inputDelay = false;
    private Coroutine showIndexCoroutine;
    private bool isBouncing = false;

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
        if (layout != null && items.Count > 0)
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
        if (isBouncing) return;

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
        if (index < 0 || index >= controller.Inventory.itemIDs.Count) return;

        int selectedItemID = controller.Inventory.itemIDs[index];
        string selectedItemName = ItemFactory.GetBlueprint(selectedItemID)?.itemName ?? "Desconocido";

        Debug.Log($"Ítem seleccionado: {selectedItemName} (ID: {selectedItemID})");
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
        if (indexDisplayContainer == null || indexDisplayText == null) return;

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
}