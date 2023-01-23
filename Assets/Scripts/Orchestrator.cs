using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;
using NonVoxel;
using System;

public class Orchestrator : MonoBehaviour
{
    public GameObject vpEnvironment;
    public GameObject vpController;
    public GameObject playerPrefab;
    public GameObject opossumPrefab;
    public GameObject dialogBox;

    private GameObject playerInstance;

    NonVoxelWorld nonVoxelWorld = new NonVoxelWorld();
    System.Random rng = new System.Random();
    SpriteMovement spriteMovement;
    VoxelPlayEnvironment voxelPlayEnvironment;
    private PlayerInputContextHandler playerInputContextHandler;
    private Dialogue dialogue;
    private VoxelWorld voxelWorld;
    private InteractableVoxels interactableVoxels;

    void Start()
    {
        dialogue = dialogBox.GetComponent<Dialogue>();
        voxelPlayEnvironment = VoxelPlayEnvironment.instance;
        spriteMovement = new SpriteMovement(voxelPlayEnvironment);

        InitCreaturesAndWorld();
        PlayerMovement playerMovement = vpController.GetComponent<PlayerMovement>();
        playerMovement.spriteContainer = playerInstance;
        playerMovement.nonVoxelWorld = nonVoxelWorld;
        playerMovement.spriteMovement = spriteMovement;
        playerMovement.dialogue = dialogue;
        vpController.GetComponent<PlayerMovement>().enabled = true;

        vpController.GetComponent<VoxelPlayPlayer>().enabled = true;

        vpController.GetComponent<VoxelPlayFirstPersonController>().enabled = true;

        interactableVoxels = gameObject.GetComponent<InteractableVoxels>();
        voxelWorld = new VoxelWorld(voxelPlayEnvironment, interactableVoxels);
        playerInputContextHandler = new PlayerInputContextHandler(playerMovement, nonVoxelWorld, dialogue, voxelWorld);
    }

    void Update() {
        playerInputContextHandler.HandlePlayerInput();
    }

    private void InitCreaturesAndWorld() {

        Vector3Int playerStartPosition = new Vector3Int(523, 50, 246);
        GameObject playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        this.playerInstance = playerInstance;
        nonVoxelWorld.SetPosition(playerInstance, playerStartPosition);

        createNPC(new Vector3Int(527, 53, 247));
        createNPC(new Vector3Int(521, 50, 246));
    }

    private void createNPC(Vector3Int startPosition) {
        GameObject opossumInstance = Instantiate(opossumPrefab, startPosition, Quaternion.identity);
        NPCBehavior npcBehavior = opossumInstance.GetComponent<NPCBehavior>();
        npcBehavior.nonVoxelWorld = nonVoxelWorld;
        npcBehavior.spriteMovement = spriteMovement;
        npcBehavior.environment = voxelPlayEnvironment;
        npcBehavior.rng = rng;
        nonVoxelWorld.SetPosition(opossumInstance, startPosition);
    }
}
