using Instantiated;
using MovementDirection;
using NonVoxel;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VoxelPlay;

public class CameraManager : MonoBehaviour {
    [SerializeField] GameObject mainCameraTarget;
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject detachedModeContainer;
    [SerializeField] Transform detachedModeSpriteRotator;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] PartyManager partyManager;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] GameObject seeThroughOrigin;
    [SerializeField] GameObject seeThroughTarget;

    public bool isRotating { get; private set; }
    public TangibleEntity attachedEntity { get; private set; }
    
    private float rotateStartTimestamp;
    private float previousRotateFraction;
    private float direction;

    private float stickValue;

    private const float TIME_TO_ROTATE = 0.5f;
    private const float DETACHED_ROTATION_SPEED = 100f;
    private const float TIME_TO_MOVE_TO_NEW_TARGET = 1f;

    public GameObject GetMainCameraTarget() {
        return mainCameraTarget;
    }

    public Camera GetMainCamera() {
        return mainCamera;
    }

    public void SetSeeThroughTarget(VoxelPlayEnvironment env) {
        env.seeThroughOrigin = seeThroughOrigin;
        env.seeThroughTarget = seeThroughTarget;
    }

    public IEnumerator MoveCameraToTargetCreature(Traveller traveller) {
        int creatureRadius = EntitySizeCalcs.GetRadius(traveller.GetStats().size);
        Vector3 cameraOffset = new Vector3(0.5f, 0.5f, 0.5f) * creatureRadius;

        DeParentCamera();
        Vector3 start = mainCameraTarget.transform.position;
        Vector3 end = traveller.transform.position + cameraOffset;
        float startTime = Time.time;

        float fractionDone = 0f;
        while (fractionDone < 1f) {
            fractionDone = (Time.time - startTime) / TIME_TO_MOVE_TO_NEW_TARGET;
            float smoothInterpolation = Mathf.SmoothStep(0f, 1f, fractionDone);
            mainCameraTarget.transform.localPosition = Vector3.Lerp(start, end, smoothInterpolation);
            yield return null;
        }

        mainCameraTarget.transform.parent = traveller.transform;
        attachedEntity = traveller;
        mainCameraTarget.transform.localPosition = cameraOffset;
    }

    public Direction GetCameraApproximateDirection() {
        float yAngle = mainCameraTarget.transform.rotation.eulerAngles.y;
        if (yAngle > 45 && yAngle <= 135) {
            return Direction.EAST;
        }
        else if (yAngle > 135 && yAngle <= 225) {
            return Direction.SOUTH;
        }
        else if (yAngle > 225 && yAngle <= 315) {
            return Direction.WEST;
        }
        else {
            return Direction.NORTH;
        }
    }

    public void DeParentCamera() {
        mainCameraTarget.transform.parent = null;
        attachedEntity = null;
    }

    // todo make gradual animation
    public void AttachCameraToPlayer(PlayerCharacter playerMovement) {
        mainCameraTarget.transform.parent = playerMovement.playerObject.transform;
        attachedEntity = playerMovement;
        // need to jump to nearest 90 degree angle if coming from detached
        Direction direction = GetCameraApproximateDirection();
        playerMovement.SetPlayerCameraDirection(direction);
        float adjustedYAngle = DirectionCalcs.GetDegreesFromDirection(direction);
        
        mainCameraTarget.transform.SetLocalPositionAndRotation(new Vector3(0.5f, 0.5f, 0.5f),
            Quaternion.Euler(0, adjustedYAngle, 0));
        SetAllSpriteRotations(adjustedYAngle);
        mainCamera.transform.SetLocalPositionAndRotation(new Vector3(0, 6, -6),
            Quaternion.Euler(45f, 0, 0));
        detachedCamera.BecomeInactive();
    }

    public void AttachCameraToDetached() {
        mainCameraTarget.transform.parent = detachedModeContainer.transform;
        attachedEntity = null;
        mainCameraTarget.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
        mainCamera.transform.SetLocalPositionAndRotation(new Vector3(0, 6, -6),
            Quaternion.Euler(45f, 0, 0));

        detachedModeSpriteRotator.rotation = mainCameraTarget.transform.rotation;
    }

    public void Rotate90Degrees(InputAction.CallbackContext obj) {
        float stickValue = obj.ReadValue<Vector2>().x;
        if (Mathf.Abs(stickValue) >= 0.5f && !isRotating) {
            isRotating = true;

            direction = stickValue > 0 ? 1 : -1;
            partyManager.currControlledCharacter.RotateCameraDirection(direction);

            rotateStartTimestamp = Time.time;
            previousRotateFraction = 0;
            StartCoroutine(ExecuteRotation());
        }
    }

    private IEnumerator ExecuteRotation() {
        float fractionRotationComplete = 0;
        while (fractionRotationComplete < 1) {
            float timeSinceMoveBegan = Time.time - rotateStartTimestamp;
            fractionRotationComplete = Mathf.Min(timeSinceMoveBegan / TIME_TO_ROTATE, 1);
            float degreesToRotate = 90f * direction * (fractionRotationComplete - previousRotateFraction);
            previousRotateFraction = fractionRotationComplete;

            mainCameraTarget.transform.Rotate(Vector3.up, degreesToRotate);
            foreach (PlayerCharacter playerMovement in partyManager.partyMembers) {
                playerMovement.RotateSprite(degreesToRotate);
            }
            foreach (NPC npc in nonVoxelWorld.npcs) {
                npc.RotateSprite(degreesToRotate);
            }

            yield return null;
        }

        isRotating = false;
    }

    public void RotateDetached(InputAction.CallbackContext obj) {
        stickValue = obj.ReadValue<Vector2>().x;
        if (!isRotating && stickValue != 0) {
            isRotating = true;
            StartCoroutine(ExecuteRotateDetached());
        }
    }

    public void StopRotatingDetached(InputAction.CallbackContext obj) {
        stickValue = 0;
        isRotating = false;
        StopAllCoroutines();
    }

    private IEnumerator ExecuteRotateDetached() {
        while (stickValue != 0) {
            float degrees = stickValue * Time.deltaTime * DETACHED_ROTATION_SPEED;
            mainCameraTarget.transform.Rotate(Vector3.up, degrees);
            detachedModeSpriteRotator.Rotate(Vector3.up, degrees);
            foreach (PlayerCharacter playerMovement in partyManager.partyMembers) {
                playerMovement.RotateSprite(degrees);
            }
            foreach (NPC npc in nonVoxelWorld.npcs) {
                npc.RotateSprite(degrees);
            }
            yield return null;
        }

        isRotating = false;
    }

    private void SetAllSpriteRotations(float yAngle) {
        Quaternion rotation = Coordinates.GetRotationFromAngle(yAngle);
        detachedModeSpriteRotator.rotation = rotation;
        foreach (PlayerCharacter playerMovement in partyManager.partyMembers) {
            playerMovement.SetDisplayRotation(rotation);
        }
        foreach (NPC npc in nonVoxelWorld.npcs) {
            npc.SetDisplayRotation(rotation);
        }
    }
}
