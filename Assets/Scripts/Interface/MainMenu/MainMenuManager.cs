using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO; 

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Arrastra aquí el botón 'Cargar Partida' desde la jerarquía para deshabilitarlo si no hay partida guardada.")]
    public Button loadGameButton;

    private void Start()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        if (loadGameButton != null)
        {
            loadGameButton.interactable = File.Exists(savePath);
        }
    }

    public void StartNewGame()
    {
        Debug.Log("Iniciando nueva partida...");

        DataManager.Instance.ResetGame();

        GameData newGameData = new GameData();

        DataManager.Instance.SaveGame(newGameData);

        SceneManager.LoadScene(newGameData.lastScene);
    }

    public void LoadSavedGame()
    {
        Debug.Log("Cargando partida guardada...");

        GameData loadedData = DataManager.Instance.LoadGame();

        if (loadedData != null)
        {
            SceneManager.LoadScene(loadedData.lastScene);
        }
        else
        {
            Debug.LogError("No se pudo cargar la partida porque no se encontró el archivo de guardado.");
        }
    }

    public void ExitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}