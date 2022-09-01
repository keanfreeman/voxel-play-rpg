using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;
using PlayerMovement;

public class SpriteMovement : MonoBehaviour
{
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
        PlayerMoveDirection moveDirection;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            moveDirection = PlayerMoveDirection.NORTH;
            moveEndPoint = spriteCurrPosition + Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            moveDirection = PlayerMoveDirection.SOUTH;
            moveEndPoint = spriteCurrPosition + Vector3.back;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            moveDirection = PlayerMoveDirection.WEST;
            moveEndPoint = spriteCurrPosition + Vector3.left;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            moveDirection = PlayerMoveDirection.EAST;
            moveEndPoint = spriteCurrPosition + Vector3.right;
        }
        else if (Input.GetKey(KeyCode.Q)) {
            moveDirection = PlayerMoveDirection.UP;
            moveEndPoint = spriteCurrPosition + Vector3.up;
        }
        else if (Input.GetKey(KeyCode.E)) {
            moveDirection = PlayerMoveDirection.DOWN;
            moveEndPoint = spriteCurrPosition + Vector3.down;
        }
        else {
            return;
        }

        if (!IsValidMoveDirection(moveDirection, spriteIndex)) {
            Debug.Log("Tried to move in an invalid way.");
            return;
        }

        spriteIndex = GetDestinationIndexFromDirection(moveDirection, spriteIndex);
        moveStartPoint = spriteCurrPosition;
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

    private bool IsValidMoveDirection(PlayerMoveDirection moveDirection, Vector3 spriteIndex) {
        Voxel voxel = environment.GetVoxel(GetDestinationIndexFromDirection(moveDirection, spriteIndex));
        if (voxel.isEmpty || !voxel.isSolid) {
            return true;
        }

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
        if (voxel.type == slopeVoxel && playerMoveLinesUpWithSlope(moveDirection, slopeRotation)) {
            return true;
        }

        return false;
    }

    private bool playerMoveLinesUpWithSlope(PlayerMoveDirection moveDirection, int slopeRotation) {
        return moveDirection == PlayerMoveDirection.NORTH && slopeRotation == 0
            || moveDirection == PlayerMoveDirection.EAST && slopeRotation == 1
            || moveDirection == PlayerMoveDirection.SOUTH && slopeRotation == 2
            || moveDirection == PlayerMoveDirection.WEST && slopeRotation == 3;
    }

    private Vector3 GetDestinationIndexFromDirection(PlayerMoveDirection moveDirection, Vector3 startIndex) {
        if (moveDirection == PlayerMoveDirection.NORTH) {
            return startIndex + Vector3.forward;
        }
        if (moveDirection == PlayerMoveDirection.SOUTH) {
            return startIndex + Vector3.back;
        }
        if (moveDirection == PlayerMoveDirection.EAST) {
            return startIndex + Vector3.right;
        }
        if (moveDirection == PlayerMoveDirection.WEST) {
            return startIndex + Vector3.left;
        }
        if (moveDirection == PlayerMoveDirection.UP) {
            return startIndex + Vector3.up;
        }
        if (moveDirection == PlayerMoveDirection.DOWN) {
            return startIndex + Vector3.down;
        }
        throw new System.ArgumentException("Unexpected direction provided");
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
