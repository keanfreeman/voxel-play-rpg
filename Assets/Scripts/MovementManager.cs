using Instantiated;
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

    public Dictionary<Traveller, Deque<Vector3Int>> creaturePaths { get; private set; } 
        = new Dictionary<Traveller, Deque<Vector3Int>>();

    public bool IsMoving(Traveller traveller) {
        return creaturePaths.ContainsKey(traveller);
    }

    public void CancelMovement(Traveller traveller) {
        if (IsMoving(traveller)) {
            creaturePaths[traveller] = null;
            pathVisualizer.EraseAll();
        }
    }

    public IEnumerator MoveAlongPath(Traveller traveller, Deque<Vector3Int> path) {
        if (path.Count == 0) {
            yield break;
        }

        // TODO handle if new path is requested when one is already running.
        creaturePaths[traveller] = path;
        yield return MoveEntity(traveller);
    }

    public IEnumerator MoveEntity(Traveller traveller) {
        Vector3Int? currDestination = null;
        pathVisualizer.DrawPath(creaturePaths[traveller]);
        while (creaturePaths[traveller] != null && creaturePaths[traveller].Count > 0) {
            if (!currDestination.HasValue) {
                int lastIndex = creaturePaths[traveller].Count - 1;
                currDestination = creaturePaths[traveller][lastIndex];
                creaturePaths[traveller].RemoveFromBack();
            }

            if (nonVoxelWorld.IsPositionOccupied(currDestination.Value, traveller)) {
                Debug.Log("Movement was interrupted along path.");
                traveller.SetMoveAnimation(false);
                creaturePaths.Remove(traveller);
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
        creaturePaths.Remove(traveller);
    }
}
