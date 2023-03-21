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
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SpriteMovement spriteMovement;

    private PlayerCameraDirection playerCameraDirection = PlayerCameraDirection.NORTH;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        moveStartTimestamp = Time.time;
    }

    public void HaltMovement() {
        StopAllCoroutines();
        isMoving = false;
        SetMoveAnimation(isMoving);
    }

    public void HandleControllerMove(InputAction.CallbackContext obj) {
        Vector2 stickValue = obj.ReadValue<Vector2>();

        SpriteMoveDirection requestedDirection;
        float angle = Vector2.SignedAngle(stickValue, Vector2.down);
        if (angle > 45 && angle <= 135) {
            requestedDirection = SpriteMoveDirection.LEFT;
        }
        else if (angle > -45 && angle <= 45) {
            requestedDirection = SpriteMoveDirection.BACK;
        }
        else if (angle > -135 && angle <= -45) {
            requestedDirection = SpriteMoveDirection.RIGHT;
        }
        else {
            requestedDirection = SpriteMoveDirection.FORWARD;
        }
        permanentMoveDirection = requestedDirection;
    }

    public void HandleControllerMoveCancel(InputAction.CallbackContext obj) {
        permanentMoveDirection = SpriteMoveDirection.NONE;
    }

    protected override Vector3Int? GetDestinationFromDirection(SpriteMoveDirection direction) {
        SpriteMoveDirection cameraAdjustedPlayerMove = CameraAdjustedPlayerMove(
            direction, playerCameraDirection);
        Vector3Int desiredCoordinate = spriteMovement.GetSpriteDesiredCoordinate(
            currVoxel, cameraAdjustedPlayerMove);
        Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
            desiredCoordinate, currVoxel);
        if (!actualCoordinate.HasValue) {
            Debug.Log("Not a valid destination terrain-wise.");
            return null;
        }
        Vector3Int destinationCoordinate = actualCoordinate.Value;
        if (nonVoxelWorld.IsPositionOccupied(destinationCoordinate)) {
            Debug.Log("Tried to move into a non-voxel-occupied space.");
            return null;
        }

        return destinationCoordinate;
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

    public void SetPlayerCameraDirection(PlayerCameraDirection direction) {
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
