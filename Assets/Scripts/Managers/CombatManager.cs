using Combat;
using GameMechanics;
using Instantiated;
using Nito.Collections;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static RandomManager;
using static UnityEngine.GraphicsBuffer;

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
    [SerializeField] TimerUIController timerUIController;
    [SerializeField] CombatUI combatUI;
    [SerializeField] MessageManager messageManager;

    public event System.Action roundEnded;
    public CombatResources CombatResources { get; private set; } = new();
    public List<KeyValuePair<int, Traveller>> initiatives { get; private set; }

    ICollection<NPC> enemies;
    int currInitiative = -1;

    public static int TILE_TO_FEET = 5;

    public bool IsInCombat() {
        return initiatives != null;
    }

    public void StartCombat() {
        if (initiatives == null) {
            timerUIController.PauseTimer();
            SetCombatantsAndInitiativeOrder();
            StartCoroutine(RunTurn(currInitiative));
        }
    }

    public IEnumerator RunTurn(int initiative) {
        Traveller currCreature = initiatives[initiative].Value;
        yield return cameraManager.MoveCameraToTargetCreature(currCreature);

        yield return currCreature.OnCombatTurnStart();

        CombatResources.AddTraveller(currCreature);
        if (currCreature.GetType() == typeof(PlayerCharacter)) {
            inputManager.SwitchPlayerToDetachedControlState(currCreature.origin);
            combatUI.SetActions((PlayerCharacter)currCreature);
            yield break;
        }

        inputManager.SwitchDetachedToWatchControlState();

        NPC npcInstance = (NPC) currCreature;
        // todo - use events for this rather than checking individual statuses
        if (!npcInstance.HasCondition(Condition.Paralyzed) 
                && !npcInstance.HasCondition(Condition.Unconscious)) {
            PlayerCharacter nearestPlayer = partyManager.FindNearestPlayer(currCreature.origin);
            if (!Coordinates.IsNextTo(npcInstance, nearestPlayer)) {
                int maxSearchLength = (npcInstance.GetStats().baseSpeed / TILE_TO_FEET) * 5;

                CoroutineWithData<Deque<Vector3Int>> coroutineWithData = new(this,
                    pathfinder.FindPath(currCreature, nearestPlayer.origin, maxSearchLength));
                yield return coroutineWithData.coroutine;
                Deque<Vector3Int> path = coroutineWithData.GetResult();

                // todo - use events instead of conditional
                int movementBudget = npcInstance.GetStats().baseSpeed;
                if (npcInstance.GetStatus(StatusEffect.Longstrider) != null) {
                    movementBudget += 10;
                }

                while (path.Count * TILE_TO_FEET > movementBudget) {
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

                yield return PerformAttack(npcInstance, npcAttack, nearestPlayer);
            }
        }

        yield return currCreature.OnCombatTurnEnd();

        inputManager.DisableWatchState();
        CombatResources.ResetCombatResources(currCreature);
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

        // todo - use events for this rather than checking individual statuses
        Condition? preventActionCondition = playerInstance.HasCondition(Condition.Paralyzed)
            ? Condition.Paralyzed : playerInstance.HasCondition(Condition.Unconscious) 
            ? Condition.Unconscious 
            : null;
        if (preventActionCondition.HasValue) {
            // todo - show an effect and disable UI components. also, explain what status is preventing action
            messageManager.DisplayMessage($"Player is {preventActionCondition.Value} and cannot act.");
            yield break;
        }

        // check if player wanted to attack
        if (selectedEntity != null && selectedEntity.GetType() == typeof(NPC)) {
            NPC attackTarget = (NPC)selectedEntity;

            ActionSO validAttackAction;
            if (!Coordinates.IsNextTo(currCreature, attackTarget)) {
                validAttackAction = StatInfo.GetRangedAction(playerInstance.GetStats());
                Tuple<Vector3Int, Vector3Int> closestPoints = playerInstance.GetNearestPoints(attackTarget);
                int distance = Coordinates.NumPointsBetween(closestPoints.Item1, closestPoints.Item2);
                bool isOutOfRange = (((AttackSO)validAttackAction).longRange / TILE_TO_FEET) < distance;
                if (validAttackAction == null || isOutOfRange) {
                    // try moving towards the enemy, then attacking
                    yield return TryMovePlayer(playerInstance);
                    if (!Coordinates.IsNextTo(currCreature, attackTarget)) {
                        messageManager.DisplayMessage("Couldn't get close enough to make a melee attack.");
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
                messageManager.DisplayMessage("You have no basic attack action. Try using the combat bar.");
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
                yield return PerformAttack(playerInstance, attack, attackTarget);
            }
        }
        else {
            yield return TryMovePlayer(playerInstance);
        }

        if (initiatives == null || initiatives.Count <= partyManager.partyMembers.Count) {
            yield return ExitCombat();
        }
    }

    public IEnumerator PerformAttack(Traveller attacker, AttackSO attack, Traveller target) {
        if (gameStateManager.controlState == ControlState.COMBAT 
                && !CombatResources.HasResource(attacker, ActionType.Action)) {
            messageManager.DisplayMessage("You cannot take two actions.");
            yield break;
        }

        CoroutineWithData<AttackResult> cwd = new(this, attacker.PerformAttack(attack, target));
        yield return cwd.coroutine;
        AttackResult attackResult = cwd.GetResult();

        if (attackResult.rolled >= target.GetStats().CalculateArmorClass(target.GetStatusEffects())) {
            CoroutineWithData<int> damageCoroutine = new(this,
                attacker.PerformDamage(attack, attackResult, target));
            yield return damageCoroutine.coroutine;
            int damageRoll = damageCoroutine.GetResult();

            Debug.Log($"Traveller rolled {damageRoll} for their damage roll.");
            yield return attacker.DealDamage(attack,
                new Damage(attack.damageType, damageRoll), target);
        }

        if (gameStateManager.controlState == ControlState.COMBAT) {
            CombatResources.ConsumeResource(attacker, ActionType.Action);
            if (initiatives.Count <= partyManager.partyMembers.Count) {
                yield return ExitCombat();
            }
        }
    }

    private IEnumerator ExitCombat() {
        initiatives = null;
        inputManager.LockPlayerControls();

        // move party next to leader
        foreach (PlayerCharacter pc in partyManager.partyMembers) {
            if (pc == partyManager.currControlledCharacter) {
                continue;
            }

            CoroutineWithData<Deque<Vector3Int>> coroutineWithData = new(this,
            pathfinder.FindPath(pc, partyManager.currControlledCharacter.origin));
            yield return coroutineWithData.coroutine;
            Deque<Vector3Int> path = coroutineWithData.GetResult();

            yield return movementManager.MoveAlongPath(pc, path);
        }

        yield return gameStateManager.ExitCombat();
        timerUIController.StartTimer();
    }

    public void HandleDetachedCancel(InputAction.CallbackContext obj) {
        StartCoroutine(ExecuteHandleDetachedCancel());
    }

    public IEnumerator ExecuteHandleDetachedCancel() {
        Traveller currCreature = initiatives[currInitiative].Value;
        if (movementManager.IsMoving(currCreature)) {
            yield break;
        }

        yield return currCreature.OnCombatTurnEnd();

        Debug.Log("Player ended turn.");
        CombatResources.ResetCombatResources(currCreature);
        IncrementInitiative();
        StartCoroutine(RunTurn(currInitiative));
    }

    public PlayerCharacter GetCurrTurnPlayer() {
        return (PlayerCharacter) initiatives[currInitiative].Value;
    }

    private IEnumerator TryMovePlayer(PlayerCharacter playerMovement) {
        int maxSearchLength = (playerMovement.GetStats().baseSpeed / TILE_TO_FEET) * 5;
        CoroutineWithData<Deque<Vector3Int>> coroutineWithData = new(this,
            pathfinder.FindPath(playerMovement, detachedCamera.currVoxel, maxSearchLength));
        yield return coroutineWithData.coroutine;
        Deque<Vector3Int> path = coroutineWithData.GetResult();

        // todo - use events instead of conditional
        int movementBudget = playerMovement.GetStats().baseSpeed;
        if (playerMovement.GetStatus(StatusEffect.Longstrider) != null) {
            movementBudget += 10;
        }
        movementBudget -= CombatResources.GetConsumedMovement(playerMovement);
        while (path.Count * TILE_TO_FEET > movementBudget) {
            path.RemoveFromFront();
        }

        CombatResources.AddConsumedMovement(playerMovement, path.Count * TILE_TO_FEET);
        yield return movementManager.MoveAlongPath(playerMovement, path);
    }
    
    public void SetEnemies(ICollection<NPC> enemies) {
        this.enemies = enemies;
    }

    private void SetCombatantsAndInitiativeOrder() {
        messageManager.DisplayMessage("Rolling initiative!");

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
        foreach (NPC npcBehavior in enemies) {
            npcBehavior.inCombat = true;
            
            int npcDexModifier = StatModifiers.GetModifierForStat(
                npcBehavior.GetStats().dexterity);
            int initiative = randomManager.RollAbilityCheck(npcDexModifier);
            initiatives.Add(new KeyValuePair<int, Traveller>(initiative, npcBehavior));
        }

        initiatives.Sort((x, y) => -x.Key.CompareTo(y.Key));
        currInitiative = 0;
    }

    public IEnumerator DestroyCombatant(Traveller traveller) {
        if (initiatives != null) {
            foreach (KeyValuePair<int, Traveller> initiative in initiatives) {
                if (initiative.Value == traveller) {
                    // ensure initiative number is not incorrect after deleting item
                    int currCreatureInitiative = initiatives[currInitiative].Key;
                    if (initiative.Key > currCreatureInitiative) {
                        currInitiative -= 1;
                    }
                    initiatives.Remove(initiative);
                    break;
                }
            }
        }
        nonVoxelWorld.DestroyEntity(traveller);
        yield break;
    }

    private void IncrementInitiative() {
        if (currInitiative >= initiatives.Count - 1) {
            currInitiative = 0;
            roundEnded?.Invoke();
        }
        else {
            currInitiative += 1;
        }
    }
}

namespace Combat {
    public class CombatResources {
        private Dictionary<Traveller, CombatResourceState> UsedResources = new();

        public void AddTraveller(Traveller traveller) {
            UsedResources[traveller] = new();
        }

        public int GetConsumedMovement(Traveller traveller) {
            return UsedResources[traveller].ConsumedMovement;
        }

        public void AddConsumedMovement(Traveller traveller, int movement) {
            UsedResources[traveller].ConsumedMovement += movement;
        }

        public bool HasResource(Traveller traveller, ActionType actionType) {
            return UsedResources[traveller].IsActionTypeAvailable[actionType];
        }

        public void ConsumeResource(Traveller traveller, ActionType actionType) {
            UsedResources[traveller].IsActionTypeAvailable[actionType] = false;
        }

        public void ResetCombatResources(Traveller traveller) {
            UsedResources[traveller].ResetCombatResources();
        }
    }

    public class CombatResourceState {
        public Dictionary<ActionType, bool> IsActionTypeAvailable { get; private set; } = new() {
            { ActionType.Action, true },
            { ActionType.BonusAction, true },
            { ActionType.Reaction, true },
            { ActionType.FreeAction, true }
        };
        public int ConsumedMovement = 0;

        public void ResetCombatResources() {
            // must use tolist to avoid modifying the actual dictionary
            foreach (ActionType actionType in IsActionTypeAvailable.Keys.ToList()) {
                IsActionTypeAvailable[actionType] = true;
            }
        }
    }
}
