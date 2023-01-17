using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;

    private int index;

    void Start()
    {
        textComponent.text = string.Empty;
        gameObject.SetActive(false);
    }

    public void StartDialogue() {
        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        index = 0;
        StartCoroutine(TypeLine());
    }

    public void GetNextText() {
        if (!gameObject.activeSelf) {
            StartDialogue();
        }
        else if (textComponent.text.Length == lines[index].Length) {
            NextLine();
        }
        else {
            StopAllCoroutines();
            textComponent.text = lines[index];
        }
    }

    private IEnumerator TypeLine() {
        foreach (char c in lines[index].ToCharArray()) {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void NextLine() {
        if (index < lines.Length - 1) {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else {
            gameObject.SetActive(false);
        }
    }
}
