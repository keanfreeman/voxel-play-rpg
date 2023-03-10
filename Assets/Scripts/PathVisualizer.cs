using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    [SerializeField] GameObject pathMarkerPrefab;

    List<GameObject> pathMarkers = new List<GameObject>();

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void DrawPath(List<Vector3Int> path) {
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
}
