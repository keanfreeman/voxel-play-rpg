using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MessageUIController : MonoBehaviour
{
    [SerializeField] UIDocument messageUIDocument;
    [SerializeField] float FADE_TIME = 0.3f;

    VisualElement wholeScreen;
    Label messageText;

    void Awake() {
        wholeScreen = messageUIDocument.rootVisualElement.Q<VisualElement>("WholeScreen");
        messageText = wholeScreen.Q<Label>("MessageText");

        wholeScreen.style.opacity = 0;
        messageText.text = "";
    }

    public IEnumerator DisplayText(string text) {
        messageText.text = text;
        if (wholeScreen.style.opacity.value == 0) {
            yield return Fade(fadeIn: true);
        }
    }

    public IEnumerator Hide() {
        if (wholeScreen.style.opacity.value != 0) {
            yield return Fade(fadeIn: false);
        }
        messageText.text = "";
    }

    private IEnumerator Fade(bool fadeIn) {
        float startOpacity = fadeIn ? 0 : 1;
        float endOpacity = fadeIn ? 1 : 0;

        float startTime = Time.time;
        while (Time.time - startTime < FADE_TIME) {
            float percentElapsed = (Time.time - startTime) / FADE_TIME;
            wholeScreen.style.opacity = Mathf.Lerp(startOpacity, endOpacity, percentElapsed);
            yield return null;
        }
        wholeScreen.style.opacity = endOpacity;
    }
}
