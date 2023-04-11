using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Ink.Runtime;
using System.Linq;
using System;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour {
    [SerializeField] UIDocument uiDocument;
    [SerializeField] GameStateManager gameStateManager;

    private string currentLine;
    private Story currentStory;
    
    private bool inChoice = false;

    // UIDocument
    private TemplateContainer dialogueRoot;
    private Label dialogueText;
    private VisualElement choiceHolder;

    private const float TEXT_WAIT_SPEED = 0.01f;
    private const string DIALOGUE_UI = "DialogueUI";
    private const string GIVEN_TEXT = "DialogueGivenText";
    private const string CHOICE_HOLDER = "DialogueChoiceHolder";

    private void Awake() {
        dialogueRoot = uiDocument.rootVisualElement.Q<TemplateContainer>(DIALOGUE_UI);
        dialogueText = dialogueRoot.Q<Label>(GIVEN_TEXT);
        choiceHolder = dialogueRoot.Q<VisualElement>(CHOICE_HOLDER);

        dialogueText.text = string.Empty;
        choiceHolder.Clear();
    }

    private void SetDisplayState(bool isVisible) {
        dialogueRoot.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void StartDialogue(Story story) {
        dialogueText.text = string.Empty;
        SetDisplayState(true);

        currentStory = story;
        currentLine = currentStory.Continue();

        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine() {
        foreach (char c in currentLine.ToCharArray()) {
            dialogueText.text += c;
            if (dialogueText.text.Length == currentLine.Length) {
                DisplayChoices();
            }
            yield return new WaitForSeconds(TEXT_WAIT_SPEED);
        }
    }

    public void HandleSubmit(InputAction.CallbackContext obj) {
        if (gameStateManager.controlState != ControlState.DIALOGUE || inChoice) {
            return;
        }

        // if done with text, continue
        if (dialogueText.text.Length == currentLine.Length) {
            // display next text
            if (currentStory.canContinue) {
                dialogueText.text = string.Empty;
                currentLine = currentStory.Continue();
                StartCoroutine(TypeLine());
            }

            // end dialogue
            SetDisplayState(false);
            gameStateManager.ExitDialogue();
        }

        // skip dialogue display
        StopAllCoroutines();
        dialogueText.text = currentLine;
        DisplayChoices();
    }

    private void DisplayChoices() {
        choiceHolder.Clear();
        List<Choice> choices = currentStory.currentChoices;
        if (choices.Count > 0) {
            for (int i = 0; i < choices.Count; i++) {
                choiceHolder.Add(CreateButton(choices[i].text, i));
            }
            choiceHolder.Children().First().Focus();
            inChoice = true;
        }
    }

    private Button CreateButton(string text, int choiceIndex) {
        Button button = new Button {
            text = text
        };
        button.clicked += () => HandleButtonClick(choiceIndex);
        return button;
    }

    private void HandleButtonClick(int choiceIndex) {
        currentStory.ChooseChoiceIndex(choiceIndex);
        choiceHolder.Clear();
        if (currentStory.canContinue) {
            dialogueText.text = string.Empty;
            currentLine = currentStory.Continue();
            StartCoroutine(TypeLine());
        }
        else {
            gameStateManager.ExitDialogue();
        }
        inChoice = false;
    }
}
