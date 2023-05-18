using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceRollerUIController : MonoBehaviour
{
    [SerializeField] UIDocument diceUIDocument;

    VisualElement wholeScreen;
    Label rollerText;

    const float FADE_TIME = 0.3f;

    void Awake() {
        wholeScreen = diceUIDocument.rootVisualElement.Q<VisualElement>("WholeScreen");
        rollerText = wholeScreen.Q<Label>("RollerText");

        wholeScreen.style.opacity = 0;
        rollerText.text = "";
    }

    public IEnumerator DisplayText(string text) {
        rollerText.text = text;
        if (wholeScreen.style.opacity.value == 0) {
            yield return Fade(fadeIn: true);
        }
    }

    public IEnumerator Hide() {
        if (wholeScreen.style.opacity.value != 0) {
            yield return Fade(fadeIn: false);
        }
        rollerText.text = "";
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
