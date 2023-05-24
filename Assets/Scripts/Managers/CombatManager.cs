using GameMechanics;
using Instantiated;
using Nito.Collections;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] TimerUIController timerUIController;
    [SerializeField] CombatUI combatUI;
    [SerializeField] MessageManager messageManager;

    public event System.Action roundEnded;

    ICollection<NPC> enemies;
    List<KeyValuePair<int, Traveller>> initiatives;
    int currInitiative = -1;
    Dictionary<Traveller, CombatResources> usedResources = new Dictionary<Traveller, CombatResources>();

    private const int TILE_TO_FEET = 5;

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

        usedResources[currCreature] = new CombatResources();
        if (currCreature.GetType() == typeof(PlayerCharacter)) {
            inputManager.SwitchPlayerToDetachedControlState(currCreature.origin);
            combatUI.SetActions((PlayerCharacter)currCreature);
            yield break;
        }

        inputManager.SwitchDetachedToWatchControlState();

        NPC npcInstance = (NPC) currCreature;
        if (!npcInstance.StatusEffects.IsParalyzed()) {
            // find nearest enemy
            PlayerCharacter nearestPlayer = partyManager.FindNearestPlayer(currCreature.origin);
            if (!Coordinates.IsNextTo(npcInstance, nearestPlayer)) {
                // try move towards enemy
                int maxSearchLength = (npcInstance.GetStats().baseSpeed / TILE_TO_FEET) * 3;

                CoroutineWithData coroutineWithData = new CoroutineWithData(this,
                    pathfinder.FindPath(currCreature, nearestPlayer.origin, maxSearchLength));
                yield return coroutineWithData.coroutine;
                Deque<Vector3Int> path = (Deque<Vector3Int>) coroutineWithData.result;
                while (path.Count * TILE_TO_FEET > npcInstance.GetStats().baseSpeed) {
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

        if (playerInstance.StatusEffects.IsParalyzed()) {
            // todo - show an effect and disable UI components
            Debug.Log("Player is paralyzed and can't act.");
            yield break;
        }

        // check if player wanted to attack
        if (selectedEntity != null && selectedEntity.GetType() == typeof(NPC)) {
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

        if (initiatives.Count <= partyManager.partyMembers.Count) {
            yield return ExitCombat();
        }
    }

    public IEnumerator PerformAttack(Traveller attacker, AttackSO attack, Traveller target) {
        if (gameStateManager.controlState == ControlState.COMBAT 
                && usedResources[attacker].usedAction) {
            messageManager.DisplayMessage("You cannot take two actions.");
            yield break;
        }

        CoroutineWithData cwd = new(this, attacker.PerformAttack(attack,
                    target));
        yield return cwd.coroutine;
        AttackResult attackResult = cwd.result as AttackResult;

        if (attackResult.rolled >= target.GetStats().CalculateArmorClass()) {
            CoroutineWithData damageCoroutine = new(this,
                attacker.PerformDamage(attack, attackResult, target));
            yield return damageCoroutine.coroutine;
            int damageRoll = (int)damageCoroutine.result;

            Debug.Log($"Traveller rolled {damageRoll} for their damage roll.");
            yield return attacker.DealDamage(attack,
                new Damage(attack.damageType, damageRoll), target);
            yield return effectManager.GenerateHitEffect(target);

            if (target.CurrHP < 1) {
                if (target.GetType() == typeof(PlayerCharacter)) {
                    // todo - defeat players too
                    Debug.Log("Player ran out of HP.");
                    yield break;
                }
                if (initiatives != null) {
                    foreach (KeyValuePair<int, Traveller> initiative in initiatives) {
                        if (initiative.Value == target) {
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
                nonVoxelWorld.DestroyEntity(target);
            }
        }

        if (gameStateManager.controlState == ControlState.COMBAT) {
            usedResources[attacker].usedAction = true;
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

            CoroutineWithData coroutineWithData = new CoroutineWithData(this,
            pathfinder.FindPath(pc, partyManager.currControlledCharacter.origin));
            yield return coroutineWithData.coroutine;
            Deque<Vector3Int> path = (Deque<Vector3Int>)coroutineWithData.result;

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

    private void IncrementInitiative() {
        if (currInitiative >= initiatives.Count - 1) {
            currInitiative = 0;
            roundEnded?.Invoke();
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
