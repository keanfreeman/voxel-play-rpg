using InstantiatedEntity;
using Nito.Collections;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatManager : MonoBehaviour
{
    [SerializeField] PartyManager partyManager;
    [SerializeField] RandomManager randomManager;
    [SerializeField] MovementManager movementManager;
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] InputManager inputManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    
    Pathfinder pathfinder;
    NPCBehavior firstCombatant;
    GameObject playerCombatants;
    List<KeyValuePair<int, Traveller>> initiatives;
    int currInitiative = -1;
    int remainingSpeed = -1;

    private const int TILE_TO_FEET = 5;

    private void Awake() {
        pathfinder = new Pathfinder(spriteMovement);
    }

    public void StartCombat() {
        if (initiatives == null) {
            SetCombatantsAndInitiativeOrder();
            StartCoroutine(RunTurn(currInitiative));
        }
    }

    public IEnumerator RunTurn(int initiative) {
        Traveller currCreature = initiatives[initiative].Value;
        if (currCreature.GetType() == typeof(PlayerMovement)) {
            remainingSpeed = partyManager.playerCharacter.stats.baseSpeed;
            inputManager.SwitchPlayerToDetachedControlState();
            yield break;
        }

        NPCBehavior creatureAsNPC = (NPCBehavior) currCreature;
        remainingSpeed = creatureAsNPC.npcInfo.stats.speed;
        inputManager.SwitchDetachedToWatchControlState();
        Deque<Vector3Int> path = pathfinder.FindPath(currCreature.currVoxel, playerMovement.currVoxel, false);
        while (path.Count * TILE_TO_FEET > remainingSpeed) {
            path.RemoveFromFront();
        }
        yield return movementManager.MoveAlongPath(currCreature, path);

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
        if (currCreature.GetType() != typeof(PlayerMovement) || movementManager.IsMoving(currCreature)) {
            yield break;
        }

        

        Deque<Vector3Int> path = pathfinder.FindPath(currCreature.currVoxel, 
            detachedCamera.currVoxel, true);
        if (path.Count * TILE_TO_FEET > remainingSpeed) {
            Debug.Log($"Tried to move further than remaining speed: {remainingSpeed}");
            yield break;
        }
        remainingSpeed -= path.Count * TILE_TO_FEET;
        yield return movementManager.MoveAlongPath(currCreature, path);

        if (remainingSpeed <= 0) {
            IncrementInitiative();
            StartCoroutine(RunTurn(currInitiative));
        }
    }

    public void HandleDetachedCancel(InputAction.CallbackContext obj) {
        if (movementManager.IsMoving(playerMovement)) {
            return;
        }

        Debug.Log("Player ended turn.");
        IncrementInitiative();
        StartCoroutine(RunTurn(currInitiative));
    }
    
    public void SetFirstCombatant(NPCBehavior firstCombatant) {
        this.firstCombatant = firstCombatant;
    }

    private void SetCombatantsAndInitiativeOrder() {
        initiatives = new List<KeyValuePair<int, Traveller>>();

        int playerDexModifier = StatModifiers.GetModifierForStat(
            partyManager.playerCharacter.stats.dexterity);
        int playerInitiative = randomManager.Roll(1, 20, playerDexModifier);
        initiatives.Add(new KeyValuePair<int, Traveller>(playerInitiative,
            playerMovement));

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
}
