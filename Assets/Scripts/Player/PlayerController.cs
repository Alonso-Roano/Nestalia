using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerHealth Health { get; private set; }
    public PlayerState State { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerRespawn RespawnController { get; private set; }
    public PlayerInventory Inventory { get; private set; }

    void Awake()
    {
        Health = GetComponent<PlayerHealth>();
        State = GetComponent<PlayerState>();
        Movement = GetComponent<PlayerMovement>();
        RespawnController = GetComponent<PlayerRespawn>();
        Inventory = GetComponent<PlayerInventory>();
    }

    void Start()
    {
        State.LoadPlayerData();
    }

    public void ReachedCheckpoint()
    {
        Debug.Log("Checkpoint alcanzado. Guardando estado y recuperando vida.");
        RespawnController.SetCheckpoint(transform.position);
        State.SavePlayerData();
        Health.SetHealth(Health.maxHealth);
    }
}