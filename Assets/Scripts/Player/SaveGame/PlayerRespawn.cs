using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPoint;

    void Start()
    {
        GameData loadedData = DataManager.Instance.LoadGame();
        respawnPoint = loadedData.checkpointPos;
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
