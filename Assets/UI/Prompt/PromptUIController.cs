using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PromptUIController : MonoBehaviour
{
    [SerializeField] UIDocument promptUIDocument;

    VisualElement wholeScreen;
    Label titleLabel;
    Label bodyLabel;
    Button yesButton;
    Button noButton;

    private void Awake() {
        wholeScreen = promptUIDocument.rootVisualElement.Q<VisualElement>("WholeScreen");
        titleLabel = wholeScreen.Q<Label>("Title");
        bodyLabel = wholeScreen.Q<Label>("Body");
        yesButton = wholeScreen.Q<Button>("YesButton");
        noButton = wholeScreen.Q<Button>("NoButton");
        yesButton.clicked += OnYesPress;
        noButton.clicked += OnNoPress;

        Hide();
    }

    public void DisplayPrompt(string title, string body) {
        titleLabel.text = title;
        bodyLabel.text = body;
        Show();
    }

    public void OnYesPress() {
        Hide();
    }

    public void OnNoPress() {
        Hide();
    }

    private void Hide() {
        wholeScreen.style.display = DisplayStyle.None;
    }

    private void Show() {
        wholeScreen.style.display = DisplayStyle.Flex;
    }
}
