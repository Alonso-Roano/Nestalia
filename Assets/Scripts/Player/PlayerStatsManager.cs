using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;

    // Referencias (asigna en Awake o Inspector)
    private PlayerController playerController; // Asume que existe

    // Stats locales (cache para no cargar JSON todo el tiempo)
    private float playTime;
    private int enemiesDefeated;
    public int hitsTaken;
    public int deaths;
    private int collectedItems;
    public List<int> fruits = new List<int>();
    public List<int> currentInventoryItems = new List<int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Encuentra player si no asignado
        playerController = FindAnyObjectByType<PlayerController>();
        Debug.Log("PlayerStatsManager Awake complete.");

        // Carga stats iniciales
        LoadStats();
    }

    void Update()
    {
        // Actualiza tiempo jugado (solo si juego no pausado)
        if (Time.timeScale > 0)
        {
            playTime += Time.deltaTime;
        }

        // Guarda periódicamente (cada 30s para no spamear I/O)
        if (Time.frameCount % 1800 == 0) // ~30s a 60fps
        {
            SaveStats();
        }
        // === TECLA L CON NUEVO INPUT SYSTEM ===
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            LogAllStats();
        }
    }

    // Métodos para actualizar stats (llámalos desde otros scripts)
    public void IncrementHitsTaken()
    {
        hitsTaken++;
        SaveStats(); // Guarda inmediatamente para persistencia
        Debug.Log($"Hits taken actualizado: {hitsTaken}");
    }

    public void IncrementDeaths()
    {
        deaths++;
        SaveStats();
        Debug.Log($"Deaths actualizado: {deaths}");
    }

    public void IncrementEnemiesDefeated()
    {
        enemiesDefeated++;
        SaveStats();
        Debug.Log($"Enemies defeated actualizado: {enemiesDefeated}");
    }

    public void IncrementCollectedItems()
    {
        collectedItems++;
        SaveStats();
        Debug.Log($"Collected items actualizado: {collectedItems}");
    }
    public void AddFruit(int fruitID)
    {
        if (!fruits.Contains(fruitID))
        {
            fruits.Add(fruitID);
            collectedItems++;
            SaveStats();
            Debug.Log($"Fruta {fruitID} añadida. Total frutas: {fruits.Count}");
        }
    }

    public bool addInventoryItem(int itemID)
    {
        if (currentInventoryItems.Contains(itemID))
        {
            Debug.LogWarning($"Item {itemID} ya está en el inventario actual.");
            return false;
        }

        currentInventoryItems.Add(itemID);
        
        SaveStats();
        Debug.Log($"Item {itemID} añadido al inventario actual. Total items: {currentInventoryItems.Count}");
        return true;
    }
    public bool removeInventoryItem(int itemID)
    {
        currentInventoryItems.Remove(itemID);
        SaveStats();
        Debug.Log($"Item {itemID} removido del inventario actual. Total items: {currentInventoryItems.Count}");
        return true;
    }

    private void LoadStats()
    {
        GameData data = DataManager.Instance.LoadGame() ?? new GameData();
        playTime = data.playTime;
        enemiesDefeated = data.enemiesDefeated;
        hitsTaken = data.hitsTaken;
        deaths = data.deaths;
        collectedItems = data.collectedItems;
        currentInventoryItems = data.currentInventoryItems;

        // --- MODIFICADO ---
        // Carga la lista de items únicos (para que WorldItem sepa desaparecer)
        fruits = new List<int>(data.collectedUniqueItems);

        // Carga el inventario actual (para el carrusel)
        if (playerController != null && playerController.Inventory != null)
        {
            playerController.Inventory.itemIDs = new List<int>(data.currentInventoryItems);
        }

        Debug.Log("Stats cargadas. Items únicos: " + fruits.Count + ", Inventario actual: " + playerController.Inventory.itemIDs.Count);
    }

    // Guarda mergeando con data existente
    public void SaveStats()
    {
        GameData data = DataManager.Instance.LoadGame() ?? new GameData();
        data.playTime = playTime;
        data.enemiesDefeated = enemiesDefeated;
        data.hitsTaken = hitsTaken;
        data.deaths = deaths;
        data.collectedItems = collectedItems;

        // --- MODIFICADO ---
        // Guarda la lista de items únicos
        data.collectedUniqueItems = fruits;
        data.currentInventoryItems = playerController.Inventory.itemIDs;

        // Guarda el inventario actual
        if (playerController != null && playerController.Inventory != null)
        {
            data.currentInventoryItems = playerController.Inventory.itemIDs;
        }

        DataManager.Instance.SaveGame(data);
        Debug.Log("Stats guardadas.");
    }
    public void ResetStats()
    {
        playTime = 0f;
        enemiesDefeated = 0;
        hitsTaken = 0;
        deaths = 0;
        collectedItems = 0;
        fruits.Clear(); // <-- ¡Muy importante! Limpia la lista en memoria.

        // También reseteamos el inventario del jugador
        if (playerController != null && playerController.Inventory != null)
        {
            playerController.Inventory.itemIDs.Clear();
        }

        Debug.Log("PlayerStatsManager: Stats en memoria reseteadas.");
    }

    // Para debug: Loggea todas las stats (llámalo con una tecla o botón)
    public void LogAllStats()
    {
        Debug.Log($"=== Player Stats ===");
        Debug.Log($"Play Time: {playTime} seconds");
        Debug.Log($"Enemies Defeated: {enemiesDefeated}");
        Debug.Log($"Hits Taken: {hitsTaken}");
        Debug.Log($"Deaths: {deaths}");
        Debug.Log($"Fruits: {fruits} - IDs: {string.Join(", ", fruits)}");
        Debug.Log($"Collected Items: {collectedItems}");
        Debug.Log($"Current Inventory Items: {currentInventoryItems} - IDs: {string.Join(", ", currentInventoryItems)}");
    }
}