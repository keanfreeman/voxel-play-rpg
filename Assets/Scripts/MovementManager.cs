using Nito.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] PathVisualizer pathVisualizer;

    public Dictionary<Traveller, Deque<Vector3Int>> movingCreatures { get; private set; } 
        = new Dictionary<Traveller, Deque<Vector3Int>>();

    public bool IsMoving(Traveller traveller) {
        return movingCreatures.ContainsKey(traveller);
    }

    public Coroutine MoveAlongPath(Traveller traveller, Deque<Vector3Int> path) {
        // TODO handle if new path is requested when one is already running.
        movingCreatures[traveller] = path;
        return StartCoroutine(MoveEntity(traveller));
    }

    public IEnumerator MoveEntity(Traveller traveller) {
        Vector3Int? currDestination = null;
        Deque<Vector3Int> path = movingCreatures[traveller];
        pathVisualizer.DrawPath(path);
        while (path.Count > 0) {
            if (!currDestination.HasValue) {
                int lastIndex = path.Count - 1;
                currDestination = path[lastIndex];
                path.RemoveAt(lastIndex);
            }

            if (!traveller.isMoving) {
                traveller.MoveToPoint(currDestination.Value);
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
