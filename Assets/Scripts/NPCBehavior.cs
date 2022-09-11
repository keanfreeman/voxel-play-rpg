using MovementDirection;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

// moves around randomly
public class NPCBehavior : MonoBehaviour
{
    [SerializeField]
    const float TIME_TO_ROTATE = 0.5f;
    private const float NPC_MIN_IDLE_TIME = 1;
    private const float NPC_MAX_IDLE_TIME = 5;

    System.Random rng = new System.Random();
    public VoxelPlayEnvironment environment;
    public NonVoxelWorld nonVoxelWorld;
    public SpriteMovement spriteMovement;
    private Transform childTransform;

    private float lastMoveTime = 0;

    public KeyCode rotationDirection = KeyCode.None;
    private bool isRotating = false;
    float rotateStartTimestamp;
    Quaternion startRotation;
    Quaternion endRotation;

    public void Start() {
        childTransform = transform.GetChild(0);
    }

    void Update()
    {
        HandleCameraRotation();
        HandleMovement();
    }

    public void MoveSprite(Vector3Int position) {
        nonVoxelWorld.SetPosition(gameObject, position);
        transform.position = position;
    }

    public Vector3Int GetRandomOneTileMovement() {
        bool isX = rng.Next(0, 2) == 0 ? true : false;
        if (isX) {
            return rng.Next(0, 2) == 0 ? Vector3Int.left : Vector3Int.right;
        }
        return rng.Next(0, 2) == 0 ? Vector3Int.forward : Vector3Int.back;
    }

    private void HandleMovement() {
        if (isRotating || Time.time - lastMoveTime < NPC_MIN_IDLE_TIME) {
            return;
        }
        lastMoveTime = Time.time;

        Vector3Int currPosition = nonVoxelWorld.GetPosition(gameObject);
        Vector3Int newPosition = nonVoxelWorld.GetPosition(gameObject)
            + GetRandomOneTileMovement();
        Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
            newPosition, currPosition);
        if (!actualCoordinate.HasValue) {
            return;
        }
        Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();

        if (!nonVoxelWorld.IsPositionOccupied(destinationCoordinate)
            && environment.GetVoxel(destinationCoordinate).isEmpty) {
            MoveSprite(destinationCoordinate);
        }
    }

    private void HandleCameraRotation() {
        if (isRotating && !IsRotationDone()) {
            return;
        }
        isRotating = false;

        startRotation = childTransform.localRotation;
        if (rotationDirection == KeyCode.LeftArrow) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles
                + (Vector3.up * 90f));
        }
        else if (rotationDirection == KeyCode.RightArrow) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles
                + (Vector3.up * -90f));
        }
        else {
            return;
        }

        rotationDirection = KeyCode.None;
        isRotating = true;
        rotateStartTimestamp = Time.time;
    }

    private bool IsRotationDone() {
        float timeSinceMoveBegan = Time.time - rotateStartTimestamp;
        float fractionRotationComplete = Mathf.Min(timeSinceMoveBegan / TIME_TO_ROTATE, 1);
        childTransform.localRotation = Quaternion.Lerp(startRotation, endRotation,
            fractionRotationComplete);

        return fractionRotationComplete == 1;
    }
}
