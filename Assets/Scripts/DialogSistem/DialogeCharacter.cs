using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Character", menuName = "Scriptable Objects/Dialogue Character")]
public class DialogeCharacter : ScriptableObject
{
    [Header("Informacion del personaje")]
    [SerializeField] private string characterName;
    [SerializeField] private Sprite profilePhoto;

    public string Name => characterName;
    public Sprite ProfilePhoto => profilePhoto;
}
