using EntityDefinition;
using MovementDirection;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelPlay;
using static ConstructionOptions;

public class BuildShadow : MonoBehaviour
{
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] DetachedCameraBottom detachedCameraBottom;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] ConstructionUI constructionUI;
    [SerializeField] NonVoxelWorld nonVoxelWorld;

    [SerializeField] GameObject genericNonVoxelObjectPrefab;

    private Vector3Int? drawStart;
    private GameObject currVoxelModelShadow;
    private GameObject objectShadow;
    private Direction rotation = Direction.NORTH;
    private int dPadRotationDirection = 0;
    private Coroutine rotateObjectCoroutine;

    public void HandleBuildSelect() {
        VoxelPlayEnvironment vpEnv = voxelWorldManager.GetEnvironment();
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        Vector3Int currVoxel = detachedCamera.currVoxel;
        BuildOption buildOption = constructionOptions.GetCurrBuildOption();

        if (buildOption == BuildOption.Voxels) {
            VoxelDefinition currVD = constructionOptions.GetCurrVoxelDefinition();
            if (drawStart.HasValue) {
                List<Vector3Int> points = Coordinates.GetPointsInCuboid(drawStart.Value, currVoxel);
                foreach (Vector3Int point in points) {
                    if (currVD.name == "Null" || currVD.name == "DefaultVoxelHole") {
                        vpEnv.VoxelDestroy(point);
                    }
                    else {
                        vpEnv.VoxelPlace(point, currVD);
                        if (currVD.name == "SlopeVoxel") {
                            vpEnv.VoxelSetTexturesRotation(point, (int)rotation);
                        }
                    }
                }

                StopDrawingModel();
                return;
            }

            drawStart = currVoxel;
            DrawVoxelShadow(currVD, drawStart.Value, drawStart.Value, rotation);
        }
        else if (buildOption == BuildOption.Objects) {
            objectShadow.transform.parent = null;
            objectShadow.transform.position = currVoxel;
            Instantiated.TangibleObject script = objectShadow.GetComponent<Instantiated.TangibleObject>();

            ObjectIdentitySO objectID = constructionOptions.GetCurrObject();
            TangibleObject definition = new TangibleObject(currVoxel, rotation, objectID.name);
            script.Init(nonVoxelWorld, definition, objectID);

            nonVoxelWorld.AddTangibleEntity(definition, script);

            objectShadow = null;
            DrawBuildModeShadow();
        }
        else {
            vpEnv.VoxelDestroy(currVoxel);
            Instantiated.InstantiatedEntity entity = nonVoxelWorld.GetEntityFromPosition(currVoxel);
            if (entity != null && TypeUtils.IsSameTypeOrIsSubclass(entity, 
                    typeof(Instantiated.TangibleObject))) {
                nonVoxelWorld.DestroyEntity(entity);
            }
        }
        return;
    }

    public void DrawBuildModeShadow() {
        ConstructionOptions options = constructionUI.constructionOptions;
        if (options.GetCurrBuildOption() == BuildOption.Voxels) {
            if (drawStart.HasValue) {
                VoxelDefinition currVD = constructionUI.constructionOptions.GetCurrVoxelDefinition();
                DrawVoxelShadow(currVD, drawStart.Value, detachedCamera.currVoxel, rotation);
            }
        }
        else if (options.GetCurrBuildOption() == BuildOption.Objects) {
            StopDrawingObject();

            Instantiated.TangibleObject objectScript;
            if (objectShadow == null) {
                ObjectIdentitySO objectID = options.GetCurrObject();
                objectShadow = Instantiate(genericNonVoxelObjectPrefab, transform);
                objectScript = objectShadow.GetComponent<Instantiated.TangibleObject>();
                Instantiate(objectID.prefab, objectScript.leafTransform);
            }
            else {
                objectScript = objectShadow.GetComponent<Instantiated.TangibleObject>();
            }
            
            Quaternion rotationQ = Coordinates.GetRotationFromAngle(
                DirectionCalcs.GetDegreesFromDirection(rotation));
            objectScript.rotationTransform.rotation = rotationQ;
        }
    }

    public void DrawVoxelShadow(VoxelDefinition vd, Vector3Int start, Vector3Int end, Direction rotation) {
        if (currVoxelModelShadow != null) {
            Destroy(currVoxelModelShadow);
        }

        ModelDefinition md = SetUpModelDefinition(vd, start, end, rotation);

        Vector3Int diff = end - start;
        VoxelPlayEnvironment env = voxelWorldManager.GetEnvironment();
        currVoxelModelShadow = env.ModelHighlight(md, detachedCamera.currVoxel);
        currVoxelModelShadow.transform.localPosition += new Vector3(0.5f, 0, 0.5f)
            + new Vector3(-0.5f * diff.x, 0, -0.5f * diff.z);
        if (diff.y > 0) {
            currVoxelModelShadow.transform.localPosition += Vector3.down * diff.y;
        }
    }

    private ModelDefinition SetUpModelDefinition(VoxelDefinition vd, Vector3Int start, Vector3Int end,
            Direction rotation) {
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
                    md.bits[iterator] = new ModelBit(md.GetVoxelIndex(x, y, z), vd,
                        DirectionCalcs.GetDegreesFromDirection(rotation));
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
        if (constructionOptions.GetCurrBuildOption() == BuildOption.Voxels) {
            StopDrawingModel();
        }
        else {
            StopDrawingObject();
        }
    }

    private void StopDrawingModel() {
        if (drawStart.HasValue) {
            drawStart = null;
            Destroy(currVoxelModelShadow);
            currVoxelModelShadow = null;
        }
    }

    private void StopDrawingObject() {
        if (objectShadow != null) {
            Destroy(objectShadow);
            objectShadow = null;
        }
    }

    public void HandleRotateObject(InputAction.CallbackContext obj) {
        if (!detachedCamera.isBuildMode) return;

        float direction = obj.ReadValue<float>();
        dPadRotationDirection = direction == 0 ? 0 
            : direction < 0 ? -1 
            : 1;
        if (dPadRotationDirection == 0) {
            StopRotation();
            return;
        }

        if (rotateObjectCoroutine == null) {
            rotateObjectCoroutine = StartCoroutine(ExecuteRotate());
        }
    }

    public IEnumerator ExecuteRotate() {
        while (dPadRotationDirection != 0) {
            int newRotation = (int)rotation + dPadRotationDirection;
            int enumNumOptions = Enum.GetNames(typeof(Direction)).Length;
            if (newRotation >= enumNumOptions) {
                newRotation = 0;
            }
            else if (newRotation < 0) {
                newRotation = enumNumOptions - 1;
            }

            rotation = (Direction)newRotation;
            DrawBuildModeShadow();
            yield return new WaitForSeconds(0.3f);
        }

        rotateObjectCoroutine = null;
    }

    public void HandleCancelRotateObject(InputAction.CallbackContext obj) {
        StopRotation();
    }

    private void StopRotation() {
        if (rotateObjectCoroutine != null) {
            StopCoroutine(rotateObjectCoroutine);
            rotateObjectCoroutine = null;
            dPadRotationDirection = 0;
        }
    }
}
