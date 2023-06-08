using CustomComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PartyInfoUI : UIHandler {
    [SerializeField] UIDocument uiDocument;
    [SerializeField] InputManager inputManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] CombatUI combatUI;

    VisualElement partyInfoUIRoot;
    PartyInfoDisplayer partyInfoDisplayer;

    void Awake() {
        partyInfoUIRoot = uiDocument.rootVisualElement.Q<VisualElement>("PartyInfoUI");
        partyInfoDisplayer = partyInfoUIRoot.Q<PartyInfoDisplayer>();
    }

    public void StartPartyInfoUI(InputAction.CallbackContext obj) {
        inputManager.LockPlayerControls();
        combatUI.SetDisplayState(false);
        partyInfoDisplayer.SetDisplayValues(partyManager.currControlledCharacter);
        SetDisplayState(true);
        inputManager.UnlockUIControls(this);
    }

    public override void SetDisplayState(bool isVisible) {
        partyInfoUIRoot.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }
    public override void HandleNavigate(InputAction.CallbackContext obj) {}
    public override void HandleCancelNavigate(InputAction.CallbackContext obj) { }
    public override void HandleSubmit(InputAction.CallbackContext obj) { }
    public override void HandleCancel(InputAction.CallbackContext obj) {
        inputManager.LockUIControls();
        SetDisplayState(false);
        combatUI.SetDisplayState(true);
        inputManager.UnlockPlayerControls();
    }
}
