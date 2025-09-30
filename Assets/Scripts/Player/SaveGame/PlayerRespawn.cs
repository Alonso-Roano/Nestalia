using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPoint;

    void Start()
    {
        respawnPoint = transform.position;
    }

    public void SetCheckpoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
        Debug.Log("Nuevo checkpoint en: " + respawnPoint);
    }

    public void Respawn()
    {
        transform.position = respawnPoint;
    }
}
