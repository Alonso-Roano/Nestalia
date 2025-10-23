using UnityEngine;
using UnityEngine.UI;

public class AbilityUIController : MonoBehaviour
{
    public Image DoubleJumpImage;
    public Image GlideImage;
    public Image ClimbImage;

    private readonly Color activeColor = Color.white;
    private readonly Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);

    private bool doubleJumpVisibleState = false;
    private bool glideVisibleState = false;
    private bool climbVisibleState = false;

    private bool doubleJumpColorState = false;
    private bool glideColorState = false;
    private bool climbColorState = false;

    void Awake()
    {
        GameData loadedData = DataManager.Instance.LoadGame();
        if (loadedData != null)
        {
            if (loadedData.hasDoubleJump) SetDoubleJumpVisible(false);
            if (loadedData.hasSlowFall) SetGlideVisible(false);
            if (loadedData.hasWallClimb) SetClimbVisible(false);
        }
        else
        {
            Debug.LogError("No se pudo cargar la partida porque no se encontró el archivo de guardado.");
        }
    }

    public void SetDoubleJumpVisible(bool isVisible)
    {
        if (isVisible != doubleJumpVisibleState)
        {
            DoubleJumpImage.gameObject.SetActive(isVisible);
            doubleJumpVisibleState = isVisible;
        }
    }

    public void SetDoubleJumpColor(bool isActive)
    {
        if (isActive != doubleJumpColorState)
        {
            DoubleJumpImage.color = isActive ? activeColor : disabledColor;
            doubleJumpColorState = isActive;
        }
    }
    public void SetGlideVisible(bool isVisible)
    {
        if (isVisible != glideVisibleState)
        {
            GlideImage.gameObject.SetActive(isVisible);
            glideVisibleState = isVisible;
        }
    }

    public void SetGlideColor(bool isActive)
    {
        if (isActive != glideColorState)
        {
            GlideImage.color = isActive ? activeColor : disabledColor;
            glideColorState = isActive;
        }
    }

    public void SetClimbVisible(bool isVisible)
    {
        if (isVisible != climbVisibleState)
        {
            ClimbImage.gameObject.SetActive(isVisible);
            climbVisibleState = isVisible;
        }
    }

    public void SetClimbColor(bool isActive)
    {
        if (isActive != climbColorState)
        {
            ClimbImage.color = isActive ? activeColor : disabledColor;
            climbColorState = isActive;
        }
    }
}