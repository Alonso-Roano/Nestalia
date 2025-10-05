using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance { get; private set; }
    public static bool isDialogueActive { get; private set; }

    private Queue<DialogeTurn> dialogTurnQueue;
    
    [SerializeField] private InputActionReference nextDialogueActionReference;
    private InputAction nextDialogueAction;
    [SerializeField] private float typingSpeed = 0.03f;
    private bool isTyping;

    private void Awake()
    {
        instance = this;
        showDialogBox(false);
        isDialogueActive = false;

        if (nextDialogueActionReference != null)
        {
            nextDialogueAction = nextDialogueActionReference.action;
        }
    }

    private void OnEnable()
    {
        if (nextDialogueAction != null)
        {
            nextDialogueAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (nextDialogueAction != null)
        {
            nextDialogueAction.Disable();
        }
    }

    public void startDialogue(DialogeRound dialoge)
    {
        if (isDialogueActive) return;
        
        dialogTurnQueue = new Queue<DialogeTurn>(dialoge.DialogeTurnList);
        StartCoroutine(DialogueCoroutine());
    }

    private IEnumerator DialogueCoroutine()
    {
        isDialogueActive = true;
        Time.timeScale = 0f;
        showDialogBox(true);

        while (dialogTurnQueue.Count > 0)
        {
            var currentTurn = dialogTurnQueue.Dequeue();
            setCharacterInfo(currentTurn.Character);
            
            yield return StartCoroutine(TypeLine(currentTurn.DialogueLine));

            yield return new WaitUntil(() => nextDialogueAction.triggered);
            yield return null;
        }
        
        showDialogBox(false);
        Time.timeScale = 1f;
        isDialogueActive = false;
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        ClearDialogArea();
        
        foreach (char c in line.ToCharArray())
        {
            DialogArea.text += c;
            
            if (nextDialogueAction.triggered)
            {
                DialogArea.text = line;
                break;
            }
            
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        isTyping = false;
    }

    [SerializeField] private RectTransform dialogBox;
    [SerializeField] private Image characterPhoto;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI DialogArea;

    public void showDialogBox(bool isActive)
    {
        dialogBox.gameObject.SetActive(isActive);
    }

    public void setCharacterInfo(DialogeCharacter character)
    {
        if (character == null) return;
        characterPhoto.sprite = character.ProfilePhoto;
        characterName.text = character.Name;
    }

    public void ClearDialogArea()
    {
        DialogArea.text = string.Empty;
    }
}