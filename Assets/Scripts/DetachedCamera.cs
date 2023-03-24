using InstantiatedEntity;
using Nito.Collections;
using NonVoxel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VoxelPlay;

public class DetachedCamera : MonoBehaviour
{
    [SerializeField] AudioListener audioListener;
    [SerializeField] DetachedCameraBottom detachedCameraBottom;
    [SerializeField] InputManager inputManager;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] MovementManager movementManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] SpriteRenderer detachedModeSprite;

    // sprites
    [SerializeField] Sprite grabIcon;
    [SerializeField] Sprite meleeAttackIcon;
    [SerializeField] Sprite rangedAttackIcon;
    [SerializeField] Sprite traverseIcon;

    public Vector3Int currVoxel { get; private set; }

    private const float SPEED_MULTIPLIER = 6.0f;
    private const float VOXEL_CHANGE_DISTANCE = 0.51f;
    private const float CURSOR_CENTER_SPEED = 1.5f;
    
    private Pathfinder pathfinder;
    private Traveller currTraveller;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
        pathfinder = new Pathfinder(spriteMovement);
        currTraveller = playerMovement;
    }

    // move to player, become visible, etc.
    public void BecomeActive() {
        transform.position = playerMovement.currVoxel;
        currVoxel = playerMovement.currVoxel;

        enabled = true;
        gameObject.SetActive(true);

        detachedCameraBottom.MoveImmediate(currVoxel);
        detachedCameraBottom.SetVisibility(true);
    }

    // become invisible and take up no resources
    public void BecomeInactive() {
        enabled = false;
        gameObject.SetActive(false);
        detachedCameraBottom.SetVisibility(false);
    }

    private void Update() {
        bool movedVoxels = UpdateCurrVoxel();
        if (movedVoxels) {
            detachedCameraBottom.MoveAnimated(currVoxel);
            UpdateCursorType();
        }
        MoveCursor();
        HandleSelect();
    }

    private void UpdateCursorType() {
        if (nonVoxelWorld.IsPositionOccupied(currVoxel)) {
            if (gameStateManager.controlState == ControlState.COMBAT 
                    && currVoxel != playerMovement.currVoxel) {
                detachedModeSprite.sprite = meleeAttackIcon;
            }
            else {
                detachedModeSprite.sprite = grabIcon;
            }
            return;
        }

        if (spriteMovement.IsReachablePosition(currVoxel, true)) {
            detachedModeSprite.sprite = traverseIcon;
            return;
        }
    }

    private void MoveCursor() {
        float verticalMove = inputManager.GetDetachedVerticalMove();
        Vector2 move = inputManager.GetDetachedMove();
        if (move == Vector2.zero && verticalMove == 0) {
            // no need to move if cursor is close enough to end
            Vector3 distance = transform.position - currVoxel;
            if (Mathf.Abs(distance.x) < 0.05 && Mathf.Abs(distance.x) < 0.05
                && Mathf.Abs(distance.x) < 0.05) {
                return;
            }
            float fractionToMove = Time.deltaTime * CURSOR_CENTER_SPEED;
            transform.position = Vector3.MoveTowards(transform.position, currVoxel,
                Mathf.Min(fractionToMove, 1f));
            return;
        }

        // move direction depends on current camera rotation
        Transform mainCameraTargetTransform = cameraManager.GetMainCameraTarget().transform;
        float rotationRadians = Mathf.Deg2Rad * mainCameraTargetTransform.eulerAngles.y * -1;
        Vector2 moveWithRotation = VectorMath.rotate(move, rotationRadians);
        float moveSpeed = Time.deltaTime * SPEED_MULTIPLIER;
        transform.Translate(moveWithRotation.x * moveSpeed, verticalMove * moveSpeed,
            moveWithRotation.y * moveSpeed);
    }

    // returns true if the voxel was changed
    private bool UpdateCurrVoxel() {
        bool movedVoxels = false;
        Vector3 deviation = currVoxel - transform.position;
        if (Mathf.Abs(deviation.x) > VOXEL_CHANGE_DISTANCE) {
            movedVoxels = true;
            currVoxel += deviation.x > 0 ? Vector3Int.left : Vector3Int.right;
        }
        if (Mathf.Abs(deviation.y) > VOXEL_CHANGE_DISTANCE) {
            movedVoxels = true;
            currVoxel += deviation.y > 0 ? Vector3Int.down : Vector3Int.up;
        }
        if (Mathf.Abs(deviation.z) > VOXEL_CHANGE_DISTANCE) {
            movedVoxels = true;
            currVoxel += deviation.z > 0 ? Vector3Int.back : Vector3Int.forward;
        }
        return movedVoxels;
    }

    private void HandleSelect() {
        if (gameStateManager.controlState != ControlState.DETACHED) {
            return;
        }

        if (!inputManager.WasSelectTriggered()) {
            return;
        }
        if (nonVoxelWorld.IsPositionOccupied(currVoxel)) {
            // select the entity
            MonoBehaviour behavior = nonVoxelWorld.GetNVEFromPosition(currVoxel);
            Traveller traveller = behavior.GetComponent<Traveller>();
            if (traveller != null) {
                currTraveller = traveller;
                Debug.Log($"Selected traveller for the detached camera at {traveller.currVoxel}");
            }
        }
        else {
            // move currently selected to target
            Deque<Vector3Int> path = pathfinder.FindPath(currTraveller.currVoxel, currVoxel, true);
            movementManager.MoveAlongPath(currTraveller, path);
        }
    }
}
