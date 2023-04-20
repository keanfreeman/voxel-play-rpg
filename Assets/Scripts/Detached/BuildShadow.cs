using EntityDefinition;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public class BuildShadow : MonoBehaviour
{
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] DetachedCameraBottom detachedCameraBottom;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] ConstructionUI constructionUI;
    [SerializeField] NonVoxelWorld nonVoxelWorld;

    private Vector3Int? drawStart;
    private GameObject currVoxelModelShadow;
    private GameObject objectShadow;

    public void HandleBuildSelect() {
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        Vector3Int currVoxel = detachedCamera.currVoxel;
        if (constructionOptions.GetCurrBuildOption() == ConstructionOptions.BuildOption.Voxels) {
            VoxelDefinition currVD = constructionOptions.GetCurrVoxelDefinition();
            if (drawStart.HasValue) {
                StopDrawingShadow();
                List<Vector3Int> points = Coordinates.GetPointsInCuboid(drawStart.Value, currVoxel);
                foreach (Vector3Int point in points) {
                    voxelWorldManager.GetEnvironment().VoxelPlace(point, currVD);
                }

                drawStart = null;
                return;
            }

            drawStart = currVoxel;
            DrawVoxelShadow(currVD, drawStart.Value, drawStart.Value);
        }
        else {
            objectShadow.transform.parent = null;
            objectShadow.transform.position = currVoxel;
            Instantiated.TangibleObject script = objectShadow.GetComponent<Instantiated.TangibleObject>();

            TangibleObject entityDefinition = constructionOptions.GetCurrObject();
            TangibleObject clone = new TangibleObject(currVoxel, entityDefinition.entityDisplay,
                entityDefinition.occupiedPositions, MovementDirection.Direction.NORTH);
            script.Init(nonVoxelWorld, clone);
            nonVoxelWorld.instantiationMap[clone] = script;
            nonVoxelWorld.AddEntity(script);

            objectShadow = null;
            DrawBuildModeShadow();
        }
        return;
    }

    public void DrawBuildModeShadow() {
        ConstructionOptions options = constructionUI.constructionOptions;
        if (options.GetCurrBuildOption() == ConstructionOptions.BuildOption.Voxels && drawStart.HasValue) {
            VoxelDefinition currVD = constructionUI.constructionOptions.GetCurrVoxelDefinition();
            DrawVoxelShadow(currVD, drawStart.Value, detachedCamera.currVoxel);
        }
        else if (options.GetCurrBuildOption() == ConstructionOptions.BuildOption.Objects) {
            if (objectShadow != null) {
                return;
            }

            TangibleObject tangibleObject = options.GetCurrObject();
            objectShadow = Instantiate(tangibleObject.entityDisplay.prefab, transform);
        }
    }

    public void DrawVoxelShadow(VoxelDefinition vd, Vector3Int start, Vector3Int end) {
        if (currVoxelModelShadow != null) {
            Destroy(currVoxelModelShadow);
        }

        ModelDefinition md = SetUpModelDefinition(vd, start, end);

        Vector3Int diff = end - start;
        VoxelPlayEnvironment env = voxelWorldManager.GetEnvironment();
        currVoxelModelShadow = env.ModelHighlight(md, detachedCamera.currVoxel);
        currVoxelModelShadow.transform.localPosition += new Vector3(0.5f, 0, 0.5f)
            + new Vector3(-0.5f * diff.x, 0, -0.5f * diff.z);
        if (diff.y > 0) {
            currVoxelModelShadow.transform.localPosition += Vector3.down * diff.y;
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

    public void StopDrawingShadow() {
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        if (constructionOptions.GetCurrBuildOption() == ConstructionOptions.BuildOption.Voxels) {
            if (drawStart.HasValue) {
                drawStart = null;
                Destroy(currVoxelModelShadow);
                currVoxelModelShadow = null;
            }
        }
        else {
            if (objectShadow != null) {
                Destroy(objectShadow);
                objectShadow = null;
            }
        }
    }
}
