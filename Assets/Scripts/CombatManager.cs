using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] PartyManager partyManager;
    [SerializeField] RandomManager randomManager;
    
    private NPCBehavior firstCombatant;
    private GameObject playerCombatants;
    List<KeyValuePair<int, MonoBehaviour>> initiatives;

    public void RunCombat() {
        if (initiatives == null) {
            SetCombatantsAndInitiativeOrder();
        }

        // TODO
    }

    public void SetFirstCombatant(NPCBehavior firstCombatant) {
        this.firstCombatant = firstCombatant;
    }

    private void SetCombatantsAndInitiativeOrder() {
        initiatives = new List<KeyValuePair<int, MonoBehaviour>>();

        int playerModifier = StatModifiers.GetModifierForStat(
            partyManager.playerCharacter.stats.dexterity);
        int playerInitiative = randomManager.dice.Roll(1, 20, playerModifier);
        initiatives.Add(new KeyValuePair<int, MonoBehaviour>(playerInitiative,
            partyManager.playerCharacter));

        foreach (NPCBehavior npcBehavior in firstCombatant.teammates) {
            int modifier = StatModifiers.GetModifierForStat(
                npcBehavior.npcInfo.stats.dexterity);
            int initiative = randomManager.dice.Roll(1, 20, modifier);
            initiatives.Add(new KeyValuePair<int, MonoBehaviour>(initiative, npcBehavior));
        }

        initiatives.Sort((x, y) => -x.Key.CompareTo(y.Key));
    }
}
