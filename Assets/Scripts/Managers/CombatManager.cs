using GameMechanics;
using Instantiated;
using Nito.Collections;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static RandomManager;

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

    public bool isInCombat() {
        return initiatives != null;
    }

    public void StartCombat() {
        if (initiatives == null) {
            SetCombatantsAndInitiativeOrder();
            StartCoroutine(RunTurn(currInitiative));
        }
    }

    public IEnumerator RunTurn(int initiative) {
        Traveller currCreature = initiatives[initiative].Value;
        yield return cameraManager.MoveCameraToTargetCreature(currCreature);

        currCreature.OnCombatTurnStart();

        usedResources[currCreature] = new CombatResources();
        if (currCreature.GetType() == typeof(PlayerCharacter)) {
            inputManager.SwitchPlayerToDetachedControlState(currCreature.origin);
            yield break;
        }

        inputManager.SwitchDetachedToWatchControlState();

        NPC npcInstance = (NPC) currCreature;
        if (!npcInstance.statusEffects.IsParalyzed()) {
            // find nearest enemy
            PlayerCharacter nearestPlayer = partyManager.FindNearestPlayer(currCreature.origin);
            if (!Coordinates.IsNextTo(npcInstance, nearestPlayer)) {
                // try move towards enemy
                int maxSearchLength = (npcInstance.GetStats().baseSpeed / TILE_TO_FEET) * 3;

                CoroutineWithData coroutineWithData = new CoroutineWithData(this,
                    pathfinder.FindPath(currCreature, nearestPlayer.origin, maxSearchLength));
                yield return coroutineWithData.coroutine;
                Deque<Vector3Int> path = (Deque<Vector3Int>) coroutineWithData.result;
                while (path.Count * TILE_TO_FEET > npcInstance.GetStats().baseSpeed * TILE_TO_FEET) {
                    path.RemoveFromFront();
                }
                yield return movementManager.MoveAlongPath(currCreature, path);
            }

            // attack enemy
            if (Coordinates.IsNextTo(npcInstance, nearestPlayer)) {
                List<ActionSO> npcActions = npcInstance.GetStats().actions;
                int pickedIndex = randomManager.rng.Next(0, npcActions.Count);
                // TODO - use less brittle attack selection method (allow for non-attacks)
                // TODO - have preferred strategies (ranged vs melee for example)
                AttackSO npcAttack = (AttackSO)npcActions[pickedIndex];
                CoroutineWithData attackRollCoroutine = new(this, 
                    npcInstance.PerformAttack(npcAttack.attackRoll.modifier, nearestPlayer));
                yield return attackRollCoroutine.coroutine;
                AttackResult attackResult = attackRollCoroutine.result as AttackResult;

                if (attackResult.rolled >= nearestPlayer.GetStats().CalculateArmorClass()) {
                    CoroutineWithData damageCoroutine = new(this,
                        npcInstance.PerformDamage(npcAttack, attackResult, nearestPlayer));
                    yield return damageCoroutine.coroutine;
                    int damageRoll = (int)damageCoroutine.result;

                    Debug.Log($"NPC rolled {damageRoll} for their damage roll.");
                    nearestPlayer.TakeDamage(new Damage(npcAttack.damageType, damageRoll));
                    if (nearestPlayer.currHP < 1) {
                        // TODO - lose on death and no party members
                        Debug.Log("Player ran out of HP.");
                    }

                    yield return effectManager.GenerateHitEffect(nearestPlayer);
                }
            }
        }

        currCreature.OnCombatTurnEnd();

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

        PlayerCharacter playerInstance = (PlayerCharacter)currCreature;

        if (playerInstance.statusEffects.IsParalyzed()) {
            // todo - show an effect and disable UI components
            Debug.Log("Player is paralyzed and can't act.");
            yield break;
        }

        // check if player wanted to attack
        if (selectedEntity != null && selectedEntity.GetType() == typeof(NPC)) {
            if (usedResources[currCreature].usedAction) {
                Debug.Log("Player tried to use action twice.");
                yield break;
            }
            NPC attackTarget = (NPC)selectedEntity;

            ActionSO validAttackAction;
            if (!Coordinates.IsNextTo(currCreature, attackTarget)) {
                validAttackAction = StatInfo.GetRangedAction(playerInstance.GetStats());
                if (validAttackAction == null) {
                    // try moving towards the enemy, then attacking
                    yield return TryMovePlayer(playerInstance);
                    if (!Coordinates.IsNextTo(currCreature, attackTarget)) {
                        // couldn't move close enough
                        Debug.Log("Couldn't get close enough to make a melee attack.");
                        yield break;
                    }
                    else {
                        validAttackAction = StatInfo.GetMeleeActionThenRanged(playerInstance.GetStats());
                    }
                }
            }
            else {
                validAttackAction = StatInfo.GetMeleeActionThenRanged(playerInstance.GetStats());
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
                CoroutineWithData cwd = new(this, playerInstance.PerformAttack(attack.attackRoll.modifier,
                    attackTarget));
                yield return cwd.coroutine;
                AttackResult attackResult = cwd.result as AttackResult;

                if (attackResult.rolled >= attackTarget.GetStats().CalculateArmorClass()) {
                    CoroutineWithData damageCoroutine = new(this,
                        playerInstance.PerformDamage(attack, attackResult, attackTarget));
                    yield return damageCoroutine.coroutine;
                    int damageRoll = (int)damageCoroutine.result;

                    Debug.Log($"Player rolled {damageRoll} for their damage roll.");
                    int newHP = attackTarget.currHP - damageRoll;
                    attackTarget.TakeDamage(new Damage(attack.damageType, damageRoll));
                    if (attackTarget.currHP < 1) {
                        Debug.Log("NPC defeated");
                        foreach (KeyValuePair<int, Traveller> initiative in initiatives) {
                            if (initiative.Value == selectedEntity) {
                                // ensure initiative number is not incorrect after deleting item
                                int currCreatureInitiative = initiatives[currInitiative].Key;
                                if (initiative.Key > currCreatureInitiative) {
                                    currInitiative -= 1;
                                }

                                nonVoxelWorld.DestroyEntity(attackTarget);
                                initiatives.Remove(initiative);
                                break;
                            }
                        }
                    }

                    yield return effectManager.GenerateHitEffect(attackTarget);
                    if (newHP < 1) {
                        // no more attacks to do
                        break;
                    }
                }
            }

            usedResources[currCreature].usedAction = true;
        }
        else {
            yield return TryMovePlayer(playerInstance);
        }

        if (initiatives.Count == partyManager.partyMembers.Count) {
            initiatives = null;
            yield return ExitCombat();
        }
    }

    private IEnumerator ExitCombat() {
        inputManager.LockPlayerControls();

        // move party next to leader
        foreach (PlayerCharacter pc in partyManager.partyMembers) {
            if (pc == partyManager.currControlledCharacter) {
                continue;
            }

            CoroutineWithData coroutineWithData = new CoroutineWithData(this,
            pathfinder.FindPath(pc, partyManager.currControlledCharacter.origin));
            yield return coroutineWithData.coroutine;
            Deque<Vector3Int> path = (Deque<Vector3Int>)coroutineWithData.result;

            yield return movementManager.MoveAlongPath(pc, path);
        }

        yield return gameStateManager.ExitCombat();
    }

    public void HandleDetachedCancel(InputAction.CallbackContext obj) {
        Traveller currCreature = initiatives[currInitiative].Value;
        if (movementManager.IsMoving(currCreature)) {
            return;
        }

        currCreature.OnCombatTurnEnd();

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
            int playerInitiative = randomManager.RollAbilityCheck(playerDexModifier);
            initiatives.Add(new KeyValuePair<int, Traveller>(playerInitiative,
                playerMovement));
        }

        // add NPCs
        foreach (NPC npcBehavior in firstCombatant.teammates) {
            npcBehavior.inCombat = true;
            
            int npcDexModifier = StatModifiers.GetModifierForStat(
                npcBehavior.GetStats().dexterity);
            int initiative = randomManager.RollAbilityCheck(npcDexModifier);
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
