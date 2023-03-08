using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatUI : MonoBehaviour {
    [SerializeField] UIDocument uiDocument;
    public const string COMBAT_UI_ROOT = "CombatUI";
    public const string ACTION_BUTTON = "ActionButton";

    VisualElement combatUIRoot;
    Button actionButton;

    void Awake() {
        combatUIRoot = uiDocument.rootVisualElement.Q<VisualElement>(COMBAT_UI_ROOT);
        actionButton = combatUIRoot.Q<Button>(ACTION_BUTTON);
    }

    public void ApplyFocus() {
        actionButton.Focus();
    }

    public void RemoveFocus() {
        uiDocument.rootVisualElement.focusController.focusedElement.Blur();
    }

    public void SetDisplayState(bool display) {
        combatUIRoot.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
