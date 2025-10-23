using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Arrastra aquí el objeto del panel del menú")]
    public GameObject menuPanel;

    void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (menuPanel == null) return;

        // Invertimos el estado actual del menú (si está activo lo desactiva, y viceversa).
        bool isActive = !menuPanel.activeSelf;
        
        // Aplicamos el nuevo estado al panel.
        menuPanel.SetActive(isActive);
        
        // Usamos un operador ternario para pausar o reanudar el juego.
        // Si el menú está activo (isActive es true), Time.timeScale = 0. Si no, Time.timeScale = 1.
        Time.timeScale = isActive ? 0f : 1f;
    }

    public void ResumeGame()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    public void LoadScene(int sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void ShowObject(GameObject objectToShow)
    {
        if (objectToShow != null)
        {
            objectToShow.SetActive(true);
        }
    }

    public void HideObject(GameObject objectToHide)
    {
        if (objectToHide != null)
        {
            objectToHide.SetActive(false);
        }
    }
}