using MovementDirection;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public class SpriteMovement : MonoBehaviour
{
    [SerializeField] private VoxelWorldManager voxelWorldManager;
    [SerializeField] private NonVoxelWorld nonVoxelWorld;

    // can the player walk on this voxel?
    public bool IsWalkablePosition(Vector3Int position) {
        Voxel voxel = voxelWorldManager.environment.GetVoxel(position);
        return !voxel.isEmpty && !voxel.hasWater;
    }

    // can the player walk through this voxel?
    public bool IsTraversiblePosition(Vector3Int position) {
        Voxel voxel = voxelWorldManager.environment.GetVoxel(position);
        return (voxel.isEmpty || voxel.hasWater) && !nonVoxelWorld.IsPositionOccupied(position);
    }

    public bool IsReachablePosition(Vector3Int position) {
        Vector3Int belowRequestedCoordinate = position + Vector3Int.down;
        return IsTraversiblePosition(position) && IsWalkablePosition(belowRequestedCoordinate);
    }

    // A must be 1 voxel from B
    public bool IsATraversibleFromB(Vector3Int requestedCoordinate, Vector3Int startCoordinate) {
        Vector3Int distance = requestedCoordinate - startCoordinate;
        Vector3Int belowRequestedCoordinate = requestedCoordinate + Vector3Int.down;
        if (!IsTraversiblePosition(requestedCoordinate) || !IsWalkablePosition(belowRequestedCoordinate)) {
            return false;
        }

        if (distance.y == 1) {
            Vector3Int abovePlayerCoordinate = startCoordinate + Vector3Int.up;
            return IsTraversiblePosition(abovePlayerCoordinate);
        }
        else if (distance.y == -1) {
            Vector3Int aboveRequestedCoordinate = requestedCoordinate + Vector3Int.up;
            return IsTraversiblePosition(aboveRequestedCoordinate);
        }
        return true;
    }

    // returns the direction to move after accounting for slopes, other terrain.
    // disallows movement if there are obstacles.
    // the player is always technically above slopes when traversing them.
    public Vector3Int? GetTerrainAdjustedCoordinate(
            Vector3Int requestedCoordinate, Vector3Int currCoordinate) {
        VoxelPlayEnvironment environment = voxelWorldManager.environment;

        Voxel requestedVoxel = environment.GetVoxel(requestedCoordinate);
        if (IsTraversiblePosition(requestedCoordinate)) {
            // check if player can move onto land
            Vector3Int belowRequestedCoordinate = requestedCoordinate + Vector3Int.down;
            if (IsWalkablePosition(belowRequestedCoordinate)) {
                return requestedCoordinate;
            }

            // check if player can move down slope to solid tile
            Voxel underfootVoxel = environment.GetVoxel(currCoordinate + Vector3Int.down);
            if (IsSlope(underfootVoxel)
                && IsSlopeDownRelativeToSprite(requestedCoordinate, currCoordinate,
                    underfootVoxel.GetTextureRotation())
                && IsWalkablePosition(belowRequestedCoordinate + Vector3Int.down)) {
                return requestedCoordinate + Vector3Int.down;
            }

            // check if the player can jump down 1 tile
            if (IsTraversiblePosition(belowRequestedCoordinate)
                    && IsWalkablePosition(belowRequestedCoordinate + Vector3Int.down)) {
                return requestedCoordinate + Vector3Int.down;
            }
        }

        // check if player can move up slope
        if (IsSlope(requestedVoxel) && IsSlopeUpRelativeToSprite(requestedCoordinate, currCoordinate,
                requestedVoxel.GetTextureRotation())) {
            return requestedCoordinate + Vector3Int.up;
        }

        // check if the player can jump up 1 tile
        Vector3Int aboveRequestedCoordinate = requestedCoordinate + Vector3Int.up;
        if (IsWalkablePosition(requestedCoordinate) && IsTraversiblePosition(aboveRequestedCoordinate)) {
            return requestedCoordinate + Vector3Int.up;
        }

        return null;
    }

    public bool IsSlope(Voxel voxel) {
        VoxelPlayEnvironment environment = voxelWorldManager.environment;

        VoxelDefinition slopeVoxel = null;
        foreach (VoxelDefinition vd in environment.voxelDefinitions) {
            if (vd != null && vd.name == "SlopeVoxel") {
                slopeVoxel = vd;
            }
        }
        if (slopeVoxel == null) {
            // TODO change back
            return false;
            // throw new System.SystemException("No expected voxel in world.");
        }
        int slopeRotation = voxel.GetTextureRotation();

        return voxel.type == slopeVoxel;
    }

    public bool IsSlopeUpRelativeToSprite(Vector3Int requestedCoordinate,
            Vector3Int currCoordinate, int slopeRotation) {
        Vector3Int diff = requestedCoordinate - currCoordinate;
        return slopeRotation == 0 && diff.z >= 1
            || slopeRotation == 1 && diff.x >= 1
            || slopeRotation == 2 && diff.z <= -1
            || slopeRotation == 3 && diff.x <= -1;
    }

    // assumes they're in the same x or z row
    public bool IsSlopeDownRelativeToSprite(Vector3Int requestedCoordinate,
            Vector3Int currCoordinate, int slopeRotation) {
        Vector3Int diff = requestedCoordinate - currCoordinate;
        return slopeRotation == 2 && diff.z >= 1
            || slopeRotation == 3 && diff.x >= 1
            || slopeRotation == 0 && diff.z <= -1
            || slopeRotation == 1 && diff.x <= -1;
    }

    public Vector3Int GetSpriteDesiredCoordinate(Vector3Int currPosition,
            SpriteMoveDirection moveDirection) {
        if (moveDirection == SpriteMoveDirection.FORWARD) {
            return currPosition + Vector3Int.forward;
        }
        else if (moveDirection == SpriteMoveDirection.RIGHT) {
            return currPosition + Vector3Int.right;
        }
        else if (moveDirection == SpriteMoveDirection.BACK) {
            return currPosition + Vector3Int.back;
        }
        else if (moveDirection == SpriteMoveDirection.LEFT) {
            return currPosition + Vector3Int.left;
        }
        throw new System.ArgumentException("Impossible direction provided.");
    }
}
