using EntityDefinition;
using MovementDirection;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;
using VoxelPlay;
using static ConstructionOptions;

public class BuildShadow : MonoBehaviour
{
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] DetachedCameraBottom detachedCameraBottom;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] ConstructionUI constructionUI;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] PromptUIController promptUIController;
    [SerializeField] TimerUIController timerUIController;

    [SerializeField] GameObject genericNonVoxelObjectPrefab;

    private Vector3Int? drawStart;
    private GameObject currVoxelModelShadow;
    private GameObject objectShadow;
    private Direction rotation = Direction.NORTH;
    private int dPadRotationDirection = 0;
    private Coroutine rotateObjectCoroutine;

    private const string CONSTRUCTION_TIME_COST = "Construction Time Cost";
    private const string CONSTRUCTION_PROMPT_BODY = "Building this will cost you {0}. Are you sure " +
        "you want to build it?";

    private void Update() {
        // todo - replace with more practical placement logic
        if (Input.GetKeyUp(KeyCode.L)) {
            Debug.Log("placing torch");
            Vector3 torchDirection = rotation == Direction.NORTH
                ? Vector3.back : rotation == Direction.EAST
                ? Vector3.left : rotation == Direction.SOUTH
                ? Vector3.forward
                : Vector3.right;
            voxelWorldManager.GetEnvironment().TorchAttach(detachedCamera.currVoxel, torchDirection);
        }
    }

    public void HandleBuildSelect() {
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        Vector3Int currVoxel = detachedCamera.currVoxel;
        TopBuildOption buildOption = constructionOptions.GetCurrTopBuildOption();
        VoxelDefinition currVD = constructionOptions.GetCurrVoxelDefinition();

        // display build time cost prompt
        if (buildOption != TopBuildOption.Voxels || drawStart.HasValue) {
            string timeCostString;
            System.Action followupMethod;
            switch (buildOption) {
                case TopBuildOption.Voxels:
                    int numVoxels;
                    if (constructionOptions.GetCurrVoxelBuildModeOption() == VoxelBuildModeOption.Cuboid) {
                        Dictionary<Vector3Int, VoxelDefinition> points = Coordinates.GetPointsInCuboid(
                            drawStart.Value, currVoxel, currVD);
                        numVoxels = points.Count;
                    }
                    else {
                        Dictionary<Vector3Int, VoxelDefinition> positions = GetTunnelPositions(
                            drawStart.Value, currVoxel, currVD);
                        numVoxels = positions.Count;
                    }

                    timeCostString = promptUIController.GetTimeCostStringFromTimes(
                        0, 0, numVoxels * 30, 0);
                    followupMethod = BuildVoxels;
                    break;
                case TopBuildOption.Objects:
                    timeCostString = promptUIController.GetTimeCostStringFromTimes(
                        0, 0, 30, 0);
                    followupMethod = BuildObject;
                    break;
                case TopBuildOption.Destroy:
                    timeCostString = promptUIController.GetTimeCostStringFromTimes(
                        0, 0, 30, 0);
                    followupMethod = DestroyVoxel;
                    break;
                default:
                    throw new NotImplementedException($"Did not implement prompt for {buildOption}");
            }
            StartCoroutine(WaitForPromptResponse(timeCostString, followupMethod));
        }
        else {
            drawStart = currVoxel;
            DrawVoxelShadow(currVD, drawStart.Value, drawStart.Value, rotation);
        }
    }

    private IEnumerator WaitForPromptResponse(string timeCostString, System.Action followupMethod) {
        CoroutineWithData<bool> cwd = new(this, promptUIController.DisplayPrompt(CONSTRUCTION_TIME_COST,
                        string.Format(CONSTRUCTION_PROMPT_BODY, timeCostString)));
        yield return cwd.coroutine;
        bool respondedYes = cwd.GetResult();
        if (respondedYes) followupMethod.Invoke();
    }

    public void HandleBuildCancel() {
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        TopBuildOption buildOption = constructionOptions.GetCurrTopBuildOption();
        if (buildOption == TopBuildOption.Voxels && drawStart.HasValue) {
            StopDrawingModel();
        }
    }

    private void DestroyVoxel() {
        VoxelPlayEnvironment vpEnv = voxelWorldManager.GetEnvironment();
        Vector3Int currVoxel = detachedCamera.currVoxel;

        vpEnv.VoxelDestroy(currVoxel);
        Instantiated.InstantiatedEntity entity = nonVoxelWorld.GetEntityFromPosition(currVoxel);
        if (entity != null && TypeUtils.IsSameTypeOrIsSubclass(entity,
                typeof(Instantiated.TangibleObject))) {
            nonVoxelWorld.DestroyEntity(entity);
        }

        timerUIController.DeductMinutes(30);
    }

    private void BuildObject() {
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        Vector3Int currVoxel = detachedCamera.currVoxel;

        objectShadow.transform.parent = null;
        objectShadow.transform.position = currVoxel;
        Instantiated.TangibleObject script = objectShadow.GetComponent<Instantiated.TangibleObject>();

        ObjectIdentitySO objectID = constructionOptions.GetCurrObject();
        TangibleObject definition = new TangibleObject(currVoxel, objectID.name, rotation);
        script.Init(nonVoxelWorld, definition, objectID);

        nonVoxelWorld.AddTangibleEntity(definition, script);

        timerUIController.DeductMinutes(30);
        objectShadow = null;
        DrawBuildModeShadow();
    }

    private void BuildVoxels() {
        VoxelPlayEnvironment vpEnv = voxelWorldManager.GetEnvironment();
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        Vector3Int currVoxel = detachedCamera.currVoxel;
        VoxelDefinition currVD = constructionOptions.GetCurrVoxelDefinition();

        int numVoxels;
        if (constructionOptions.GetCurrVoxelBuildModeOption() == VoxelBuildModeOption.Cuboid) {
            Dictionary<Vector3Int, VoxelDefinition> points
                = Coordinates.GetPointsInCuboid(drawStart.Value, currVoxel, currVD);
            numVoxels = points.Count;
            foreach (KeyValuePair<Vector3Int, VoxelDefinition> point in points) {
                if (currVD.name == "Null" || currVD.name == "DefaultVoxelHole") {
                    vpEnv.VoxelDestroy(point.Key);
                }
                else {
                    vpEnv.VoxelPlace(point.Key, currVD);
                    if (currVD.name == "SlopeVoxel") {
                        vpEnv.VoxelSetTexturesRotation(point.Key, (int)rotation);
                    }
                }
            }
        }
        else {
            Dictionary<Vector3Int, VoxelDefinition> positions
                = GetTunnelPositions(drawStart.Value, currVoxel, currVD);
            numVoxels = positions.Count;
            foreach (KeyValuePair<Vector3Int, VoxelDefinition> pair in positions) {
                Vector3Int point = pair.Key;
                VoxelDefinition tunnelVD = pair.Value;
                if (tunnelVD.name == "Null" || tunnelVD.name == "DefaultVoxelHole") {
                    vpEnv.VoxelDestroy(point);
                }
                else {
                    vpEnv.VoxelPlace(point, tunnelVD);
                    if (tunnelVD.name == "SlopeVoxel") {
                        vpEnv.VoxelSetTexturesRotation(point, (int)rotation);
                    }
                }
            }
        }

        timerUIController.DeductMinutes(30 * numVoxels);
        StopDrawingModel();
    }

    private Dictionary<Vector3Int, VoxelDefinition> GetTunnelPositions(Vector3Int start, 
            Vector3Int end, VoxelDefinition floorVoxel) {
        VoxelPlayEnvironment vpEnv = voxelWorldManager.GetEnvironment();
        VoxelDefinition nullVoxel = vpEnv.voxelDefinitions[0];
        Direction tunnelDirection = DirectionCalcs.GetDirectionFromPoints(start, end);

        Vector3Int diff = end - start;
        bool iterateX = tunnelDirection == Direction.EAST || tunnelDirection == Direction.WEST;

        Vector3Int lengthPosition = start;
        Vector3Int lengthIterator = iterateX 
            ? (diff.x > 0 ? Vector3Int.right : Vector3Int.left)
            : (diff.z > 0 ? Vector3Int.forward : Vector3Int.back);

        int tunnelDistance = Mathf.Abs(iterateX ? diff.x : diff.z) + 1;
        int tunnelWidth = Mathf.Abs(iterateX ? diff.z : diff.x) + 1;
        // 1 floor voxel + 3 air voxels = 4 height
        Dictionary<Vector3Int, VoxelDefinition> blocksToPlace = new(tunnelDistance * tunnelWidth * 4);
        do {
            Vector3Int widthPosition = lengthPosition;
            Vector3Int widthIterator = iterateX
            ? (diff.z > 0 ? Vector3Int.forward : Vector3Int.back)
            : (diff.x > 0 ? Vector3Int.right : Vector3Int.left);
            do {
                blocksToPlace.Add(widthPosition, floorVoxel);
                blocksToPlace.Add(widthPosition + Vector3Int.up, nullVoxel);
                blocksToPlace.Add(widthPosition + Vector3Int.up * 2, nullVoxel);
                blocksToPlace.Add(widthPosition + Vector3Int.up * 3, nullVoxel);

                widthPosition += widthIterator;
            } while (iterateX
                ? (widthPosition.z != end.z + widthIterator.z)
                : (widthPosition.x != end.x + widthIterator.x));

            lengthPosition += lengthIterator;
            if (lengthPosition.y != end.y) {
                lengthPosition.y += diff.y > 0 ? 1 : -1;
            }
        } while (iterateX 
            ? (lengthPosition.x != end.x + lengthIterator.x) 
            : (lengthPosition.z != end.z + lengthIterator.z));

        return blocksToPlace;
    }

    public void DrawBuildModeShadow() {
        ConstructionOptions options = constructionUI.constructionOptions;
        if (options.GetCurrTopBuildOption() == TopBuildOption.Voxels) {
            if (drawStart.HasValue) {
                VoxelDefinition currVD = constructionUI.constructionOptions.GetCurrVoxelDefinition();
                DrawVoxelShadow(currVD, drawStart.Value, detachedCamera.currVoxel, rotation);
            }
        }
        else if (options.GetCurrTopBuildOption() == TopBuildOption.Objects) {
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

        currVoxelModelShadow.transform.parent = null;
        Vector3 newPosition = new Vector3(0.5f, 0, 0.5f) + start 
            + new Vector3(0.5f * diff.x, 0, 0.5f * diff.z);
        if (diff.y < 0) {
            newPosition += new Vector3Int(0, diff.y, 0);
        }
        currVoxelModelShadow.transform.position = newPosition;
    }

    private ModelDefinition SetUpModelDefinition(VoxelDefinition voxelType, Vector3Int start, Vector3Int end,
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

        VoxelBuildModeOption buildMode = constructionUI.constructionOptions.GetCurrVoxelBuildModeOption();
        Dictionary<Vector3Int, VoxelDefinition> positionToTypeMap;
        if (buildMode == VoxelBuildModeOption.Cuboid) {
            positionToTypeMap = Coordinates.GetPointsInCuboid(start, end, voxelType);
        }
        else {
            positionToTypeMap = GetTunnelPositions(start, end, voxelType);
        }

        Vector3Int bottomLeft = Coordinates.GetBottomLeftOfCuboid(start, end);
        List<ModelBit> modelBits = new();
        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                for (int z = 0; z < sizeZ; z++) {
                    Vector3Int currModelPosition = bottomLeft + new Vector3Int(x, y, z);
                    VoxelDefinition type = positionToTypeMap.GetValueOrDefault(currModelPosition, null);
                    if (type != null) {
                        modelBits.Add(new ModelBit(md.GetVoxelIndex(x, y, z), type,
                            DirectionCalcs.GetDegreesFromDirection(rotation)));
                    }
                }
            }
        }
        md.bits = modelBits.ToArray();

        md.ComputeFinalColors();
        md.ComputeBounds();

        return md;
    }

    public void StopDrawingShadow() {
        ConstructionOptions constructionOptions = constructionUI.constructionOptions;
        if (constructionOptions.GetCurrTopBuildOption() == TopBuildOption.Voxels) {
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
            Debug.Log($"New rotation is {rotation}");
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
