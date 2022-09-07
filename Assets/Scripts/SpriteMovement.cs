using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;
using PlayerMovement;
using UnityEngine.ProBuilder;
using NonVoxel;

public class SpriteMovement : MonoBehaviour {
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
        PlayerMoveDirection requestedDirection;
        if (Input.GetKey(KeyCode.W)) {
            requestedDirection = PlayerMoveDirection.FORWARD;
        }
        else if (Input.GetKey(KeyCode.S)) {
            requestedDirection = PlayerMoveDirection.BACK;
        }
        else if (Input.GetKey(KeyCode.A)) {
            requestedDirection = PlayerMoveDirection.LEFT;
        }
        else if (Input.GetKey(KeyCode.D)) {
            requestedDirection = PlayerMoveDirection.RIGHT;
        }
        else if (Input.GetKey(KeyCode.Q)) {
            requestedDirection = PlayerMoveDirection.UP;
        }
        else if (Input.GetKey(KeyCode.E)) {
            requestedDirection = PlayerMoveDirection.DOWN;
        }
        else {
            return;
        }

        PlayerMoveDirection actualDirection = GetTerrainAdjustedDirection(requestedDirection,
            nonVoxelWorld.GetPosition(spriteContainer), playerCameraDirection);
        if (actualDirection == PlayerMoveDirection.NONE) {
            Debug.Log("Tried to move in an invalid way.");
            return;
        }

        Vector3Int desiredPosition = nonVoxelWorld.GetPosition(spriteContainer)
            + GetMoveVectorFromDirection(actualDirection, playerCameraDirection);
        if (nonVoxelWorld.IsPositionOccupied(desiredPosition)) {
            Debug.Log("Tried to move into a non-voxel-occupied space.");
            return;
        }
        nonVoxelWorld.SetPosition(spriteContainer, desiredPosition);
        moveStartPoint = spriteCurrPosition;
        moveEndPoint = moveStartPoint + GetMoveVectorFromDirection(actualDirection, playerCameraDirection);
        isMoving = true;
        moveStartTimestamp = Time.time;
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

    // returns the direction to move after accounting for slopes, other terrain.
    // disallows movement if there are obstacles.
    // the player is always technically above slopes when traversing them.
    private PlayerMoveDirection GetTerrainAdjustedDirection(PlayerMoveDirection requestedDirection,
            Vector3Int spriteIndex, PlayerCameraDirection playerCameraDirection) {
        Vector3Int aheadIndex = spriteIndex + GetMoveVectorFromDirection(requestedDirection,
            playerCameraDirection);
        Voxel aheadVoxel = environment.GetVoxel(aheadIndex);

        if (aheadVoxel.isEmpty) {
            // check if player can move onto land
            Vector3Int belowAheadIndex = aheadIndex + Vector3Int.down;
            Voxel belowAheadVoxel = environment.GetVoxel(belowAheadIndex);
            if (!belowAheadVoxel.isEmpty) {
                return requestedDirection;
            }

            // check if player can move down slope to solid tile
            Voxel underfootVoxel = environment.GetVoxel(spriteIndex + Vector3Int.down);
            Voxel twoBelowAheadVoxel = environment.GetVoxel(belowAheadIndex + Vector3Int.down);
            if (IsSlope(underfootVoxel)
                && IsSlopeDownRelativeToPlayer(requestedDirection, 
                    underfootVoxel.GetTextureRotation(), playerCameraDirection)
                && !twoBelowAheadVoxel.isEmpty) {
                return requestedDirection - 1;
            }
        }

        // check if player can move up slope
        if (IsSlope(aheadVoxel) && IsSlopeUpRelativeToPlayer(requestedDirection,
                aheadVoxel.GetTextureRotation(), playerCameraDirection)) {
            return requestedDirection + 1;
        }

        return PlayerMoveDirection.NONE;
    }

    private bool IsSlope(Voxel voxel) {
        VoxelDefinition slopeVoxel = null;
        foreach (VoxelDefinition vd in environment.voxelDefinitions) {
            if (vd != null && vd.name == "SlopeVoxel") {
                slopeVoxel = vd;
            }
        }
        if (slopeVoxel == null) {
            throw new System.SystemException("No expected voxel in world.");
        }
        int slopeRotation = voxel.GetTextureRotation();
        return voxel.type == slopeVoxel;
    }

    private bool IsSlopeUpRelativeToPlayer(PlayerMoveDirection moveDirection, int slopeRotation,
            PlayerCameraDirection playerCameraDirection) {
        return (isMovingNorth(moveDirection, playerCameraDirection) && slopeRotation == 0)
            || (isMovingEast(moveDirection, playerCameraDirection) && slopeRotation == 1)
            || (isMovingSouth(moveDirection, playerCameraDirection) && slopeRotation == 2)
            || (isMovingWest(moveDirection, playerCameraDirection) && slopeRotation == 3);
    }

    private bool IsSlopeDownRelativeToPlayer(PlayerMoveDirection moveDirection, int slopeRotation,
            PlayerCameraDirection playerCameraDirection) {
        return (isMovingNorth(moveDirection, playerCameraDirection) && slopeRotation == 2)
            || (isMovingEast(moveDirection, playerCameraDirection) && slopeRotation == 3)
            || (isMovingSouth(moveDirection, playerCameraDirection) && slopeRotation == 0)
            || (isMovingWest(moveDirection, playerCameraDirection) && slopeRotation == 1);
    }

    private bool isMovingNorth(PlayerMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("RIGHT");
    }

    private bool isMovingEast(PlayerMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("RIGHT");
    }

    private bool isMovingSouth(PlayerMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("RIGHT");
    }

    private bool isMovingWest(PlayerMoveDirection moveDirection,
            PlayerCameraDirection playerCameraDirection) {
        string enumName = moveDirection.ToString();
        return playerCameraDirection == PlayerCameraDirection.WEST && enumName.StartsWith("FORWARD")
            || playerCameraDirection == PlayerCameraDirection.NORTH && enumName.StartsWith("LEFT")
            || playerCameraDirection == PlayerCameraDirection.EAST && enumName.StartsWith("BACK")
            || playerCameraDirection == PlayerCameraDirection.SOUTH && enumName.StartsWith("RIGHT");
    }

    // assumes 1 unit of travel
    private Vector3Int GetMoveVectorFromDirection(
            PlayerMoveDirection moveDirection,
            PlayerCameraDirection rotation) {
        
        string enumName = moveDirection.ToString();

        Vector3Int move = Vector3Int.zero;
        if (isMovingNorth(moveDirection, rotation)) {
            move = Vector3Int.forward;
        }
        else if (isMovingEast(moveDirection, rotation)) {
            move = Vector3Int.right;
        }
        else if (isMovingSouth(moveDirection, rotation)) {
            move = Vector3Int.back;
        }
        else if (isMovingWest(moveDirection, rotation)) {
            move = Vector3Int.left;
        }

        // VERTICAL
        if (enumName.EndsWith("UP")) {
            move += Vector3Int.up;
        }
        else if (enumName.EndsWith("DOWN")) {
            move += Vector3Int.down;
        }

        return move;
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
