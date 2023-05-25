using CustomComponents;
using Instantiated;
using Spells;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CombatUI : UIHandler {
    [SerializeField] UIDocument uiDocument;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] ActionManager actionManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] CombatManager combatManager;

    public const string COMBAT_UI_ROOT = "CombatUI";
    public const string COMBAT_BAR = "CombatBar";
    public const string ACTION_BUTTON = "ActionButton";

    VisualElement combatUIRoot;
    VisualElement combatBar;

    VisualElement choiceInfoBox;
    Label choiceInfoTitle;
    Label choiceInfoBody;

    private List<ActionChoice> actionButtons = new();
    private ActionChoice currHighlightedButton;

    void Awake() {
        combatUIRoot = uiDocument.rootVisualElement.Q<VisualElement>(COMBAT_UI_ROOT);
        combatBar = combatUIRoot.Q<VisualElement>(COMBAT_BAR);
        choiceInfoBox = combatUIRoot.Q<VisualElement>("ChoiceInfoBox");
        choiceInfoTitle = combatUIRoot.Q<Label>("Title");
        choiceInfoBody = combatUIRoot.Q<Label>("Body");

        int iterator = 0;
        foreach (ActionChoice button in combatBar.Children()) {
            button.text = "";
            button.position = iterator;
            actionButtons.Add(button);
            button.gainedFocus += OnButtonGainedFocus;
            button.lostFocus += OnButtonLostFocus;
            button.selected += OnButtonSelected;
            iterator++;
        }
        SetChoiceInfoBoxDisplayState(false);
    }

    public void SetActions(PlayerCharacter playerCharacter) {
        List<ActionSO> actions = playerCharacter.GetActions();
        // todo - make possible with more than 10 actions
        for (int i = 0; i < 10; i++) {
            if (i < actions.Count) {
                actionButtons[i].UpdateAction(actions[i]);
            }
            else {
                actionButtons[i].ClearAction();
            }
        }
    }

    private void OnButtonSelected(ActionChoice button) {
        if (button.currAction == null) {
            return;
        }

        PlayerCharacter currPC = gameStateManager.controlState == ControlState.COMBAT
            ? combatManager.GetCurrTurnPlayer() : partyManager.currControlledCharacter;
        StartCoroutine(actionManager.PerformAction(currPC, button.currAction));
    }

    private void OnButtonLostFocus(ActionChoice button) {
        SetChoiceInfoBoxDisplayState(false);
    }

    private void OnButtonGainedFocus(ActionChoice button) {
        currHighlightedButton = button;
        if (button.currAction == null) {
            SetChoiceInfoBoxDisplayState(false);
            return;
        }

        SetChoiceInfoBoxDisplayState(true);
        choiceInfoTitle.text = button.currAction.actionName;

        if (button.currAction.GetType() == typeof(AttackSO)) {
            AttackSO attack = (AttackSO)button.currAction;
            choiceInfoBody.text = attack.GetDescription();
        }
        else if (button.currAction.GetType() == typeof(SpecialActionSO)) {
            SpecialActionSO specialActionSO = (SpecialActionSO)button.currAction;
            choiceInfoBody.text = specialActionSO.description;
        }
        else if (button.currAction.GetType() == typeof(SpellSO)) {
            SpellSO spell = (SpellSO)button.currAction;
            string bodyText = $"{spell.description}";
            bodyText += spell.providedAttack != null ? $"\n\n{spell.providedAttack.GetDescription()}" : "";
            choiceInfoBody.text = bodyText;
        }
        else {
            throw new System.NotImplementedException($"Need to implement a description for " +
                $"{button.currAction.GetType()}");
        }
    }

    public void SetFocus() {
        currHighlightedButton ??= actionButtons[0];
        currHighlightedButton.Focus();
    }

    public void StopFocus() {
        currHighlightedButton.Blur();
    }

    private void SetChoiceInfoBoxDisplayState(bool isVisible) {
        if (isVisible) {
            choiceInfoBox.style.display = DisplayStyle.Flex;
        }
        else {
            choiceInfoBox.style.display = DisplayStyle.None;
        }
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
