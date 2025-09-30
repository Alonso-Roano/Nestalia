using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("Juego guardado en: " + savePath);
    }

    public GameData LoadGame()
    {
        Debug.Log(savePath);
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Juego cargado correctamente.");
            return data;
        }
        else
        {
            Debug.LogError("No se encontró el archivo de guardado.");
            return null;
        }
    }
    public void ResetGame()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Datos del juego eliminados. El juego se ha reiniciado.");
        }
        else
        {
            Debug.LogWarning("No hay datos de juego para eliminar.");
        }
    }
}