using Instantiated;
using Nito.Collections;
using EntityDefinition;
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
    
    public Instantiated.PlayerCharacter mainCharacter { get; private set; }
    public List<Instantiated.PlayerCharacter> partyMembers { get; private set; } = new List<Instantiated.PlayerCharacter>();
    public Instantiated.PlayerCharacter currControlledCharacter { get; private set; }

    public void SetMainCharacter(Instantiated.PlayerCharacter playerMovement) {
        mainCharacter = playerMovement;
    }

    public void SetCurrControlledCharacter(Instantiated.PlayerCharacter playerMovement) {
        inputManager.SetPlayerMovementControls(currControlledCharacter, playerMovement);
        currControlledCharacter = playerMovement;
    }

    public void SwitchToNextCharacter(InputAction.CallbackContext obj) {
        if (gameStateManager.controlState == ControlState.COMBAT) {
            return;
        }

        int currCharacterPosition = partyMembers.FindIndex(0, 
            (Instantiated.PlayerCharacter iter) => iter == currControlledCharacter);
        int nextCharacterPosition = currCharacterPosition == partyMembers.Count - 1 ?
            0 : currCharacterPosition + 1;
        SetCurrControlledCharacter(partyMembers[nextCharacterPosition]);

        if (gameStateManager.controlState != ControlState.DETACHED) {
            cameraManager.AttachCameraToPlayer(currControlledCharacter);
        }
    }

    public Instantiated.PlayerCharacter GetPlayerMovement(EntityDefinition.PlayerCharacter playerCharacter) {
        foreach (Instantiated.PlayerCharacter playerMovement in partyMembers) {
            if (playerMovement.playerInfo == playerCharacter) {
                return playerMovement;
            }
        }
        throw new KeyNotFoundException("No PlayerMovement found for PlayerCharacter.");
    }

    // TODO - use actual path to calculate distance
    public Instantiated.PlayerCharacter FindNearestPlayer(Vector3Int position) {
        Instantiated.PlayerCharacter nearest = partyMembers[0];
        float nearestDistance = float.MaxValue;
        foreach (Instantiated.PlayerCharacter playerMovement in partyMembers) {
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
        List<Instantiated.PlayerCharacter> orderedPartyList = new List<Instantiated.PlayerCharacter>();
        HashSet<Instantiated.PlayerCharacter> closenessList = new HashSet<Instantiated.PlayerCharacter>(partyMembers);
        closenessList.Remove(currControlledCharacter);

        Instantiated.PlayerCharacter followTarget = currControlledCharacter;
        while (closenessList.Count > 0) {
            Instantiated.PlayerCharacter closest = FindClosestTo(closenessList, followTarget);
            orderedPartyList.Add(closest);
            closenessList.Remove(closest);
            followTarget = closest;
        }

        // order them to move towards their followTarget
        Vector3Int nextMove = leaderOldPosition;
        foreach (Instantiated.PlayerCharacter playerMovement in orderedPartyList) {
            Vector3Int temp = playerMovement.origin;
            playerMovement.MoveOriginToPoint(nextMove);
            nextMove = temp;
        }
    }

    private Instantiated.PlayerCharacter FindClosestTo(HashSet<Instantiated.PlayerCharacter> candidates, Instantiated.PlayerCharacter target) {
        Instantiated.PlayerCharacter closest = candidates.GetEnumerator().Current;
        float distance = float.MaxValue;
        foreach (Instantiated.PlayerCharacter playerMovement in candidates) {
            int currDistance = Coordinates.NumPointsBetween(playerMovement.origin, target.origin);
            if (currDistance < distance) {
                closest = playerMovement;
                distance = currDistance;
            }
        }

        return closest;
    }
}
