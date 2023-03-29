using InstantiatedEntity;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] InputManager inputManager;
    
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
            float directDistance = (position - playerMovement.currVoxel).magnitude;
            if (directDistance < nearestDistance) {
                nearest = playerMovement;
                nearestDistance = directDistance;
            }
        }
        return nearest;
    }
}
