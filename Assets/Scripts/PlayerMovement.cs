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
    [SerializeField] public GameObject voxelHideTarget;
    [SerializeField] private Transform rotationTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SpriteMovement spriteMovement;
    [SerializeField] private CameraManager cameraManager;

    // STATE
    bool isFacingRight = false;

    public PlayerCameraDirection playerCameraDirection = PlayerCameraDirection.NORTH;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        moveStartTimestamp = Time.time;
    }

    public void HaltMovement() {
        StopAllCoroutines();
        isMoving = false;
        SetMoveAnimation(isMoving);
    }

    // returns true if we need to freeze other controls while moving/rotating
    public bool HandleMovementControls() {
        HandleMovement();

        return isMoving || cameraManager.isRotating;
    }

    public void HandleMovement() {
        if (isMoving || cameraManager.isRotating) {
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

    public void SetCameraDirection(PlayerCameraDirection direction) {
        playerCameraDirection = direction;
    }

    public void RotateCameraDirection(float direction) {
        if (direction > 0) {
            switch (playerCameraDirection) {
                case PlayerCameraDirection.NORTH:
                    playerCameraDirection = PlayerCameraDirection.EAST;
                    break;
                case PlayerCameraDirection.EAST:
                    playerCameraDirection = PlayerCameraDirection.SOUTH;
                    break;
                case PlayerCameraDirection.SOUTH:
                    playerCameraDirection = PlayerCameraDirection.WEST;
                    break;
                case PlayerCameraDirection.WEST:
                    playerCameraDirection = PlayerCameraDirection.NORTH;
                    break;
                default:
                    throw new System.ArgumentException("Unexpected direction provided");
            }
        }
        else {
            switch (playerCameraDirection) {
                case PlayerCameraDirection.NORTH:
                    playerCameraDirection = PlayerCameraDirection.WEST;
                    break;
                case PlayerCameraDirection.EAST:
                    playerCameraDirection = PlayerCameraDirection.NORTH;
                    break;
                case PlayerCameraDirection.SOUTH:
                    playerCameraDirection = PlayerCameraDirection.EAST;
                    break;
                case PlayerCameraDirection.WEST:
                    playerCameraDirection = PlayerCameraDirection.SOUTH;
                    break;
                default:
                    throw new System.ArgumentException("Unexpected direction provided");
            }
        }
    }

    public override void RotateSprite(float degrees) {
        rotationTransform.Rotate(Vector3.up, degrees);
    }

    public override void SetSpriteRotation(Vector3 rotation) {
        rotationTransform.rotation = Quaternion.Euler(rotation);
    }
}
