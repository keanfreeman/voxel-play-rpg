using InstantiatedEntity;
using Nito.Collections;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class PartyManager : MonoBehaviour
{
    [SerializeField] InputManager inputManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] Pathfinder pathfinder;
    [SerializeField] MovementManager movementManager;
    
    public PlayerMovement mainCharacter { get; private set; }
    public List<PlayerMovement> partyMembers { get; private set; } = new List<PlayerMovement>();
    public PlayerMovement currControlledCharacter { get; private set; }

    public void SetMainCharacter(PlayerMovement playerMovement) {
        mainCharacter = playerMovement;
    }

    public void SetCurrControlledCharacter(PlayerMovement playerMovement) {
        inputManager.SetPlayerMovementControls(currControlledCharacter, playerMovement);
        currControlledCharacter = playerMovement;
    }

    public void SwitchToNextCharacter(InputAction.CallbackContext obj) {
        if (gameStateManager.controlState == ControlState.COMBAT) {
            return;
        }

        int currCharacterPosition = partyMembers.FindIndex(0, 
            (PlayerMovement iter) => iter == currControlledCharacter);
        int nextCharacterPosition = currCharacterPosition == partyMembers.Count - 1 ?
            0 : currCharacterPosition + 1;
        SetCurrControlledCharacter(partyMembers[nextCharacterPosition]);

        if (gameStateManager.controlState != ControlState.DETACHED) {
            cameraManager.AttachCameraToPlayer(currControlledCharacter);
        }
    }

    public PlayerMovement GetPlayerMovement(PlayerCharacter playerCharacter) {
        foreach (PlayerMovement playerMovement in partyMembers) {
            if (playerMovement.playerInfo == playerCharacter) {
                return playerMovement;
            }
        }
        throw new KeyNotFoundException("No PlayerMovement found for PlayerCharacter.");
    }

    // TODO - use actual path to calculate distance
    public PlayerMovement FindNearestPlayer(Vector3Int position) {
        PlayerMovement nearest = partyMembers[0];
        float nearestDistance = float.MaxValue;
        foreach (PlayerMovement playerMovement in partyMembers) {
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
        List<PlayerMovement> orderedPartyList = new List<PlayerMovement>();
        HashSet<PlayerMovement> closenessList = new HashSet<PlayerMovement>(partyMembers);
        closenessList.Remove(currControlledCharacter);

        PlayerMovement followTarget = currControlledCharacter;
        while (closenessList.Count > 0) {
            PlayerMovement closest = FindClosestTo(closenessList, followTarget);
            orderedPartyList.Add(closest);
            closenessList.Remove(closest);
            followTarget = closest;
        }

        // order them to move towards their followTarget
        Vector3Int nextMove = leaderOldPosition;
        foreach (PlayerMovement playerMovement in orderedPartyList) {
            Vector3Int temp = playerMovement.origin;
            playerMovement.MoveOriginToPoint(nextMove);
            nextMove = temp;
        }
    }

    private PlayerMovement FindClosestTo(HashSet<PlayerMovement> candidates, PlayerMovement target) {
        PlayerMovement closest = candidates.GetEnumerator().Current;
        float distance = float.MaxValue;
        foreach (PlayerMovement playerMovement in candidates) {
            int currDistance = Coordinates.NumPointsBetween(playerMovement.origin, target.origin);
            if (currDistance < distance) {
                closest = playerMovement;
                distance = currDistance;
            }
        }

        return closest;
    }
}
