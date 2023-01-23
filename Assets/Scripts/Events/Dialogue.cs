using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class Dialogue : MonoBehaviour {
    public TextMeshProUGUI textComponent;

    private List<string> sentences;
    private int index;

    private float TEXT_SPEED = 0.02f; 

    private void Start() {
        textComponent.text = string.Empty;
        sentences = new List<string>();
        gameObject.SetActive(false);
    }

    public void StartDialogue(List<string> sentences) {
        this.sentences = sentences;
        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        index = 0;
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine() {
        foreach (char c in sentences[index].ToCharArray()) {
            textComponent.text += c;
            yield return new WaitForSeconds(TEXT_SPEED);
        }
    }

    public void HandleReturn() {
        if (textComponent.text.Length == sentences[index].Length) {
            GetNextLine();
        }
        else {
            StopAllCoroutines();
            textComponent.text = sentences[index];
        }
    }

    private void GetNextLine() {
        if (index < sentences.Count - 1) {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else {
            gameObject.SetActive(false);
        }
    }
}
