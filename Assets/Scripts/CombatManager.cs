using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] PartyManager partyManager;
    [SerializeField] RandomManager randomManager;
    [SerializeField] MovementManager movementManager;
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] PlayerMovement playerMovement;
    
    Pathfinder pathfinder;
    NPCBehavior firstCombatant;
    GameObject playerCombatants;
    List<KeyValuePair<int, Traveller>> initiatives;
    int currInitiative = -1;

    private void Awake() {
        pathfinder = new Pathfinder(spriteMovement);
    }

    public void RunCombat() {
        if (initiatives == null) {
            SetCombatantsAndInitiativeOrder();
        }

        // decide what to do on turn
        Traveller currCreature = initiatives[currInitiative].Value;
        if (movementManager.IsMoving(currCreature)) {
            return;
        }
        else {
            IncrementInitiative();
            currCreature = initiatives[currInitiative].Value;
        }
        if (currCreature.GetType() == typeof(NPCBehavior)) {
            // move towards player
            List<Vector3Int> path = pathfinder.FindPath(currCreature.currVoxel, playerMovement.currVoxel, false);
            movementManager.MoveAlongPath(currCreature, path);
        }
        else {
            // give player control
            Debug.Log("Player turn");
        }
    }

    public void SetFirstCombatant(NPCBehavior firstCombatant) {
        this.firstCombatant = firstCombatant;
    }

    private void SetCombatantsAndInitiativeOrder() {
        initiatives = new List<KeyValuePair<int, Traveller>>();

        int playerModifier = StatModifiers.GetModifierForStat(
            partyManager.playerCharacter.stats.dexterity);
        int playerInitiative = randomManager.dice.Roll(1, 20, playerModifier);
        initiatives.Add(new KeyValuePair<int, Traveller>(playerInitiative,
            playerMovement));

        foreach (NPCBehavior npcBehavior in firstCombatant.teammates) {
            int modifier = StatModifiers.GetModifierForStat(
                npcBehavior.npcInfo.stats.dexterity);
            int initiative = randomManager.dice.Roll(1, 20, modifier);
            initiatives.Add(new KeyValuePair<int, Traveller>(initiative, npcBehavior));
        }

        initiatives.Sort((x, y) => -x.Key.CompareTo(y.Key));
        currInitiative = 0;
    }

    private void IncrementInitiative() {
        if (currInitiative >= initiatives.Count - 1) {
            currInitiative = 0;
        }
        else {
            currInitiative += 1;
        }
    }
}
