using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    [Header("Habilidades Desbloqueadas")]
    public bool hasDoubleJump;
    public bool hasSlowFall;
    public bool hasWallClimb;

    private PlayerController controller;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void SavePlayerData()
    {
        if (DataManager.Instance == null) return;

        GameData data = new GameData
        {
            checkpointPos = transform.position,

            maxHealth = controller.Health.maxHealth,

            hasDoubleJump = this.hasDoubleJump,
            hasSlowFall = this.hasSlowFall,
            hasWallClimb = this.hasWallClimb,

            lastScene = SceneManager.GetActiveScene().buildIndex

        };

        DataManager.Instance.SaveGame(data);
        Debug.Log("Datos del jugador guardados.");
    }

    public void LoadPlayerData()
    {
        if (DataManager.Instance == null) return;
        
        GameData loadedData = DataManager.Instance.LoadGame();
        if (loadedData != null)
        {
            transform.position = loadedData.checkpointPos;

            controller.Health.maxHealth = loadedData.maxHealth;
            controller.Health.SetHealth(loadedData.maxHealth);

            this.hasDoubleJump = loadedData.hasDoubleJump;
            this.hasSlowFall = loadedData.hasSlowFall;
            this.hasWallClimb = loadedData.hasWallClimb;
            
            Debug.Log("Datos del jugador cargados.");
        }
    }

    public void ResetPlayerData()
    {
        if (DataManager.Instance == null) return;
        
        DataManager.Instance.ResetGame();
        
        transform.position = Vector3.zero;
        hasDoubleJump = false;
        hasSlowFall = false;
        hasWallClimb = false;
        
        controller.Health.maxHealth = 100;
        controller.Health.SetHealth(controller.Health.maxHealth);

        Debug.Log("Juego y datos del jugador restablecidos.");
    }
}