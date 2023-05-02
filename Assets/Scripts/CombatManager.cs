using GameMechanics;
using Ink;
using Instantiated;
using Nito.Collections;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatManager : MonoBehaviour
{
    [SerializeField] PartyManager partyManager;
    [SerializeField] RandomManager randomManager;
    [SerializeField] MovementManager movementManager;
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] InputManager inputManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] Pathfinder pathfinder;
    [SerializeField] EffectManager effectManager;
    
    NPC firstCombatant;
    List<KeyValuePair<int, Traveller>> initiatives;
    int currInitiative = -1;
    Dictionary<Traveller, CombatResources> usedResources = new Dictionary<Traveller, CombatResources>();

    private const int TILE_TO_FEET = 5;

    public void StartCombat() {
        if (initiatives == null) {
            SetCombatantsAndInitiativeOrder();
            StartCoroutine(RunTurn(currInitiative));
        }
    }

    public IEnumerator RunTurn(int initiative) {
        Traveller currCreature = initiatives[initiative].Value;
        yield return cameraManager.MoveCameraToTargetCreature(currCreature);

        usedResources[currCreature] = new CombatResources();
        if (currCreature.GetType() == typeof(PlayerCharacter)) {
            inputManager.SwitchPlayerToDetachedControlState(currCreature.origin);
            yield break;
        }

        inputManager.SwitchDetachedToWatchControlState();

        // find nearest enemy
        PlayerCharacter nearestPlayer = partyManager.FindNearestPlayer(currCreature.origin);
        NPC creatureAsNPC = (NPC) currCreature;
        if (!Coordinates.IsNextTo(creatureAsNPC, nearestPlayer)) {
            // try move towards enemy
            int maxSearchLength = (creatureAsNPC.GetStats().baseSpeed / TILE_TO_FEET) * 3;

            CoroutineWithData coroutineWithData = new CoroutineWithData(this,
                pathfinder.FindPath(currCreature, nearestPlayer.origin, maxSearchLength));
            yield return coroutineWithData.coroutine;
            Deque<Vector3Int> path = (Deque<Vector3Int>) coroutineWithData.result;
            while (path.Count * TILE_TO_FEET > creatureAsNPC.GetStats().baseSpeed * TILE_TO_FEET) {
                path.RemoveFromFront();
            }
            yield return movementManager.MoveAlongPath(currCreature, path);
        }

        // attack enemy
        if (Coordinates.IsNextTo(creatureAsNPC, nearestPlayer)) {
            // TODO use less brittle attack selection method
            AttackSO npcAttack = (AttackSO)creatureAsNPC.GetStats().actions[0];
            int attackRoll = randomManager.Roll(npcAttack.attackRoll);
            Debug.Log($"NPC rolled {attackRoll} for their attack roll.");
            if (attackRoll >= nearestPlayer.GetStats().CalculateArmorClass()) {
                int damageRoll = randomManager.Roll(npcAttack.damageRoll);
                Debug.Log($"NPC rolled {damageRoll} for their damage roll.");
                int newHP = nearestPlayer.currHP - damageRoll;
                nearestPlayer.SetHP(newHP);
                if (newHP < 1) {
                    // TODO - lose on death and no party members
                    Debug.Log("Player ran out of HP.");
                }

                yield return effectManager.GenerateHitEffect(nearestPlayer);
            }
        }

        inputManager.DisableWatchState();
        ResetCombatResources(currCreature);
        IncrementInitiative();
        StartCoroutine(RunTurn(currInitiative));
    }

    public void HandleDetachedSelect(InputAction.CallbackContext obj) {
        StartCoroutine(ExecuteDetachedSelect());
    }

    public IEnumerator ExecuteDetachedSelect() {
        if (gameStateManager.controlState != ControlState.COMBAT) {
            yield break;
        }

        Traveller currCreature = initiatives[currInitiative].Value;
        InstantiatedEntity selectedEntity = nonVoxelWorld.GetEntityFromPosition(detachedCamera.currVoxel);
        if (currCreature.GetType() != typeof(PlayerCharacter) || movementManager.IsMoving(currCreature)
                || (selectedEntity != null && selectedEntity.GetType() == typeof(PlayerCharacter))) {
            yield break;
        }

        PlayerCharacter playerMovement = (PlayerCharacter)currCreature;

        // check if player wanted to attack
        if (selectedEntity != null && selectedEntity.GetType() == typeof(NPC)) {
            if (usedResources[currCreature].usedAction) {
                Debug.Log("Player tried to use action twice.");
                yield break;
            }
            NPC npcBehavior = (NPC)selectedEntity;

            ActionSO validAttackAction;
            if (!Coordinates.IsNextTo(currCreature, npcBehavior)) {
                validAttackAction = StatInfo.GetRangedAction(playerMovement.GetStats());
                if (validAttackAction == null) {
                    // try moving towards the enemy, then attacking
                    yield return TryMovePlayer(playerMovement);
                    if (!Coordinates.IsNextTo(currCreature, npcBehavior)) {
                        // couldn't move close enough
                        Debug.Log("Couldn't get close enough to make a melee attack.");
                        yield break;
                    }
                    else {
                        validAttackAction = StatInfo.GetMeleeActionThenRanged(playerMovement.GetStats());
                    }
                }
            }
            else {
                validAttackAction = StatInfo.GetMeleeActionThenRanged(playerMovement.GetStats());
            }

            if (validAttackAction == null) {
                Debug.Log("Player had no attack action they could use.");
                yield break;
            }

            List<AttackSO> attacksToDo;
            if (validAttackAction.GetType() == typeof(MultiattackSO)) {
                attacksToDo = ((MultiattackSO)validAttackAction).attacks;
            }
            else {
                attacksToDo = new List<AttackSO> { (AttackSO)validAttackAction };
            }

            foreach (AttackSO attack in attacksToDo) {
                int attackRoll = randomManager.Roll(attack.attackRoll);
                Debug.Log($"Player rolled {attackRoll} for their attack roll.");
                if (attackRoll >= npcBehavior.GetStats().CalculateArmorClass()) {
                    int damageRoll = randomManager.Roll(attack.damageRoll);
                    Debug.Log($"Player rolled {damageRoll} for their damage roll.");
                    int newHP = npcBehavior.currHP - damageRoll;
                    npcBehavior.SetHP(newHP);
                    if (newHP < 1) {
                        Debug.Log("NPC defeated");
                        foreach (KeyValuePair<int, Traveller> initiative in initiatives) {
                            if (initiative.Value == selectedEntity) {
                                // ensure initiative number is not incorrect after deleting item
                                int currCreatureInitiative = initiatives[currInitiative].Key;
                                if (initiative.Key > currCreatureInitiative) {
                                    currInitiative -= 1;
                                }

                                nonVoxelWorld.DeleteEntity(npcBehavior);
                                initiatives.Remove(initiative);
                                break;
                            }
                        }
                    }

                    yield return effectManager.GenerateHitEffect(npcBehavior);
                    if (newHP < 1) {
                        // no more attacks to do
                        break;
                    }
                }
            }

            usedResources[currCreature].usedAction = true;
        }
        else {
            yield return TryMovePlayer(playerMovement);
        }

        if (initiatives.Count == partyManager.partyMembers.Count) {
            initiatives = null;
            gameStateManager.ExitCombat();
        }
    }

    public void HandleDetachedCancel(InputAction.CallbackContext obj) {
        Traveller currCreature = initiatives[currInitiative].Value;
        if (movementManager.IsMoving(currCreature)) {
            return;
        }

        Debug.Log("Player ended turn.");
        ResetCombatResources(currCreature);
        IncrementInitiative();
        StartCoroutine(RunTurn(currInitiative));
    }

    public PlayerCharacter GetCurrTurnPlayer() {
        return (PlayerCharacter) initiatives[currInitiative].Value;
    }

    private IEnumerator TryMovePlayer(PlayerCharacter playerMovement) {
        int maxSearchLength = (playerMovement.GetStats().baseSpeed / TILE_TO_FEET) * 3;
        CoroutineWithData coroutineWithData = new CoroutineWithData(this,
            pathfinder.FindPath(playerMovement, detachedCamera.currVoxel, maxSearchLength));
        yield return coroutineWithData.coroutine;
        Deque<Vector3Int> path = (Deque<Vector3Int>)coroutineWithData.result;

        int remainingSpeed = playerMovement.GetStats().baseSpeed
            - usedResources[playerMovement].consumedMovement;
        while (path.Count * TILE_TO_FEET > remainingSpeed) {
            path.RemoveFromFront();
        }

        usedResources[playerMovement].consumedMovement += path.Count * TILE_TO_FEET;
        yield return movementManager.MoveAlongPath(playerMovement, path);
    }
    
    public void SetFirstCombatant(NPC firstCombatant) {
        this.firstCombatant = firstCombatant;
    }

    private void SetCombatantsAndInitiativeOrder() {
        initiatives = new List<KeyValuePair<int, Traveller>>();

        // add players
        foreach (PlayerCharacter playerMovement in partyManager.partyMembers) {
            int playerDexModifier = StatModifiers.GetModifierForStat(
                playerMovement.GetStats().dexterity);
            int playerInitiative = randomManager.Roll(1, 20, playerDexModifier);
            initiatives.Add(new KeyValuePair<int, Traveller>(playerInitiative,
                playerMovement));
        }

        // add NPCs
        foreach (NPC npcBehavior in firstCombatant.teammates) {
            npcBehavior.inCombat = true;
            
            int npcDexModifier = StatModifiers.GetModifierForStat(
                npcBehavior.GetStats().dexterity);
            int initiative = randomManager.Roll(1, 20, npcDexModifier);
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

    private void ResetCombatResources(Traveller traveller) {
        usedResources.Remove(traveller);
    }

    private class CombatResources {
        public bool usedAction = false;
        public bool usedBonusAction = false;
        public bool usedReaction = false;
        public bool usedFreeAction = false;
        public int consumedMovement = 0;
    }
}
