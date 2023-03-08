using CustomComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatUITestScript : MonoBehaviour
{
    [SerializeField] public UIDocument uiDocument;

    private List<string> actions = new List<string> {
            "battleaxe",
            "rapier",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"
    };
    private List<string> bonusActions = new List<string> {
            "healing word",
            "dash",
            "second attack",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"
    };
    private struct ComponentIds {
        public const string ACTION_CHOICE_BAR = "ActionChoiceBar";
        public const string ACTION_BUTTON = "ActionButton";
        public const string BONUS_ACTION_BUTTON = "BonusActionButton";
    }

    void Awake()
    {
        //InputManager inputManager = new InputManager(new PlayerInputActions());
        //inputManager.SwitchPlayerControlStateToUI();
        //
        //ActionChoiceBar actionChoiceBar = uiDocument.rootVisualElement.Q<ActionChoiceBar>();
        //Button actionButton = uiDocument.rootVisualElement.Q<Button>(ComponentIds.ACTION_BUTTON);
        //Button bonusActionButton = 
        //    uiDocument.rootVisualElement.Q<Button>(ComponentIds.BONUS_ACTION_BUTTON);
        //actionChoiceBar.PopulateBar(actions, bonusActions, actionButton, bonusActionButton);
        //actionButton.Focus();
    }

    void Update()
    {

    }
}
