using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public class DetachedCamera : MonoBehaviour
{
    [SerializeField] private Camera detachedCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private GameObject detachedCameraBottomPrefab;
    
    private VoxelPlayEnvironment vpEnvironment;
    private InputManager inputManager;
    private GameObject detachedCameraBottom;
    private DetachedCameraBottom detachedCameraBottomComponent;

    private const float SPEED_MULTIPLIER = 6.0f;
    private const float VOXEL_CHANGE_DISTANCE = 0.51f;
    private const float CURSOR_CENTER_SPEED = 1f;
    
    private Vector3Int currVoxel;

    public void Init(Vector3Int startPosition, VoxelPlayEnvironment vpEnvironment,
        InputManager inputManager) {
        this.vpEnvironment = vpEnvironment;
        this.inputManager = inputManager;
        transform.position = startPosition;
        currVoxel = startPosition;

        detachedCameraBottom = Instantiate(detachedCameraBottomPrefab, startPosition,
            Quaternion.identity);
        detachedCameraBottomComponent = detachedCameraBottom
            .GetComponent<DetachedCameraBottom>();
        detachedCameraBottomComponent.Init(vpEnvironment);
    }

    public void HandleFrame() {
        UpdateHighlighterPosition();
        MoveCursor();
    }

    private void MoveCursor() {
        float verticalMove = inputManager.GetDetachedVerticalMove();
        Vector2 move = inputManager.GetDetachedMove();
        if (move == Vector2.zero && verticalMove == 0) {
            float fractionToMove = Time.deltaTime * CURSOR_CENTER_SPEED;
            transform.position = Vector3.MoveTowards(transform.position, currVoxel,
                Mathf.Min(fractionToMove, 1f));

        }
        else {
            float moveSpeed = Time.deltaTime * SPEED_MULTIPLIER;
            transform.Translate(move.x * moveSpeed, verticalMove * moveSpeed,
                move.y * moveSpeed);
        }
    }

    private void UpdateHighlighterPosition() {
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

        if (movedVoxels) {
            detachedCameraBottomComponent.MoveTo(currVoxel);
        }
    }
}
