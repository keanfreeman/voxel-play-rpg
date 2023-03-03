using CustomComponents;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MainGameUI : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] CombatUI combatUI;
    [SerializeField] DialogueUI dialogueUI;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.K)) {
        //    uiDocument.rootVisualElement.Q<VisualElement>("CombatUI").style.display = DisplayStyle.None;
        //    uiDocument.rootVisualElement.Q<VisualElement>("DialogueUI").style.display = DisplayStyle.Flex;
        //}
    }
}
