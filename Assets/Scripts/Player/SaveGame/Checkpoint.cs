using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Se entro en el endpoint " + other);
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.State.SavePlayerData();
                controller.RespawnController.SetCheckpoint(transform.position);
                controller.Health.SetHealth(controller.Health.maxHealth);
            }
        }
    }
    
    public GameData SaveCheckpoint(int newLevel, int newSubLevel, Vector3 newPosition, int sceneIndex)
    {
        GameData data = DataManager.Instance.LoadGame();

        if (data == null)
        {
            data = new GameData();
        }

        data.currentLevel = newLevel;
        data.currentSubLevel = newSubLevel;
        data.checkpointPos = newPosition;
        data.lastScene = sceneIndex;

        DataManager.Instance.SaveGame(data);

        return data;
    }
}
