using CustomComponents;
using Instantiated;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CombatUI : UIHandler {
    [SerializeField] UIDocument uiDocument;
    [SerializeField] GameStateManager gameStateManager;
    public const string COMBAT_UI_ROOT = "CombatUI";
    public const string COMBAT_BAR = "CombatBar";
    public const string ACTION_BUTTON = "ActionButton";

    VisualElement combatUIRoot;
    VisualElement combatBar;

    VisualElement choiceInfoBox;
    Label choiceInfoTitle;
    Label choiceInfoBody;

    private List<ActionChoice> actionButtons = new();
    private int currFocus = 0;

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

    private void OnButtonLostFocus(ActionChoice button) {
        SetChoiceInfoBoxDisplayState(false);
    }

    private void OnButtonGainedFocus(ActionChoice button) {
        if (button.currAction == null) {
            SetChoiceInfoBoxDisplayState(false);
            return;
        }

        SetChoiceInfoBoxDisplayState(true);
        choiceInfoTitle.text = button.currAction.actionName;

        if (button.currAction.GetType() == typeof(AttackSO)) {
            AttackSO attack = (AttackSO)button.currAction;
            string rangeString = attack.isRanged ? $"{attack.shortRange}/{attack.longRange}" : "None";
            choiceInfoBody.text = $"Attack Roll: {attack.attackRoll}\n" +
                $"Damage: {attack.damageRoll} {attack.damageType}\n" +
                $"Range: {rangeString}";
        }
        else if (button.currAction.GetType() == typeof(SpecialActionSO)) {
            SpecialActionSO specialActionSO = (SpecialActionSO)button.currAction;
            choiceInfoBody.text = specialActionSO.description;
        }
    }

    public void SetFocus() {
        actionButtons[currFocus].Focus();
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
