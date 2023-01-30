using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Ink.Runtime;

public class Dialogue : MonoBehaviour {
    private string currentLine;
    private Story currentStory;
    private VisualElement root;
    private Label dialogueText;

    private float TEXT_WAIT_SPEED = 0.01f;
    private const string GIVEN_TEXT = "GivenText";

    private void Start() {
        root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        dialogueText = root.Query<Label>(GIVEN_TEXT).First();
        dialogueText.text = string.Empty;

        SetUIVisibility(false);
    }

    private void SetUIVisibility(bool newVisibilityValue) {
        root.style.visibility = newVisibilityValue ? Visibility.Visible : Visibility.Hidden;
    }

    public void StartDialogue(TextAsset inkJSON) {
        dialogueText.text = string.Empty;
        SetUIVisibility(true);

        currentStory = new Story(inkJSON.text);
        currentLine = currentStory.Continue();
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine() {
        foreach (char c in currentLine.ToCharArray()) {
            dialogueText.text += c;
            yield return new WaitForSeconds(TEXT_WAIT_SPEED);
        }
    }

    // returns true if dialogue has ended
    public bool HandleInput() {
        if (dialogueText.text.Length == currentLine.Length) {
            return GetNextLine();
        }
        else {
            StopAllCoroutines();
            dialogueText.text = currentLine;
            return false;
        }
    }

    private bool GetNextLine() {
        if (currentStory.canContinue) {
            dialogueText.text = string.Empty;
            currentLine = currentStory.Continue();
            StartCoroutine(TypeLine());
            return false;
        }
        else {
            SetUIVisibility(false);
            return true;
        }
    }
}
