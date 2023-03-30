using GameMechanics;
using InstantiatedEntity;
using Nito.Collections;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    
    NPCBehavior firstCombatant;
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
        usedResources[currCreature] = new CombatResources();
        if (currCreature.GetType() == typeof(PlayerMovement)) {
            inputManager.SwitchPlayerToDetachedControlState(currCreature.currVoxel);
            yield break;
        }

        inputManager.SwitchDetachedToWatchControlState();
        yield return cameraManager.MoveCameraToTargetCreature(currCreature);

        // find nearest enemy
        PlayerMovement nearestPlayer = partyManager.FindNearestPlayer(currCreature.currVoxel);

        // move towards enemy
        NPCBehavior creatureAsNPC = (NPCBehavior) currCreature;
        int remainingSpeed = creatureAsNPC.npcInfo.stats.baseSpeed - usedResources[currCreature].consumedMovement;

        Deque<Vector3Int> path = pathfinder.FindPath(currCreature.currVoxel, nearestPlayer.currVoxel, false);
        while (path.Count * TILE_TO_FEET > remainingSpeed) {
            path.RemoveFromFront();
        }
        yield return movementManager.MoveAlongPath(currCreature, path);

        // attack enemy
        if (Coordinates.IsNextTo(creatureAsNPC.currVoxel, nearestPlayer.currVoxel)) {
            // TODO use less brittle attack selection method
            Attack npcAttack = (Attack)creatureAsNPC.npcInfo.stats.actions[0];
            int attackRoll = randomManager.Roll(npcAttack.attackRoll);
            Debug.Log($"NPC rolled {attackRoll} for their attack roll.");
            if (attackRoll >= nearestPlayer.playerInfo.stats.CalculateArmorClass()) {
                int damageRoll = randomManager.Roll(npcAttack.damageRoll);
                Debug.Log($"NPC rolled {damageRoll} for their damage roll.");
                int newHP = nearestPlayer.currHP - damageRoll;
                nearestPlayer.SetHP(newHP);
                if (newHP < 1) {
                    // TODO - lose on death and no party members
                    Debug.Log("Player ran out of HP.");
                }

                yield return effectManager.GenerateHitEffect(nearestPlayer.currVoxel);
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
        if (currCreature.GetType() != typeof(PlayerMovement) || movementManager.IsMoving(currCreature)
                || currCreature.currVoxel == detachedCamera.currVoxel) {
            yield break;
        }

        PlayerMovement playerMovement = (PlayerMovement)currCreature;

        InstantiatedNVE selectedEntity = nonVoxelWorld.GetNVEFromPosition(detachedCamera.currVoxel);
        if (selectedEntity != null
                && selectedEntity.GetType() == typeof(NPCBehavior)
                && Coordinates.IsNextTo(currCreature.currVoxel, selectedEntity.currVoxel)) {
            if (usedResources[currCreature].usedAction) {
                Debug.Log("Player tried to use action twice.");
                yield break;
            }

            NPCBehavior npcBehavior = (NPCBehavior)selectedEntity;
            // TODO - consider other actions (currently equipped weapon?)
            Attack attack = (Attack)playerMovement.playerInfo.stats.actions[0];
            int attackRoll = randomManager.Roll(attack.attackRoll);
            Debug.Log($"Player rolled {attackRoll} for their attack roll.");
            if (attackRoll >= npcBehavior.npcInfo.stats.CalculateArmorClass()) {
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

                            nonVoxelWorld.ResetPosition(npcBehavior.currVoxel);
                            initiatives.Remove(initiative);
                            Destroy(selectedEntity.gameObject);
                            break;
                        }
                    }
                }

                yield return effectManager.GenerateHitEffect(npcBehavior.currVoxel);
            }

            usedResources[currCreature].usedAction = true;
        }
        else {
            Deque<Vector3Int> path = pathfinder.FindPath(currCreature.currVoxel, detachedCamera.currVoxel, 
                true);

            int remainingSpeed = playerMovement.playerInfo.stats.baseSpeed 
                - usedResources[currCreature].consumedMovement;
            if (path.Count * TILE_TO_FEET > remainingSpeed) {
                Debug.Log($"Tried to move further than remaining speed: {remainingSpeed}");
                yield break;
            }

            usedResources[currCreature].consumedMovement += path.Count * TILE_TO_FEET;
            yield return movementManager.MoveAlongPath(currCreature, path);
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
    
    public void SetFirstCombatant(NPCBehavior firstCombatant) {
        this.firstCombatant = firstCombatant;
    }

    private void SetCombatantsAndInitiativeOrder() {
        initiatives = new List<KeyValuePair<int, Traveller>>();

        // add players
        foreach (PlayerMovement playerMovement in partyManager.partyMembers) {
            int playerDexModifier = StatModifiers.GetModifierForStat(
                playerMovement.playerInfo.stats.dexterity);
            int playerInitiative = randomManager.Roll(1, 20, playerDexModifier);
            initiatives.Add(new KeyValuePair<int, Traveller>(playerInitiative,
                playerMovement));
        }

        // add NPCs
        foreach (NPCBehavior npcBehavior in firstCombatant.teammates) {
            int npcDexModifier = StatModifiers.GetModifierForStat(
                npcBehavior.npcInfo.stats.dexterity);
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
