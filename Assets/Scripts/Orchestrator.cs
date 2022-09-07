using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;
using NonVoxel;

public class Orchestrator : MonoBehaviour
{
    public GameObject vpEnvironment;
    public GameObject vpController;
    public GameObject playerPrefab;
    public GameObject opossumPrefab;

    private GameObject playerInstance;

    NonVoxelWorld nonVoxelWorld = new NonVoxelWorld();

    void Start()
    {
        InitCreaturesAndWorld();
        SpriteMovement spriteMovement = vpController.GetComponent<SpriteMovement>();
        spriteMovement.spriteContainer = playerInstance;
        spriteMovement.nonVoxelWorld = nonVoxelWorld;
        vpController.GetComponent<SpriteMovement>().enabled = true;

        vpController.GetComponent<VoxelPlayPlayer>().enabled = true;

        vpController.GetComponent<VoxelPlayFirstPersonController>().enabled = true;
    }

    private void InitCreaturesAndWorld() {
        Vector3Int playerStartPosition = new Vector3Int(523, 50, 246);
        GameObject playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        this.playerInstance = playerInstance;
        nonVoxelWorld.SetPosition(playerInstance, playerStartPosition);
        
        Vector3Int opossumStartPosition = new Vector3Int(521, 50, 246);
        GameObject opossumInstance = Instantiate(opossumPrefab, opossumStartPosition, Quaternion.identity);
        opossumInstance.GetComponent<NPCBehavior>().nonVoxelWorld = nonVoxelWorld;
        nonVoxelWorld.SetPosition(opossumInstance, opossumStartPosition);
    }
}
