using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] PathVisualizer pathVisualizer;

    private Traveller traveller;
    private List<Vector3Int> path;
    private Vector3Int? currDestination = null;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void MoveAlongPath(Traveller traveller, List<Vector3Int> path) {
        // todo make generic so NPCs can move
        this.traveller = traveller;
        this.path = path;
        StartCoroutine(MoveEntity());
    }

    private IEnumerator MoveEntity() {
        while (path.Count != 0) {
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
    }
}
