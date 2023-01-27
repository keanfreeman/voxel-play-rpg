using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Ink.Runtime;

public class Dialogue : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private GameObject[] choices;

    private string currentLine;
    private Story currentStory;

    private float TEXT_WAIT_SPEED = 0.01f; 

    private void Start() {
        textComponent.text = string.Empty;
        gameObject.SetActive(false);
    }

    public void StartDialogue(TextAsset inkJSON) {
        textComponent.text = string.Empty;
        gameObject.SetActive(true);

        currentStory = new Story(inkJSON.text);
        currentLine = currentStory.Continue();
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine() {
        foreach (char c in currentLine.ToCharArray()) {
            textComponent.text += c;
            yield return new WaitForSeconds(TEXT_WAIT_SPEED);
        }
    }

    public void HandleInput() {
        if (textComponent.text.Length == currentLine.Length) {
            GetNextLine();
        }
        else {
            StopAllCoroutines();
            textComponent.text = currentLine;
        }
    }

    private void GetNextLine() {
        if (currentStory.canContinue) {
            textComponent.text = string.Empty;
            currentLine = currentStory.Continue();
            StartCoroutine(TypeLine());
        }
        else {
            gameObject.SetActive(false);
        }
    }
}
