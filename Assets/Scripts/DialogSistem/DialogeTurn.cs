using UnityEngine;

[System.Serializable]
public class DialogeTurn
{
    [field: SerializeField]
    public DialogeCharacter Character { get; private set; }
    [SerializeField, TextArea(2, 4)]
    private string dialogueLine = string.Empty;

    public string DialogueLine => dialogueLine;

}
