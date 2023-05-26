using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CustomComponents;

public class StatusUIController : MonoBehaviour
{
    [SerializeField] private UIDocument statusUIDocument;

    VisualElement statusEffectStack;

    private void Awake() {
        statusEffectStack = statusUIDocument.rootVisualElement.Q<VisualElement>("StatusEffectStack");
        statusEffectStack.Clear();
    }
}
