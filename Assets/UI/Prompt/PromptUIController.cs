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
    [SerializeField] CombatUI combatUI;

    VisualElement wholeScreen;
    Label titleLabel;
    Label bodyLabel;
    Button yesButton;
    Button noButton;

    bool isConstructionUI;
    bool? isYesSelection = null;

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

    public IEnumerator DisplayPrompt(string title, string body, bool isConstructionUI = true) {
        this.isConstructionUI = isConstructionUI;
        if (isConstructionUI) {
            constructionUI.SetDisplayState(false);
        }
        else {
            combatUI.SetDisplayState(false);
        }

        titleLabel.text = title;
        bodyLabel.text = body;

        yield return gameStateManager.SetControlState(ControlState.UI);

        Show();
        yesButton.Focus();

        while (!isYesSelection.HasValue) {
            yield return null;
        }

        if (isConstructionUI) {
            yield return gameStateManager.SetControlState(ControlState.DETACHED);
        }
        else {
            yield return gameStateManager.SetControlState(ControlState.COMBAT);
        }

        yield return isYesSelection.Value;
        isYesSelection = null;
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
        if (days > 0) stringsToAdd.Add(days.ToString() + (days > 1 ? " days" : " day"));
        if (hours > 0) stringsToAdd.Add(hours.ToString() + (hours > 1 ? " hours" : " hour"));
        if (minutes > 0) stringsToAdd.Add(minutes.ToString() + (minutes > 1 ? " minutes" : " minute"));
        if (seconds > 0) stringsToAdd.Add(seconds.ToString() + (seconds > 1 ? " seconds" : " second"));

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
        isYesSelection = true;
        Hide();

        if (isConstructionUI) {
            constructionUI.SetDisplayState(true);
        }
        else {
            combatUI.SetDisplayState(true);
        }
    }

    private void OnNoPress() {
        isYesSelection = false;
        Hide();

        if (isConstructionUI) {
            constructionUI.SetDisplayState(true);
        }
        else {
            combatUI.SetDisplayState(true);
        }
    }

    private void Hide() {
        wholeScreen.style.display = DisplayStyle.None;
    }

    private void Show() {
        wholeScreen.style.display = DisplayStyle.Flex;
    }
}
