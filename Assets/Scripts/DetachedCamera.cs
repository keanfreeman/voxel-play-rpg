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

    [SerializeField] private float SPEED_MULTIPLIER = 6.0f;
    [SerializeField] private float VOXEL_CHANGE_DISTANCE = 0.51f;
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
    }

    public void HandleFrame() {
        Vector2 move = inputManager.GetDetachedMove();
        float verticalMove = inputManager.GetDetachedVerticalMove();
        float factor = Time.deltaTime * SPEED_MULTIPLIER;
        transform.Translate(move.x * factor, verticalMove * factor, move.y * factor);

        // move highlighter to new voxel if moved too much
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
