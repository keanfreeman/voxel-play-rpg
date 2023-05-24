using GameMechanics;
using Instantiated;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    [SerializeField] MessageManager messageManager;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] CombatManager combatManager;

    public IEnumerator PerformAction(Traveller performer, ActionSO action) {
        if (action.GetType() == typeof(AttackSO)) {
            AttackSO attack = (AttackSO)action;

            if (!attack.isRanged && gameStateManager.controlState != ControlState.COMBAT) {
                messageManager.DisplayMessage("Cannot perform melee attack outside of combat.");
                yield break;
            }

            CoroutineWithData rangedAttackCoroutine = new(this, PerformRangedAttack(performer, attack));
            yield return rangedAttackCoroutine.coroutine;
            HashSet<NPC> enemies = rangedAttackCoroutine.result == null ? null 
                : (HashSet<NPC>)rangedAttackCoroutine.result;
            if (enemies != null && enemies.Count > 0 
                    && gameStateManager.controlState != ControlState.COMBAT) {
                EnterCombatAfterAttacking(enemies);
            }
        }
        else if (action.GetType() == typeof(SpecialActionSO)) {
            SpecialActionSO specialActionSO = (SpecialActionSO)action;
            // todo - use non-string identifier
            if (specialActionSO.actionName == "Second Wind") {

            }
        }
        else {
            throw new System.NotImplementedException($"Need to implement a description for " +
                $"{action.GetType()}");
        }
    }

    private IEnumerator PerformRangedAttack(Traveller performer, AttackSO attack) {
        messageManager.DisplayMessage(new Message(
            "Please select a creature to attack", isPermanent: true));
        CoroutineWithData cwd = new(this, detachedCamera.EnterSelectMode(performer.origin));
        yield return cwd.coroutine;

        messageManager.StopDisplayingPermanentMessage();
        if (cwd.result == null) {
            messageManager.DisplayMessage("No creature at that position.");
            yield return null;
            yield break;
        }

        Vector3Int targetPosition = (Vector3Int)cwd.result;
        InstantiatedEntity entity = nonVoxelWorld.GetEntityFromPosition(targetPosition);
        if (entity == null || !TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(NPC))) {
            messageManager.DisplayMessage("Must choose an NPC as a target.");
            yield return null;
            yield break;
        }

        NPC npc = (NPC)entity;
        if (npc.GetFaction() != EntityDefinition.Faction.ENEMY) {
            messageManager.DisplayMessage("Cannot target a non-enemy NPC for an attack.");
            yield return null;
            yield break;
        }
        HashSet<NPC> enemies = npc.teammates;
        yield return combatManager.PerformAttack(performer, attack, npc);
        if (!nonVoxelWorld.npcs.Contains(npc)) {
            // NPC was killed
            enemies.Remove(npc);
        }
        yield return enemies;
    }

    private void EnterCombatAfterAttacking(ICollection<NPC> enemies) {
        combatManager.SetEnemies(enemies);
        gameStateManager.EnterCombat(null);
        combatManager.StartCombat();
    }
}
