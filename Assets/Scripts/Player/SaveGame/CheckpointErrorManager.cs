using UnityEngine;

public class CheckpointErrorManager : MonoBehaviour
{
    public static CheckpointErrorManager Instance;
    
    // La posición especial que se usa SOLO para el teletransporte por daño
    private Vector3 errorRespawnPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opcional: Asegura que persista si cambias de escena
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Inicializa con la posición de inicio del jugador
        errorRespawnPosition = Vector3.zero; 
    }

    // Método que llaman los triggers para guardar la nueva posición de "error"
    public void SetErrorCheckpoint(Vector3 newPosition)
    {
        errorRespawnPosition = newPosition;
        Debug.Log("Nuevo Checkpoint Error Guardado: " + newPosition);
    }

    // Método que llama el jugador cuando recibe daño para obtener la posición
    public Vector3 GetErrorRespawnPosition()
    {
        return errorRespawnPosition;
    }
}