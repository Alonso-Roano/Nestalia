using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance { get; private set; }
    public static bool isDialogueActive { get; private set; }

    private Queue<DialogeTurn> dialogTurnQueue;

    [SerializeField] private float typingSpeed = 0.03f;

    // Flag que se activa por UnityEvent
    private bool nextPressed = false;

    private void Awake()
    {
        instance = this;
        showDialogBox(false);
        isDialogueActive = false;
    }

    // Este método LO LLAMA UnityEvents desde el Input System
    public void OnNextDialogue(InputAction.CallbackContext ctx)
    {
        nextPressed = true;
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
            nextPressed = false;

            var currentTurn = dialogTurnQueue.Dequeue();
            setCharacterInfo(currentTurn.Character);

            yield return StartCoroutine(TypeLine(currentTurn.DialogueLine));

            // Espera a que UnityEvent dispare OnNextDialogue()
            yield return new WaitUntil(() => nextPressed);
        }

        showDialogBox(false);
        Time.timeScale = 1f;
        isDialogueActive = false;
    }

    private IEnumerator TypeLine(string line)
    {
        ClearDialogArea();

        foreach (char c in line.ToCharArray())
        {
            DialogArea.text += c;

            if (nextPressed)
            {
                DialogArea.text = line;
                break;
            }

            yield return new WaitForSecondsRealtime(typingSpeed);
        }
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
