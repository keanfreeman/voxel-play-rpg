using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat
{
    public NPCBehavior firstCombatant;

    private NonVoxelWorld nonVoxelWorld;
    private System.Random rng;
    private Dice dice;
    private PartyManager partyManager;

    private GameObject playerCombatants;

    List<KeyValuePair<int, MonoBehaviour>> initiatives;

    public Combat(NonVoxelWorld nonVoxelWorld, System.Random rng, Dice dice,
            PartyManager partyManager) {
        this.nonVoxelWorld = nonVoxelWorld;
        this.rng = rng;
        this.dice = dice;
        this.partyManager = partyManager;
    }

    public void RunCombat() {
        if (initiatives == null) {
            SetCombatantsAndInitiativeOrder();
        }


    }

    private void SetCombatantsAndInitiativeOrder() {
        initiatives = new List<KeyValuePair<int, MonoBehaviour>>();

        int playerModifier = StatModifiers.GetModifierForStat(
            partyManager.playerCharacter.stats.dexterity);
        int playerInitiative = dice.Roll(1, 20, playerModifier);
        initiatives.Add(new KeyValuePair<int, MonoBehaviour>(playerInitiative,
            partyManager.playerCharacter));

        foreach (NPCBehavior npcBehavior in firstCombatant.teammates) {
            int modifier = StatModifiers.GetModifierForStat(
                npcBehavior.npcInfo.stats.dexterity);
            int initiative = dice.Roll(1, 20, modifier);
            initiatives.Add(new KeyValuePair<int, MonoBehaviour>(initiative, npcBehavior));
        }

        initiatives.Sort((x, y) => -x.Key.CompareTo(y.Key));
    }
}
