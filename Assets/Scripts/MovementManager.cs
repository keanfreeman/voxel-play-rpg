using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] PathVisualizer pathVisualizer;

    private PlayerMovement movedEntity;
    private List<Vector3Int> path;
    private Vector3Int? currDestination = null;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void MoveAlongPath(PlayerMovement monoBehaviour, List<Vector3Int> path) {
        // todo make generic so NPCs can move
        movedEntity = monoBehaviour;
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

            if (!movedEntity.isMoving) {
                movedEntity.TryMoveToPoint(currDestination.Value);
                
                pathVisualizer.DestroyNearestMarker();
            }

            while (!movedEntity.IsMoveTransitionDone()) {
                yield return null;
            }
            movedEntity.isMoving = false;
            currDestination = null;
        }

        movedEntity.SetMoveAnimation(false);
    }
}
