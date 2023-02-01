using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Ink.Runtime;
using System.Linq;
using System;
using UnityEngine.InputSystem;

public class Dialogue : MonoBehaviour {
    private string currentLine;
    private Story currentStory;
    
    public bool isDialogueActive;
    private bool handledButton = false;

    // UIDocument
    private VisualElement root;
    private Label dialogueText;
    private VisualElement choiceHolder;

    private const float TEXT_WAIT_SPEED = 0.01f;
    private const string GIVEN_TEXT = "GivenText";
    private const string CHOICE_HOLDER = "ChoiceHolder";

    private void Start() {
        isDialogueActive = false;

        root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        dialogueText = root.Query<Label>(GIVEN_TEXT).First();
        choiceHolder = root.Query<VisualElement>(CHOICE_HOLDER).First();

        SetUIVisibility(false);
        dialogueText.text = string.Empty;
        choiceHolder.Clear();
    }

    private void SetUIVisibility(bool isVisible) {
        root.style.visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
    }

    public void StartDialogue(Story story) {
        isDialogueActive = true;
        dialogueText.text = string.Empty;
        SetUIVisibility(true);

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

    public void HandleInput() {
        // todo - remove this hack and only handle input through button presses
        if (handledButton) {
            handledButton = false;
            return;
        }

        if (dialogueText.text.Length == currentLine.Length) {
            // display next text
            if (currentStory.canContinue) {
                dialogueText.text = string.Empty;
                currentLine = currentStory.Continue();
                StartCoroutine(TypeLine());
            }

            // end dialogue
            isDialogueActive = false;
            SetUIVisibility(false);
        }

        // display remaining text
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
        if (!isDialogueActive) {
            Debug.LogError("Clicked a button when the dialogue was inactive.");
            return;
        }

        handledButton = true;
        currentStory.ChooseChoiceIndex(choiceIndex);
        choiceHolder.Clear();
        if (currentStory.canContinue) {
            dialogueText.text = string.Empty;
            currentLine = currentStory.Continue();
            StartCoroutine(TypeLine());
        }
        else {
            isDialogueActive = false;
        }
    }
}
