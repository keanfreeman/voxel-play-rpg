using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatUI : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    public const string COMBAT_UI_ROOT = "CombatUI";

    VisualElement combatUIRoot;

    void Awake()
    {
        combatUIRoot = uiDocument.rootVisualElement.Q<VisualElement>(COMBAT_UI_ROOT);
    }

    public void SetDisplayState(bool display) {
        combatUIRoot.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
