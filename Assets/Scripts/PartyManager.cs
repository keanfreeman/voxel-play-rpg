using Instantiated;
using Nito.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Saving;

public class PartyManager : MonoBehaviour
{
    [SerializeField] InputManager inputManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] Pathfinder pathfinder;
    [SerializeField] MovementManager movementManager;
    
    public PlayerCharacter mainCharacter { get; private set; }
    public List<PlayerCharacter> partyMembers { get; private set; } = new List<PlayerCharacter>();
    public PlayerCharacter currControlledCharacter { get; private set; }

    public void ClearData() {
        mainCharacter = null;
        partyMembers = new List<PlayerCharacter>();
        currControlledCharacter = null;
    }

    public void SetMainCharacter(PlayerCharacter playerMovement) {
        mainCharacter = playerMovement;
    }

    public void SetCurrControlledCharacter(PlayerCharacter playerMovement) {
        inputManager.SetPlayerMovementControls(currControlledCharacter, playerMovement);
        currControlledCharacter = playerMovement;
    }

    public void SwitchToNextCharacter(InputAction.CallbackContext obj) {
        if (gameStateManager.controlState == ControlState.COMBAT) {
            return;
        }

        int currCharacterPosition = partyMembers.FindIndex(0, 
            (PlayerCharacter iter) => iter == currControlledCharacter);
        int nextCharacterPosition = currCharacterPosition == partyMembers.Count - 1 ?
            0 : currCharacterPosition + 1;
        SetCurrControlledCharacter(partyMembers[nextCharacterPosition]);

        if (gameStateManager.controlState != ControlState.DETACHED) {
            cameraManager.AttachCameraToPlayer(currControlledCharacter);
        }
    }

    public Vector3Int GetPositionFromDestination(EntityDefinition.EnvChangeDestination destination,
            PlayerCharacter playerCharacter) {
        return destination.destinationTile + Vector3Int.right * GetPlayerIndex(playerCharacter);
    }

    private int GetPlayerIndex(PlayerCharacter playerCharacter) {
        for (int i = 0; i < partyMembers.Count; i++) {
            if (partyMembers[i] == playerCharacter) {
                return i;
            }
        }
        throw new KeyNotFoundException("No PlayerMovement found for PlayerCharacter.");
    }

    public PlayerCharacter GetPlayerInstance(EntityDefinition.PlayerCharacter playerCharacter) {
        foreach (PlayerCharacter playerMovement in partyMembers) {
            if (playerMovement.GetEntity() == playerCharacter) {
                return playerMovement;
            }
        }
        throw new KeyNotFoundException("No PlayerMovement found for PlayerCharacter.");
    }

    // TODO - use actual path to calculate distance
    public PlayerCharacter FindNearestPlayer(Vector3Int position) {
        PlayerCharacter nearest = partyMembers[0];
        float nearestDistance = float.MaxValue;
        foreach (PlayerCharacter playerMovement in partyMembers) {
            foreach (Vector3Int playerPosition in playerMovement.occupiedPositions) {
                float directDistance = (position - playerPosition).magnitude;
                if (directDistance < nearestDistance) {
                    nearest = playerMovement;
                    nearestDistance = directDistance;
                }
            }
        }
        return nearest;
    }

    // TODO - handle cases where a party member is not directly next to the leader
    // TODO - fix for large or bigger party members (size-agnostic)
    public void OnLeaderMoved(Vector3Int leaderOldPosition) {
        if (gameStateManager.controlState == ControlState.COMBAT) {
            return;
        }

        // figure out who's following who
        List<PlayerCharacter> orderedPartyList = new List<PlayerCharacter>();
        HashSet<PlayerCharacter> closenessList = new HashSet<PlayerCharacter>(partyMembers);
        closenessList.Remove(currControlledCharacter);

        PlayerCharacter followTarget = currControlledCharacter;
        while (closenessList.Count > 0) {
            PlayerCharacter closest = FindClosestTo(closenessList, followTarget);
            orderedPartyList.Add(closest);
            closenessList.Remove(closest);
            followTarget = closest;
        }

        // order them to move towards their followTarget
        Vector3Int nextMove = leaderOldPosition;
        foreach (PlayerCharacter playerMovement in orderedPartyList) {
            Vector3Int temp = playerMovement.origin;
            playerMovement.MoveOriginToPoint(nextMove);
            nextMove = temp;
        }
    }

    private PlayerCharacter FindClosestTo(HashSet<PlayerCharacter> candidates, PlayerCharacter target) {
        PlayerCharacter closest = candidates.GetEnumerator().Current;
        float distance = float.MaxValue;
        foreach (PlayerCharacter playerMovement in candidates) {
            int currDistance = Coordinates.NumPointsBetween(playerMovement.origin, target.origin);
            if (currDistance < distance) {
                closest = playerMovement;
                distance = currDistance;
            }
        }

        return closest;
    }
}
