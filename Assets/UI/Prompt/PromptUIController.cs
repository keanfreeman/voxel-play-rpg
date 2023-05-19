using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PromptUIController : MonoBehaviour
{
    [SerializeField] UIDocument promptUIDocument;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] ConstructionUI constructionUI;

    VisualElement wholeScreen;
    Label titleLabel;
    Label bodyLabel;
    Button yesButton;
    Button noButton;

    Action currYesHandler = null;

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

    public IEnumerator DisplayPrompt(string title, string body, Action yesHandler) {
        constructionUI.SetDisplayState(false);

        titleLabel.text = title;
        bodyLabel.text = body;
        currYesHandler = yesHandler;

        yield return gameStateManager.SetControlState(ControlState.UI);

        Show();
        yesButton.Focus();
    }

    public string GetTimeCostStringFromTimes(int days, int hours, int minutes, int seconds) {
        while (seconds >= 60) {
            minutes += 1;
            seconds -= 60;
        }
        while (minutes >= 60) {
            hours += 1;
            minutes -= 60;
        }
        while (hours >= 24) {
            days += 1;
            hours -= 24;
        }

        List<string> stringsToAdd = new();
        if (days > 0) stringsToAdd.Add(days.ToString() + (days > 2 ? " days" : "day"));
        if (hours > 0) stringsToAdd.Add(hours.ToString() + (hours > 2 ? " hours" : "hour"));
        if (minutes > 0) stringsToAdd.Add(minutes.ToString() + (minutes > 2 ? " minutes" : "minute"));
        if (seconds > 0) stringsToAdd.Add(seconds.ToString() + (seconds > 2 ? " seconds" : "second"));

        if (stringsToAdd.Count > 1) {
            int lastIndex = stringsToAdd.Count - 1;
            stringsToAdd[lastIndex] = " and " + stringsToAdd[lastIndex];

            if (stringsToAdd.Count > 2) {
                for (int i = 0; i < stringsToAdd.Count - 2; i++) {
                    stringsToAdd[i] += ", ";
                }
            }
        }

        string costString = "";
        foreach (string item in stringsToAdd) {
            costString += item;
        }
        return costString;
    }

    private void OnYesPress() {
        currYesHandler?.Invoke();
        currYesHandler = null;
        Hide();
        StartCoroutine(gameStateManager.SetControlState(ControlState.DETACHED));
        constructionUI.SetDisplayState(true);
    }

    private void OnNoPress() {
        Hide();
        StartCoroutine(gameStateManager.SetControlState(ControlState.DETACHED));
        constructionUI.SetDisplayState(true);
    }

    private void Hide() {
        wholeScreen.style.display = DisplayStyle.None;
    }

    private void Show() {
        wholeScreen.style.display = DisplayStyle.Flex;
    }
}
