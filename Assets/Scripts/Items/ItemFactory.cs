using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // << AÑADIDO: Necesario para usar FirstOrDefault()

// ====================================================================================
// 1. Estructuras para manejar la data del JSON (deben ser serializables)
// ====================================================================================

// Representa un único ítem con toda su data.
[Serializable]
public class ItemBlueprint
{
    public int id;
    public string itemName;

    // --- MODIFICADO: Ahora especificamos la hoja de sprites y el nombre del sprite individual ---
    public string spriteSheetPath;  // Ruta en Resources a la HOJA DE SPRITES (ej: "Items/SpriteSheet")
    public string spriteName;       // Nombre del sprite DENTRO de la hoja (ej: "SpriteSheet_0")

    public int healthToRestore;
    public string abilityGranted;

    public bool isAbilityBooster = false;
    public float boostMultiplier = 1.5f;
    public float boostDuration = 10f;

    public string change = "jump";
}

// Contenedor para la lista completa de ítems que será el objeto raíz del JSON.
[Serializable]
public class ItemDataContainer
{
    public List<ItemBlueprint> items;
}

// ====================================================================================
// 2. Componente de Propiedades del Ítem (Este SÍ debe ser un MonoBehaviour)
// ====================================================================================

public class ItemProperties : MonoBehaviour
{
    [HideInInspector] public string itemName;
    [HideInInspector] public int healthRestore;
    [HideInInspector] public string ability;
}


// ====================================================================================
// 3. La Fábrica/Creador de Objetos (Clase Estática Pura)
// ====================================================================================

public static class ItemFactory
{
    private static Dictionary<int, ItemBlueprint> itemDatabase;

    public static void Initialize(string jsonFileName)
    {
        TextAsset jsonText = Resources.Load<TextAsset>(jsonFileName);

        if (jsonText == null)
        {
            Debug.LogError($"ItemFactory ERROR: JSON '{jsonFileName}' no encontrado en Resources.");
            return;
        }

        ItemDataContainer container = JsonUtility.FromJson<ItemDataContainer>(jsonText.text);

        itemDatabase = new Dictionary<int, ItemBlueprint>();
        foreach (var blueprint in container.items)
        {
            itemDatabase.Add(blueprint.id, blueprint);
        }

        Debug.Log($"ItemFactory inicializado. {itemDatabase.Count} ítems cargados.");
    }

    public static ItemBlueprint GetBlueprint(int itemID)
    {
        if (itemDatabase != null && itemDatabase.TryGetValue(itemID, out ItemBlueprint blueprint))
        {
            return blueprint;
        }
        return null; // Devuelve null si no se encuentra el ID.
    }

    public static Sprite GetSpriteForItem(int itemID)
    {
        ItemBlueprint blueprint = GetBlueprint(itemID);
        if (blueprint != null)
        {
            Sprite[] allSprites = Resources.LoadAll<Sprite>(blueprint.spriteSheetPath);
            return allSprites.FirstOrDefault(s => s.name == blueprint.spriteName);
        }
        return null; // Devuelve null si no se encuentra.
    }
    public static GameObject CreateItem(int itemId)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemFactory NO ha sido inicializado. Llama a Initialize() primero.");
            return null;
        }

        if (!itemDatabase.TryGetValue(itemId, out ItemBlueprint blueprint))
        {
            Debug.LogError($"ItemFactory ERROR: No se encontró un ítem con ID: {itemId}.");
            return null;
        }

        GameObject itemGO = new GameObject(blueprint.itemName);
        SpriteRenderer sr = itemGO.AddComponent<SpriteRenderer>();

        // --- LÓGICA DE CARGA DE SPRITE MODIFICADA ---

        // 1. Cargar TODOS los sprites de la hoja de sprites especificada en el JSON.
        Sprite[] allSpritesFromSheet = Resources.LoadAll<Sprite>(blueprint.spriteSheetPath);

        if (allSpritesFromSheet.Length == 0)
        {
            Debug.LogWarning($"ItemFactory ADVERTENCIA: No se encontró o no se pudo cargar la hoja de sprites en la ruta: {blueprint.spriteSheetPath}");
        }
        else
        {
            // 2. Encontrar el sprite específico por su nombre dentro del array cargado.
            Sprite itemSprite = allSpritesFromSheet.FirstOrDefault(s => s.name == blueprint.spriteName);

            if (itemSprite != null)
            {
                sr.sprite = itemSprite;
            }
            else
            {
                Debug.LogWarning($"ItemFactory ADVERTENCIA: Se encontró la hoja '{blueprint.spriteSheetPath}', pero no un sprite con el nombre '{blueprint.spriteName}' para el ítem ID {itemId}.");
            }
        }

        // --- FIN DE LA MODIFICACIÓN ---

        ItemProperties properties = itemGO.AddComponent<ItemProperties>();
        properties.itemName = blueprint.itemName;
        properties.healthRestore = blueprint.healthToRestore;
        properties.ability = blueprint.abilityGranted;

        return itemGO;
    }
}