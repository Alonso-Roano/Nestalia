using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New dialog", menuName = "Scriptable Objects / Dialogue round")]
public class DialogeRound : ScriptableObject
{
    [SerializeField] private List<DialogeTurn> dialogeTurnList;

    public List<DialogeTurn> DialogeTurnList => dialogeTurnList;
}
