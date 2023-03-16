using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;
using MovementDirection;
using UnityEngine.ProBuilder;
using NonVoxel;
using UnityEngine.InputSystem;
using System.Drawing;

public class PlayerMovement : Traveller {
    // CONST
    [SerializeField]
    const float TIME_TO_ROTATE = 0.5f;

    [SerializeField] public Camera playerCamera;
    [SerializeField] public GameObject voxelHideTarget;
    [SerializeField] private Transform spriteChildTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SpriteMovement spriteMovement;

    // STATE
    bool isFacingRight = false;

    public bool isRotating;
    float rotateStartTimestamp;
    Quaternion startRotation;
    Quaternion endRotation;
    PlayerCameraDirection playerCameraDirection = PlayerCameraDirection.NORTH;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        moveStartTimestamp = Time.time;
    }

    public void HaltMovement() {
        StopAllCoroutines();
        isMoving = false;
        SetMoveAnimation(isMoving);
    }

    public void SetCameraState(bool newState) {
        playerCamera.enabled = newState;
    }

    // returns true if we need to freeze other controls while moving/rotating
    public bool HandleMovementControls() {
        HandleMovement();
        HandleCameraRotation();

        return isRotating || isMoving;
    }

    public void HandleMovement() {
        if (isMoving || isRotating) {
            return;
        }
        SpriteMoveDirection requestedDirection = inputManager.moveDirection;
        if (requestedDirection == SpriteMoveDirection.NONE) {
            SetMoveAnimation(isMoving);
            return;
        }

        // make player sprite face the direction they asked for
        if (requestedDirection == SpriteMoveDirection.RIGHT) {
            isFacingRight = true;
        }
        else if (requestedDirection == SpriteMoveDirection.LEFT) {
            isFacingRight = false;
        }
        spriteRenderer.flipX = isFacingRight;
        

        SpriteMoveDirection cameraAdjustedPlayerMove = CameraAdjustedPlayerMove(
            requestedDirection, playerCameraDirection);
        Vector3Int desiredCoordinate = spriteMovement.GetSpriteDesiredCoordinate(
            currVoxel, cameraAdjustedPlayerMove);
        Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
            desiredCoordinate, currVoxel);
        if (!actualCoordinate.HasValue) {
            Debug.Log("Tried to move in an invalid way.");
            return;
        }
        Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();
        if (nonVoxelWorld.IsPositionOccupied(destinationCoordinate)) {
            Debug.Log("Tried to move into a non-voxel-occupied space.");
            return;
        }
        MoveToPoint(destinationCoordinate);
    }

    private SpriteMoveDirection CameraAdjustedPlayerMove(SpriteMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        if (isMovingNorth(moveDirection, playerCameraDirection)) {
            return SpriteMoveDirection.FORWARD;
        }
        else if (isMovingEast(moveDirection, playerCameraDirection)) {
            return SpriteMoveDirection.RIGHT;
        }
        else if (isMovingSouth(moveDirection, playerCameraDirection)) {
            return SpriteMoveDirection.BACK;
        }
        else if (isMovingWest(moveDirection, playerCameraDirection)) {
            return SpriteMoveDirection.LEFT;
        }
        throw new System.ArgumentException("Impossible direction provided.");
    }

    public bool isMovingNorth(SpriteMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("RIGHT");
    }

    public bool isMovingEast(SpriteMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("RIGHT");
    }

    public bool isMovingSouth(SpriteMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("RIGHT");
    }

    public bool isMovingWest(SpriteMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("RIGHT");
    }

    private void HandleCameraRotation() {
        if (isMoving || (isRotating && !IsRotationDone())) {
            return;
        }
        isRotating = false;
        startRotation = spriteChildTransform.rotation;

        KeyCode direction;
        if (Input.GetKey(KeyCode.LeftArrow)) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles + (Vector3.up * 90f));
            playerCameraDirection = GetNewCameraDirection(playerCameraDirection, true);
            direction = KeyCode.LeftArrow;
        }
        else if (Input.GetKey(KeyCode.RightArrow)) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles + (Vector3.up * -90f));
            playerCameraDirection = GetNewCameraDirection(playerCameraDirection, false);
            direction = KeyCode.RightArrow;
        }
        else {
            return;
        }

        nonVoxelWorld.RotateNonPlayerCreatures(direction);
        isRotating = true;
        rotateStartTimestamp = Time.time;
    }

    private PlayerCameraDirection GetNewCameraDirection(PlayerCameraDirection currentDirection, bool isLeft) {
        if (isLeft) {
            switch (currentDirection) {
                case PlayerCameraDirection.NORTH:
                    return PlayerCameraDirection.EAST;
                case PlayerCameraDirection.EAST:
                    return PlayerCameraDirection.SOUTH;
                case PlayerCameraDirection.SOUTH:
                    return PlayerCameraDirection.WEST;
                case PlayerCameraDirection.WEST:
                    return PlayerCameraDirection.NORTH;
                default:
                    throw new System.ArgumentException("Unexpected direction provided");
            }
        }
        switch (currentDirection) {
            case PlayerCameraDirection.NORTH:
                return PlayerCameraDirection.WEST;
            case PlayerCameraDirection.EAST:
                return PlayerCameraDirection.NORTH;
            case PlayerCameraDirection.SOUTH:
                return PlayerCameraDirection.EAST;
            case PlayerCameraDirection.WEST:
                return PlayerCameraDirection.SOUTH;
            default:
                throw new System.ArgumentException("Unexpected direction provided");
        }
    }

    private bool IsRotationDone() {
        float timeSinceMoveBegan = Time.time - rotateStartTimestamp;
        float fractionRotationComplete = Mathf.Min(timeSinceMoveBegan / TIME_TO_ROTATE, 1);
        spriteChildTransform.transform.rotation = Quaternion.Lerp(startRotation, endRotation, fractionRotationComplete);

        return fractionRotationComplete == 1;
    }
}
