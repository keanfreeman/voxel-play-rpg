using Instantiated;
using MovementDirection;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelPlay;

public class SpriteMovement : MonoBehaviour
{
    [SerializeField] private VoxelWorldManager voxelWorldManager;
    [SerializeField] private NonVoxelWorld nonVoxelWorld;

    private const float FRACTION_WALKABLE_MINIMUM = 0.5f;

    // can the player walk on this voxel?
    public bool IsWalkablePosition(Vector3Int position) {
        Voxel voxel = voxelWorldManager.environment.GetVoxel(position);
        return !voxel.isEmpty && !voxel.hasWater;
    }

    // can the player walk through this voxel?
    public bool IsTraversiblePosition(Vector3Int position,
            ICollection<TangibleEntity> ignoredCreatures) {
        Voxel voxel = voxelWorldManager.environment.GetVoxel(position);
        return (voxel.isEmpty || voxel.hasWater) && 
            !nonVoxelWorld.IsPositionOccupied(position, ignoredCreatures);
    }

    public bool IsReachablePosition(Vector3Int newOrigin, Traveller traveller,
            ICollection<TangibleEntity> ignoredCreatures) {
        HashSet<Vector3Int> newPositions = traveller.GetPositionsIfOriginAtPosition(newOrigin);
        foreach (Vector3Int position in newPositions) {
            if (!IsTraversiblePosition(position, ignoredCreatures)) {
                return false;
            }
        }

        List<Vector3Int> footPositions = Coordinates.GetFloorPositions(newPositions);
        List<Vector3Int> below = footPositions.Select(x => x + Vector3Int.down).ToList();
        int numWalkable = NumWalkable(below);
        return IsNumWalkableAcceptable(numWalkable, below.Count);
    }

    private bool IsNumWalkableAcceptable(int numWalkable, int containerCount) {
        return (float)numWalkable / containerCount >= FRACTION_WALKABLE_MINIMUM;
    }

    public Vector3Int? GetTerrainAdjustedCoordinate(Vector3Int requestedCoordinate, Traveller traveller,
            List<TangibleEntity> ignoredCreatures) {
        VoxelPlayEnvironment environment = voxelWorldManager.environment;

        HashSet<Vector3Int> requestedCoordinates = traveller.GetPositionsIfOriginAtPosition(requestedCoordinate);

        // Allow movement forward onto flat land with at least one walkable tile beneath
        List<Vector3Int> footCoordinates = Coordinates.GetFloorPositions(requestedCoordinates);
        if (AllUnoccupied(requestedCoordinates, ignoredCreatures)) {
            List<Vector3Int> belowRequested = footCoordinates.Select(x => x + Vector3Int.down).ToList();
            int numWalkable = NumWalkable(belowRequested);
            if (IsNumWalkableAcceptable(numWalkable, footCoordinates.Count)) {
                return requestedCoordinate;
            }

            // Allow movement onto ground below if there's at least one walkable tile beneath
            if (AllUnoccupied(belowRequested, ignoredCreatures)) {
                List<Vector3Int> twoBelowRequested = belowRequested.Select(x => x + Vector3Int.down).ToList();
                int numWalkable2 = NumWalkable(twoBelowRequested);
                if (IsNumWalkableAcceptable(numWalkable2, twoBelowRequested.Count)) {
                    return requestedCoordinate + Vector3Int.down;
                }
            }
        }

        // Allow movement up 1 tile
        List<Vector3Int> aboveRequested = requestedCoordinates.Select(x => x + Vector3Int.up).ToList();
        if (AllUnoccupied(aboveRequested, ignoredCreatures)) {
            int numWalkable = NumWalkable(requestedCoordinates);
            if (IsNumWalkableAcceptable(numWalkable, footCoordinates.Count)) {
                // ensure there's headroom to move to that tile
                List<Vector3Int> abovePlayer = traveller.occupiedPositions
                    .Select(x => x + Vector3Int.up).ToList();
                if (AllUnoccupied(abovePlayer, ignoredCreatures)) {
                    return requestedCoordinate + Vector3Int.up;
                }
            }
        }

        return null;
    }

    private bool AllUnoccupied(ICollection<Vector3Int> positions,
            ICollection<TangibleEntity> ignoredCreatures) {
        foreach (Vector3Int position in positions) {
            if (!IsTraversiblePosition(position, ignoredCreatures)) {
                return false;
            }
        }
        return true;
    }

    private int NumWalkable(ICollection<Vector3Int> positions) {
        return positions.Count((Vector3Int position) => IsWalkablePosition(position));
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

    public Vector3Int GetSpriteDesiredCoordinate(TangibleEntity entity,
            SpriteMoveDirection moveDirection) {
        if (moveDirection == SpriteMoveDirection.FORWARD) {
            return entity.origin + Vector3Int.forward;
        }
        else if (moveDirection == SpriteMoveDirection.RIGHT) {
            return entity.origin + Vector3Int.right;
        }
        else if (moveDirection == SpriteMoveDirection.BACK) {
            return entity.origin + Vector3Int.back;
        }
        else if (moveDirection == SpriteMoveDirection.LEFT) {
            return entity.origin + Vector3Int.left;
        }
        else {
            throw new System.ArgumentException("Impossible direction provided.");
        }
    }
}
