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

    private GameObject playerCombatants;


    List<KeyValuePair<int, MonoBehaviour>> initiatives;

    public Combat(NonVoxelWorld nonVoxelWorld, System.Random rng, Dice dice) {
        this.nonVoxelWorld = nonVoxelWorld;
        this.rng = rng;
        this.dice = dice;
    }

    public void RunCombat() {
        if (initiatives == null) {
            initiatives = new List<KeyValuePair<int, MonoBehaviour>>();
            // todo really find player characters and roll their initiatives
            foreach (NPCBehavior npcBehavior in firstCombatant.teammates) {
                int modifier = StatModifiers.GetModifierForStat(
                    npcBehavior.npcInfo.stats.dexterity);
                int initiative = dice.Roll(1, 20, modifier);
                initiatives.Add(new KeyValuePair<int, MonoBehaviour>(initiative, npcBehavior));
            }

            initiatives.Sort((x, y) => x.Key.CompareTo(y.Key));
        }


    }
}
