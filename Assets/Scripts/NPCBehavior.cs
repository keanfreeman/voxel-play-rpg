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
    public VoxelPlayEnvironment environment;
    public NonVoxelWorld nonVoxelWorld;
    public SpriteMovement spriteMovement;

    private float lastMoveTime = 0;

    void Update()
    {
        if (Time.time - lastMoveTime < NPC_MIN_IDLE_TIME) {
            return;
        }
        lastMoveTime = Time.time;

        Vector3Int currPosition = nonVoxelWorld.GetPosition(gameObject);
        Vector3Int newPosition = nonVoxelWorld.GetPosition(gameObject)
            + GetRandomOneTileMovement();
        Vector3Int? actualCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
            newPosition, currPosition);
        if (!actualCoordinate.HasValue) {
            Debug.Log("NPC tried to move in an invalid way.");
            return;
        }
        Vector3Int destinationCoordinate = actualCoordinate.GetValueOrDefault();

        if (!nonVoxelWorld.IsPositionOccupied(destinationCoordinate)
            && environment.GetVoxel(destinationCoordinate).isEmpty) {
            MoveSprite(destinationCoordinate);
        }
    }

    public void MoveSprite(Vector3Int position) {
        nonVoxelWorld.SetPosition(gameObject, position);
        transform.position = position;
    }

    public Vector3Int GetRandomOneTileMovement() {
        bool isX = rng.Next(0, 2) == 0 ? true : false;
        if (isX) {
            return rng.Next(0, 2) == 0 ? Vector3Int.left : Vector3Int.right;
        }
        return rng.Next(0, 2) == 0 ? Vector3Int.forward : Vector3Int.back;
    }
}
