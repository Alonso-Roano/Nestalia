using System.Collections.Generic;
using UnityEngine;
using System; // Necesario para usar Action

public class PlayerInventory : MonoBehaviour
{
    public List<int> itemIDs = new List<int>();
    public static event Action<int> OnItemAdded;

    public void AddItem(int itemID)
    {
        itemIDs.Add(itemID);
        Debug.Log("Item agregado al inventario: " + itemID);

        OnItemAdded?.Invoke(itemID);
    }
    public void RemoveAt(int itemID)
    {
        itemIDs.RemoveAt(itemID);
        Debug.Log("Item agregado al inventario: " + itemID);
    }
}