using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelPlay;

// moves around randomly
public class NPCBehavior : MonoBehaviour
{
    private const float NPC_MIN_IDLE_TIME = 1;
    private const float NPC_MAX_IDLE_TIME = 5;

    System.Random rng = new System.Random();
    VoxelPlayEnvironment environment;
    public NonVoxelWorld nonVoxelWorld;

    private float lastMoveTime = 0;

    private void Start() {
        environment = VoxelPlayEnvironment.instance;
    }

    void Update()
    {
        if (Time.time - lastMoveTime < NPC_MIN_IDLE_TIME) {
            return;
        }
        lastMoveTime = Time.time;

        Vector3Int newPosition = nonVoxelWorld.GetPosition(gameObject)
            + new Vector3Int(rng.Next(-1, 2), 0, rng.Next(-1, 2));
        if (!nonVoxelWorld.IsPositionOccupied(newPosition)
            && environment.GetVoxel(newPosition).isEmpty) {
            MoveSprite(newPosition);
        }
    }

    public void MoveSprite(Vector3Int position) {
        nonVoxelWorld.SetPosition(gameObject, position);
        transform.position = position;
    }
}
