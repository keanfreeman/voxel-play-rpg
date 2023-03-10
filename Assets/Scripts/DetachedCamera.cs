using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelPlay;

public class DetachedCamera : MonoBehaviour
{
    [SerializeField] private Camera detachedCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private DetachedCameraBottom detachedCameraBottom;
    [SerializeField] private GameObject rotationObject;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private VoxelWorldManager voxelWorldManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private SpriteMovement spriteMovement;

    private const float SPEED_MULTIPLIER = 6.0f;
    private const float VOXEL_CHANGE_DISTANCE = 0.51f;
    private const float CURSOR_CENTER_SPEED = 1.5f;
    private const float ROTATION_SPEED = 100f;
    
    private Vector3Int currVoxel;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    // move to player, become visible, etc.
    public void BecomeActive() {
        transform.position = playerMovement.currVoxel;
        currVoxel = playerMovement.currVoxel;

        gameObject.SetActive(true);
        voxelWorldManager.environment.cameraMain = detachedCamera;

        detachedCameraBottom.MoveImmediate(currVoxel);
        detachedCameraBottom.SetVisibility(true);
        voxelWorldManager.environment.seeThroughTarget 
            = detachedCameraBottom.seeThroughTarget;
    }

    // become invisible and take up no resources
    public void BecomeInactive() {
        voxelWorldManager.environment.cameraMain = playerMovement.playerCamera;
        voxelWorldManager.environment.seeThroughTarget = playerMovement.voxelHideTarget;
        gameObject.SetActive(false);
        detachedCameraBottom.SetVisibility(false);
    }

    public void HandleFrame() {
        bool movedVoxels = UpdateCurrVoxel();
        if (movedVoxels) {
            detachedCameraBottom.MoveAnimated(currVoxel);
        }
        RotateCursor();
        MoveCursor();

        if (inputManager.WasSelectTriggered()) {
            Vector3Int start = playerMovement.currVoxel;
            Node startNode = new Node(start);
            Node endNode = new Node(currVoxel);
            Pathfinder pathfinder = new Pathfinder(voxelWorldManager.environment,
                spriteMovement);
            List<Vector3Int> path = pathfinder.FindPath(startNode, endNode);
            Debug.Log(path.Count);
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
        float rotationRadians = Mathf.Deg2Rad * rotationObject.transform.eulerAngles.y * -1;
        Vector2 moveWithRotation = VectorMath.rotate(move, rotationRadians);
        float moveSpeed = Time.deltaTime * SPEED_MULTIPLIER;
        transform.Translate(moveWithRotation.x * moveSpeed, verticalMove * moveSpeed,
            moveWithRotation.y * moveSpeed);
    }

    private void RotateCursor() {
        float rotation = inputManager.GetDetachedRotation();
        rotationObject.transform.Rotate(Vector3.up,
            rotation * Time.deltaTime * ROTATION_SPEED);
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
}
