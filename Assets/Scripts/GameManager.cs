using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance;

    [Header("Configuración de Inicialización")]
    [SerializeField]
    private string itemDatabaseFile = "Fruits";

    void Awake()
    {
        Debug.Log("Seve el game manager");
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeSystems();
    }

    private void InitializeSystems()
    {
        Debug.Log("GameManager: Inicializando ItemFactory...");
        ItemFactory.Initialize(itemDatabaseFile);
    }
}