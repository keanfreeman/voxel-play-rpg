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
    [SerializeField] ConstructionUI constructionUI;
    [SerializeField] PartyInfoUI partyInfoUI;
}
