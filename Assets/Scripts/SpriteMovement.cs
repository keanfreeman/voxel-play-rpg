using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;
using PlayerMovement;

public class SpriteMovement : MonoBehaviour {
    const float TIME_TO_MOVE_A_TILE = 0.2f;
    const float SPRINT_SPEEDUP = 2f;

    VoxelPlayFirstPersonController scriptInstance;
    GameObject spriteObject;
    GameObject cameraObject;
    VoxelPlayEnvironment environment;

    bool isFollowingSprite = false;

    bool isMoving = false;
    bool isSprinting = false;
    float moveStartTimestamp;
    Vector3 moveStartPoint;
    Vector3 moveEndPoint;
    Vector3 spriteIndex = new Vector3(523, 50, 246);

    void Start() {
        environment = VoxelPlayEnvironment.instance;
        moveStartTimestamp = Time.time;
        scriptInstance = GetComponent<VoxelPlayFirstPersonController>();
        spriteObject = GameObject.Find("PlayerSprite");
        cameraObject = GameObject.Find("FirstPersonCharacter");
    }

    void Update() {
        HandleMovement();

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
        if (isMoving && !IsMoveTransitionDone()) {
            return;
        }
        isMoving = false;
        isSprinting = false;

        if (Input.GetKey(KeyCode.LeftShift)) {
            isSprinting = true;
        }
        Vector3 spriteCurrPosition = spriteObject.transform.position;
        PlayerMoveDirection requestedDirection;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            requestedDirection = PlayerMoveDirection.NORTH;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            requestedDirection = PlayerMoveDirection.SOUTH;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            requestedDirection = PlayerMoveDirection.WEST;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            requestedDirection = PlayerMoveDirection.EAST;
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

        PlayerMoveDirection actualDirection = GetRealMoveDirection(requestedDirection, spriteIndex);
        if (actualDirection == PlayerMoveDirection.NONE) {
            Debug.Log("Tried to move in an invalid way.");
            return;
        }

        spriteIndex = GetDestinationPositionFromDirection(actualDirection, spriteIndex);
        moveStartPoint = spriteCurrPosition;
        moveEndPoint = GetDestinationPositionFromDirection(actualDirection, spriteCurrPosition);
        isMoving = true;
        moveStartTimestamp = Time.time;
    }

    // returns true when the transition is done
    private bool IsMoveTransitionDone() {
        float timeSinceMoveBegan = Time.time - moveStartTimestamp;
        float sprintMultiplier = isSprinting ? SPRINT_SPEEDUP : 1;
        float fractionOfMovementDone = timeSinceMoveBegan * sprintMultiplier / (TIME_TO_MOVE_A_TILE);
        float linearFriendlyFraction = Mathf.Min(fractionOfMovementDone, 1f);
        spriteObject.transform.position = Vector3.Lerp(moveStartPoint, moveEndPoint, linearFriendlyFraction);
        return linearFriendlyFraction >= 1f;
    }

    // returns the direction to move after accounting for slopes, other terrain
    private PlayerMoveDirection GetRealMoveDirection(PlayerMoveDirection requestedDirection, Vector3 spriteIndex) {
        Voxel aheadVoxel = environment.GetVoxel(GetDestinationPositionFromDirection(requestedDirection, spriteIndex));

        if ((aheadVoxel.isEmpty)) {
            Voxel belowAheadVoxel = environment.GetVoxel(GetDestinationPositionFromDirection(requestedDirection,
                spriteIndex + Vector3.down));
            if (!belowAheadVoxel.isEmpty) {
                return requestedDirection;
            }
            // handle being on downward slope
            Voxel underfootVoxel = environment.GetVoxel(GetDestinationPositionFromDirection(PlayerMoveDirection.DOWN,
                spriteIndex));
            if (IsSlope(underfootVoxel) && IsSlopeDownRelativeToPlayer(requestedDirection,
                underfootVoxel.GetTextureRotation())) {
                return requestedDirection - 1;
            }
        }

        if (IsSlope(aheadVoxel) && IsSlopeUpRelativeToPlayer(requestedDirection, aheadVoxel.GetTextureRotation())) {
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

    private bool IsSlopeUpRelativeToPlayer(PlayerMoveDirection moveDirection, int slopeRotation) {
        return moveDirection == PlayerMoveDirection.NORTH && slopeRotation == 0
            || moveDirection == PlayerMoveDirection.EAST && slopeRotation == 1
            || moveDirection == PlayerMoveDirection.SOUTH && slopeRotation == 2
            || moveDirection == PlayerMoveDirection.WEST && slopeRotation == 3;
    }

    private bool IsSlopeDownRelativeToPlayer(PlayerMoveDirection moveDirection, int slopeRotation) {
        return moveDirection == PlayerMoveDirection.NORTH && slopeRotation == 2
            || moveDirection == PlayerMoveDirection.EAST && slopeRotation == 3
            || moveDirection == PlayerMoveDirection.SOUTH && slopeRotation == 0
            || moveDirection == PlayerMoveDirection.WEST && slopeRotation == 1;
    }

    // assumes 1 unit of travel
    private Vector3 GetDestinationPositionFromDirection(PlayerMoveDirection moveDirection, Vector3 startIndex) {
        string enumName = moveDirection.ToString();
        Vector3 destinationIndex;
        if (enumName.StartsWith("NORTH")) {
            destinationIndex = startIndex + Vector3.forward;
        }
        else if (enumName.StartsWith("SOUTH")) {
            destinationIndex = startIndex + Vector3.back;
        }
        else if (enumName.StartsWith("EAST")) {
            destinationIndex = startIndex + Vector3.right;
        }
        else if (enumName.StartsWith("WEST")) {
            destinationIndex = startIndex + Vector3.left;
        }
        else if (enumName.StartsWith("UP")) {
            destinationIndex = startIndex + Vector3.up;
        }
        else if (enumName.StartsWith("DOWN")) {
            destinationIndex = startIndex + Vector3.down;
        }
        else {
            throw new System.ArgumentException("No direction provided when expected.");
        }

        Vector3 verticalModifier = Vector3.zero;
        if (enumName.EndsWith("_UP")) {
            verticalModifier += Vector3.up;
        }
        else if (enumName.EndsWith("_DOWN")) {
            verticalModifier += Vector3.down;
        }

        return destinationIndex + verticalModifier;
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

        if (!isFollowingSprite) {
            transform.position = spriteObject.transform.position + (Vector3.up * 5f) + (Vector3.back * 5f);
            cameraObject.transform.rotation = spriteObject.transform.rotation;
            transform.SetParent(spriteObject.transform);

            scriptInstance.crosshairScale = 0;
        }
        isFollowingSprite = !isFollowingSprite;
    }
}
