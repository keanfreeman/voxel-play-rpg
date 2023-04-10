using Nito.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    [SerializeField] GameObject pathMarkerPrefab;

    List<GameObject> pathMarkers = new List<GameObject>();

    public void DrawPath(Deque<Vector3Int> path) {
        foreach (GameObject gameObject in pathMarkers) {
            Destroy(gameObject);
        }
        pathMarkers.Clear();


        foreach (Vector3Int point in path) {
            pathMarkers.Add(
                Instantiate(pathMarkerPrefab, point, Quaternion.identity)
            );
        }
    }

    public void DestroyNearestMarker() {
        if (pathMarkers.Count < 1) {
            return;
        }
        int lastIndex = pathMarkers.Count - 1;
        Destroy(pathMarkers[lastIndex]);
        pathMarkers.RemoveAt(lastIndex);
    }
}
