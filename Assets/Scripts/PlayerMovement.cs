using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;
using MovementDirection;
using UnityEngine.ProBuilder;
using NonVoxel;

public class PlayerMovement : MonoBehaviour {
    // CONST
    [SerializeField]
    const float TIME_TO_ROTATE = 0.5f;
    [SerializeField]
    const float TIME_TO_MOVE_A_TILE = 0.2f;
    [SerializeField]
    const float SPRINT_SPEEDUP = 2f;

    // IMPORT
    VoxelPlayFirstPersonController scriptInstance;
    public GameObject spriteContainer;
    Transform spriteChildTransform;
    GameObject cameraObject;
    VoxelPlayEnvironment environment;
    public NonVoxelWorld nonVoxelWorld;
    public SpriteMovement spriteMovement;

    // STATE
    bool isFollowingSprite = false;

    bool isMoving = false;
    bool isSprinting = false;
    float moveStartTimestamp;
    Vector3 moveStartPoint;
    Vector3 moveEndPoint;

    bool isRotating;
    float rotateStartTimestamp;
    Quaternion startRotation;
    Quaternion endRotation;
    PlayerCameraDirection playerCameraDirection = PlayerCameraDirection.NORTH;

    void Start() {
        environment = VoxelPlayEnvironment.instance;
        moveStartTimestamp = Time.time;
        scriptInstance = GetComponent<VoxelPlayFirstPersonController>();
        cameraObject = GameObject.Find("FirstPersonCharacter");
        spriteChildTransform = spriteContainer.transform.GetChild(0);
    }

    void Update() {
        HandleMovement();
        HandleCameraRotation();

        if (Input.GetKeyUp(KeyCode.K)) {
            ToggleFreeCamera();
        }

        if (Input.GetKeyUp(KeyCode.J)) {
            Debug.Log("Debug key pressed.");
        }
    }

    private void HandleMovement() {
        if (!isFollowingSprite) {
            return;
        }
        if (isRotating || (isMoving && !IsMoveTransitionDone())) {
            return;
        }
        isMoving = false;
        isSprinting = false;

        if (Input.GetKey(KeyCode.LeftShift)) {
            isSprinting = true;
        }
        Vector3 spriteCurrPosition = spriteContainer.transform.position;
        SpriteMoveDirection requestedDirection;
        if (Input.GetKey(KeyCode.W)) {
            requestedDirection = SpriteMoveDirection.FORWARD;
        }
        else if (Input.GetKey(KeyCode.S)) {
            requestedDirection = SpriteMoveDirection.BACK;
        }
        else if (Input.GetKey(KeyCode.A)) {
            requestedDirection = SpriteMoveDirection.LEFT;
        }
        else if (Input.GetKey(KeyCode.D)) {
            requestedDirection = SpriteMoveDirection.RIGHT;
        }
        else if (Input.GetKey(KeyCode.Q)) {
            requestedDirection = SpriteMoveDirection.UP;
        }
        else if (Input.GetKey(KeyCode.E)) {
            requestedDirection = SpriteMoveDirection.DOWN;
        }
        else {
            return;
        }

        SpriteMoveDirection cameraAdjustedPlayerMove = CameraAdjustedPlayerMove(requestedDirection,
            playerCameraDirection);
        Vector3Int desiredCoordinate = spriteMovement.GetSpriteDesiredCoordinate(
            nonVoxelWorld.GetPosition(spriteContainer), cameraAdjustedPlayerMove);
        Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(desiredCoordinate,
            nonVoxelWorld.GetPosition(spriteContainer));
        if (!actualCoordinate.HasValue) {
            Debug.Log("Tried to move in an invalid way.");
            return;
        }
        Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();

        if (nonVoxelWorld.IsPositionOccupied(destinationCoordinate)) {
            Debug.Log("Tried to move into a non-voxel-occupied space.");
            return;
        }
        nonVoxelWorld.SetPosition(spriteContainer, destinationCoordinate);
        moveStartPoint = spriteCurrPosition;
        moveEndPoint = destinationCoordinate;
        isMoving = true;
        moveStartTimestamp = Time.time;
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
        startRotation = spriteChildTransform.transform.rotation;

        if (Input.GetKey(KeyCode.LeftArrow)) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles + (Vector3.up * 90f));
            playerCameraDirection = GetNewCameraDirection(playerCameraDirection, true);
        }
        else if (Input.GetKey(KeyCode.RightArrow)) {
            endRotation = Quaternion.Euler(startRotation.eulerAngles + (Vector3.up * -90f));
            playerCameraDirection = GetNewCameraDirection(playerCameraDirection, false);
        }
        else {
            return;
        }

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

    // returns true when the transition is done
    private bool IsMoveTransitionDone() {
        float timeSinceMoveBegan = Time.time - moveStartTimestamp;
        float sprintMultiplier = isSprinting ? SPRINT_SPEEDUP : 1;
        float fractionOfMovementDone = timeSinceMoveBegan * sprintMultiplier / (TIME_TO_MOVE_A_TILE);
        float linearFriendlyFraction = Mathf.Min(fractionOfMovementDone, 1f);
        spriteContainer.transform.position = Vector3.Lerp(moveStartPoint, moveEndPoint, linearFriendlyFraction);
        return linearFriendlyFraction >= 1f;
    }
    
    private void ToggleFreeCamera() {
        Debug.Log("SWITCHING FREE CAMERA");
        if (isFollowingSprite) {
            transform.SetParent(null);
            scriptInstance.crosshairScale = 0.1f;
        }

        scriptInstance.useThirdPartyController = !scriptInstance.useThirdPartyController;
        scriptInstance.isFlying = !scriptInstance.isFlying;
        scriptInstance.freeMode = !scriptInstance.freeMode;
        scriptInstance.hasCharacterController = !scriptInstance.hasCharacterController;
        scriptInstance.voxelHighlight = !scriptInstance.voxelHighlight;
        scriptInstance.unstuck = !scriptInstance.unstuck;

        if (!isFollowingSprite) {
            transform.position = spriteChildTransform.transform.position
                + (Vector3.up * 5f) + (Vector3.back * 5f);
            cameraObject.transform.rotation = spriteChildTransform.transform.rotation;
            transform.SetParent(spriteChildTransform.transform);

            scriptInstance.crosshairScale = 0;
        }
        isFollowingSprite = !isFollowingSprite;
    }
}
