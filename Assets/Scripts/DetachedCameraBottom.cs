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

    public bool isDrawing { get; private set; }

    private Vector3 moveStartPoint;
    private Vector3Int moveEndPoint;
    private float moveStartTime;
    private GameObject currModelGO;

    Vector3Int? currHighlighted;

    private const float TRANSITION_TIME = 0.1f;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    public void DrawModel(VoxelDefinition vd, Vector3Int start, Vector3Int end) {
        if (currModelGO != null) {
            Destroy(currModelGO);
        }

        ModelDefinition md = SetUpModelDefinition(vd, start, end);

        Vector3Int diff = end - start;
        VoxelPlayEnvironment env = voxelWorldManager.GetEnvironment();
        currModelGO = env.ModelHighlight(md, detachedCamera.currVoxel);
        currModelGO.transform.localPosition += new Vector3(0.5f, 0, 0.5f)
            + new Vector3(-0.5f * diff.x, 0, -0.5f * diff.z);
        if (diff.y > 0) {
            currModelGO.transform.localPosition += Vector3.down * diff.y;
        }
    }

    private ModelDefinition SetUpModelDefinition(VoxelDefinition vd, Vector3Int start, Vector3Int end) {
        Vector3Int diff = start - end;
        int sizeX = Mathf.Abs(diff.x) + 1;
        int sizeY = Mathf.Abs(diff.y) + 1;
        int sizeZ = Mathf.Abs(diff.z) + 1;

        ModelDefinition md = ModelDefinition.Create(sizeX, sizeY, sizeZ);
        md.offsetX = 0;
        md.offsetY = 0;
        md.offsetZ = 0;
        md.exclusiveTree = false;
        md.fitToTerrain = false;
        md.buildDuration = 0;

        md.bits = new ModelBit[Coordinates.GetNumPointsInCuboid(start, end)];
        int iterator = 0;
        for (int x = 0; x < Mathf.Abs(diff.x) + 1; x++) {
            for (int y = 0; y < Mathf.Abs(diff.y) + 1; y++) {
                for (int z = 0; z < Mathf.Abs(diff.z) + 1; z++) {
                    md.bits[iterator] = new ModelBit(md.GetVoxelIndex(x, y, z), vd);
                    iterator += 1;
                }
            }
        }

        md.ComputeFinalColors();
        md.ComputeBounds();

        return md;
    }

    public void StopDrawingModel() {
        Destroy(currModelGO);
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
