using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] PathVisualizer pathVisualizer;

    public Dictionary<Traveller, List<Vector3Int>> movingCreatures { get; private set; } 
        = new Dictionary<Traveller, List<Vector3Int>>();

    public bool IsMoving(Traveller traveller) {
        return movingCreatures.ContainsKey(traveller);
    }

    public void MoveAlongPath(Traveller traveller, List<Vector3Int> path) {
        // TODO handle if new path is requested when one is already running.
        movingCreatures[traveller] = path;
        StartCoroutine(MoveEntity(traveller));
    }

    private IEnumerator MoveEntity(Traveller traveller) {
        Vector3Int? currDestination = null;
        List<Vector3Int> path = movingCreatures[traveller];
        while (path.Count > 0) {
            if (path.Count != 0) {
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
            }
            currDestination = null;
            yield return null;
        }

        traveller.SetMoveAnimation(false);
        movingCreatures.Remove(traveller);
    }
}
