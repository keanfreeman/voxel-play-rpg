using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public class DetachedCameraBottom : MonoBehaviour
{
    [SerializeField] public GameObject seeThroughTarget;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] ConstructionUI constructionUI;

    private Vector3 moveStartPoint;
    private Vector3Int moveEndPoint;
    private float moveStartTime;

    Vector3Int? currHighlighted;

    private const float TRANSITION_TIME = 0.1f;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    public void SetVisibility(bool visibility) {
        StopAllCoroutines();
        gameObject.SetActive(visibility);
    }

    public void MoveImmediate(Vector3Int position) {
        transform.position = position;
    }

    public void MoveAnimated(Vector3Int position) {
        moveStartTime = Time.time;
        moveStartPoint = transform.position;
        moveEndPoint = position;
        StartCoroutine(AnimateMove());
    }

    private IEnumerator AnimateMove() {
        while (Time.time - moveStartTime < TRANSITION_TIME) {
            float fractionOfMovementDone = (Time.time - moveStartTime) / TRANSITION_TIME;
            transform.position = Vector3.Lerp(moveStartPoint, moveEndPoint,
                fractionOfMovementDone);
            yield return null;
        }
        transform.position = moveEndPoint;
        //HighlightVoxel();
    }

    private void HighlightVoxel() {
        currHighlighted = moveEndPoint;
        float edgeWidth = 5f;
        VoxelHitInfo voxelHitInfo;
        voxelWorldManager.GetEnvironment().RayCast(new Rayd(currHighlighted.Value,
            new Vector3(0.001f, 0.001f, 0.001f)), out voxelHitInfo, 0.001f);
        voxelWorldManager.GetEnvironment()
            .VoxelHighlight(voxelHitInfo, new Color(100f, 100f, 100f), edgeWidth);
    }
}