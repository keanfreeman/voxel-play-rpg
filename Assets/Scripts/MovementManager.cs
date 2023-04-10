using InstantiatedEntity;
using Nito.Collections;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] PathVisualizer pathVisualizer;
    [SerializeField] NonVoxelWorld nonVoxelWorld;

    public Dictionary<Traveller, Deque<Vector3Int>> movingCreatures { get; private set; } 
        = new Dictionary<Traveller, Deque<Vector3Int>>();

    public bool IsMoving(Traveller traveller) {
        return movingCreatures.ContainsKey(traveller);
    }

    public IEnumerator MoveAlongPath(Traveller traveller, Deque<Vector3Int> path) {
        if (path.Count == 0) {
            yield break;
        }

        // TODO handle if new path is requested when one is already running.
        movingCreatures[traveller] = path;
        yield return MoveEntity(traveller);
    }

    public IEnumerator MoveEntity(Traveller traveller) {
        Vector3Int? currDestination = null;
        Deque<Vector3Int> path = movingCreatures[traveller];
        pathVisualizer.DrawPath(path);
        while (path.Count > 0) {
            if (!currDestination.HasValue) {
                int lastIndex = path.Count - 1;
                currDestination = path[lastIndex];
                path.RemoveFromBack();
            }

            if (nonVoxelWorld.IsPositionOccupied(currDestination.Value, traveller)) {
                Debug.Log("Movement was interrupted along path.");
                traveller.SetMoveAnimation(false);
                movingCreatures.Remove(traveller);
                yield break;
            }

            if (!traveller.isMoving) {
                traveller.MoveOriginToPoint(currDestination.Value);
                pathVisualizer.DestroyNearestMarker();
            }

            while (traveller.isMoving) {
                yield return null;
            }
            currDestination = null;
        }

        traveller.SetMoveAnimation(false);
        movingCreatures.Remove(traveller);
    }
}
