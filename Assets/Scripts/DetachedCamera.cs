using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelPlay;

public class DetachedCamera : MonoBehaviour
{
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private DetachedCameraBottom detachedCameraBottom;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private SpriteMovement spriteMovement;
    [SerializeField] private PathVisualizer pathVisualizer;
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private NonVoxelWorld nonVoxelWorld;
    [SerializeField] private CameraManager cameraManager;

    private const float SPEED_MULTIPLIER = 6.0f;
    private const float VOXEL_CHANGE_DISTANCE = 0.51f;
    private const float CURSOR_CENTER_SPEED = 1.5f;
    
    private Vector3Int currVoxel;
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

        gameObject.SetActive(true);

        detachedCameraBottom.MoveImmediate(currVoxel);
        detachedCameraBottom.SetVisibility(true);
    }

    // become invisible and take up no resources
    public void BecomeInactive() {
        gameObject.SetActive(false);
        detachedCameraBottom.SetVisibility(false);
    }

    public void HandleFrame() {
        bool movedVoxels = UpdateCurrVoxel();
        if (movedVoxels) {
            detachedCameraBottom.MoveAnimated(currVoxel);
        }
        MoveCursor();
        HandleSelect();
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
        if (!inputManager.WasSelectTriggered()) {
            return;
        }
        if (nonVoxelWorld.IsPositionOccupied(currVoxel)) {
            // select the entity
            GameObject gameObject = nonVoxelWorld.GetObjectFromPosition(currVoxel);
            Traveller traveller = gameObject.GetComponent<Traveller>();
            if (traveller != null) {
                currTraveller = traveller;
                Debug.Log($"Selected traveller for the detached camera at {traveller.currVoxel}");
            }
        }
        else {
            // move currently selected to target
            List<Vector3Int> path = pathfinder.FindPath(currTraveller.currVoxel, currVoxel);
            pathVisualizer.DrawPath(path);
            movementManager.MoveAlongPath(currTraveller, path);
        }
    }
}
