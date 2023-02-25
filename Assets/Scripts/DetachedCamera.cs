using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

public class DetachedCamera : MonoBehaviour
{
    [SerializeField] private Camera detachedCamera;
    [SerializeField] private AudioListener audioListener;
    
    private VoxelPlayEnvironment vpEnvironment;
    private InputManager inputManager;

    private const float SPEED_MULTIPLIER = 3.0f;

    public void Init(Vector3Int startPosition, VoxelPlayEnvironment vpEnvironment,
        InputManager inputManager) {
        this.vpEnvironment = vpEnvironment;
        this.inputManager = inputManager;
        transform.position = startPosition;
        enabled = true;
    }

    public void HandleFrame() {
        Vector2 move = inputManager.GetDetachedMove();
        float verticalMove = inputManager.GetDetachedVerticalMove();
        float factor = Time.deltaTime * SPEED_MULTIPLIER;
        transform.Translate(move.x * factor, verticalMove * factor, move.y * factor);
    }
}
