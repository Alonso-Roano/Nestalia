using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerInventory : MonoBehaviour
{
    public List<int> itemIDs = new List<int>();
    public static event Action<int> OnItemAdded;

    public void Awake()
    {
        itemIDs = PlayerStatsManager.Instance.currentInventoryItems;
        
        Debug.Log("Inventario cargado en PlayerInventory: " + string.Join(", ", itemIDs));
    }

    public void AddItem(int itemID)
    {
        itemIDs.Add(itemID);
        Debug.Log("Item agregado al inventario: " + itemID);
        PlayerStatsManager.Instance.addInventoryItem(itemID);
        OnItemAdded?.Invoke(itemID);
    }
    public void RemoveAt(int itemID)
    {
        itemIDs.RemoveAt(itemID);
        PlayerStatsManager.Instance.removeInventoryItem(itemID);
        Debug.Log("Item agregado al inventario: " + itemID);
    }
}