using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;

public class SpriteMovement : MonoBehaviour
{
    const float TIME_TO_MOVE_A_TILE = 0.2f;
    const float SPRINT_SPEEDUP = 2f;

    VoxelPlayFirstPersonController scriptInstance;
    GameObject spriteObject;
    GameObject cameraObject;

    bool isFollowingSprite = false;
    
    bool isMoving = false;
    bool isSprinting = false;
    float moveStartTimestamp;
    Vector3 moveStartPoint;
    Vector3 moveEndPoint;

    void Start() {
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
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            moveEndPoint = spriteCurrPosition + Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            moveEndPoint = spriteCurrPosition + Vector3.back;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            moveEndPoint = spriteCurrPosition + Vector3.left;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            moveEndPoint = spriteCurrPosition + Vector3.right;
        }
        else if (Input.GetKey(KeyCode.Q)) {
            moveEndPoint = spriteCurrPosition + Vector3.up;
        }
        else if (Input.GetKey(KeyCode.E)) {
            moveEndPoint = spriteCurrPosition + Vector3.down;
        }
        else {
            return;
        }

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

    private void ToggleFreeCamera() {
        Debug.Log("SWITCHING FREE CAMERA");
        if (isFollowingSprite) {
            transform.SetParent(null);
        }

        scriptInstance.useThirdPartyController = !scriptInstance.useThirdPartyController;
        scriptInstance.isFlying = !scriptInstance.isFlying;
        scriptInstance.freeMode = !scriptInstance.freeMode;
        scriptInstance.startOnFlat = !scriptInstance.startOnFlat;
        scriptInstance.hasCharacterController = !scriptInstance.hasCharacterController;

        if (!isFollowingSprite) {
            transform.position = spriteObject.transform.position + (Vector3.up * 5f) + (Vector3.back * 5f);
            cameraObject.transform.rotation = spriteObject.transform.rotation;
            transform.SetParent(spriteObject.transform);
        }
        isFollowingSprite = !isFollowingSprite;
    }
}
