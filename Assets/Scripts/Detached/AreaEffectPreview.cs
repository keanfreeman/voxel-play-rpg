using Instantiated;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AreaEffectPreview : MonoBehaviour
{
    [SerializeField] GameObject markerPrefab;

    List<GameObject> areaMarkers = new();

    public void Display(AreaEffectPreviewInfo areaEffectPreviewInfo) {
        Clear();
        List<Vector3Int> points;
        if (areaEffectPreviewInfo.selectModeShape == SelectModeShape.None) return;
        else if (areaEffectPreviewInfo.selectModeShape == SelectModeShape.Cone) {
            points = Coordinates.GetPointsInCone(areaEffectPreviewInfo.performer, 
                areaEffectPreviewInfo.target, areaEffectPreviewInfo.radius);
        }
        else {
            points = Coordinates.GetPointsInSphereCenteredOn(areaEffectPreviewInfo.target,
                areaEffectPreviewInfo.radius);
        }

        Display(points);
    }

    public void Display(List<Vector3Int> positions) {
        foreach (Vector3Int point in positions) {
            areaMarkers.Add(Instantiate(markerPrefab, point, Quaternion.identity));
        }
    }

    public void Clear() {
        while (areaMarkers.Count > 0) {
            int lastIndex = areaMarkers.Count - 1;
            Destroy(areaMarkers[lastIndex]);
            areaMarkers.RemoveAt(lastIndex);
        }
    }
}

public class AreaEffectPreviewInfo {
    public Traveller performer;
    public Vector3Int target;
    public SelectModeShape selectModeShape;
    public int radius;

    public AreaEffectPreviewInfo(Traveller performer, Vector3Int target, 
            SelectModeShape selectModeShape, int radius) {
        this.performer = performer;
        this.target = target;
        this.selectModeShape = selectModeShape;
        this.radius = radius;
    }
}
