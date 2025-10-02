using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    [Header("Configuracion")]
    public GameObject menuPanel;

    private bool menuOpen = false;

    void Awake()
    {
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            menuOpen = !menuOpen;
            menuPanel.SetActive(menuOpen);
            Time.timeScale = menuOpen ? 0f : 1f;
        }
    }
}