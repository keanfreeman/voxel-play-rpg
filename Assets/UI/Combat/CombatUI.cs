using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CombatUI : UIHandler {
    [SerializeField] UIDocument uiDocument;
    [SerializeField] GameStateManager gameStateManager;
    public const string COMBAT_UI_ROOT = "CombatUI";
    public const string ACTION_BUTTON = "ActionButton";

    VisualElement combatUIRoot;
    Button actionButton;

    void Awake() {
        combatUIRoot = uiDocument.rootVisualElement.Q<VisualElement>(COMBAT_UI_ROOT);
        actionButton = combatUIRoot.Q<Button>(ACTION_BUTTON);
    }

    public void SetFocus() {
        actionButton.Focus();
    }

    public override void SetDisplayState(bool isVisible) {
        if (isVisible) {
            combatUIRoot.style.display = DisplayStyle.Flex;
        }
        else {
            combatUIRoot.style.display = DisplayStyle.None;
        }
    }

    public override void HandleNavigate(InputAction.CallbackContext obj) { }
    public override void HandleCancelNavigate(InputAction.CallbackContext obj) { }
    public override void HandleSubmit(InputAction.CallbackContext obj) { }
    public override void HandleCancel(InputAction.CallbackContext obj) {
        gameStateManager.CloseCombatBar();
    }
}
