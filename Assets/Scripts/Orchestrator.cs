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

    // todo more complexity
    [Header("Ink JSON")]
    [SerializeField] private TextAsset dialogueJSON;

    private GameObject playerInstance;

    NonVoxelWorld nonVoxelWorld = new NonVoxelWorld();
    System.Random rng = new System.Random();
    SpriteMovement spriteMovement;
    VoxelPlayEnvironment voxelPlayEnvironment;
    private PlayerInputContextHandler playerInputContextHandler;
    private Dialogue dialogue;
    private VoxelWorld voxelWorld;
    private InteractableVoxels interactableVoxels;
    private PlayerInputActions playerInputActions;
    private InputManager inputManager;


    void Start()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        inputManager = new InputManager(playerInputActions);

        dialogue = dialogBox.GetComponent<Dialogue>();
        voxelPlayEnvironment = VoxelPlayEnvironment.instance;
        spriteMovement = new SpriteMovement(voxelPlayEnvironment);

        InitCreaturesAndWorld();
        PlayerMovement playerMovement = vpController.GetComponent<PlayerMovement>();
        playerMovement.spriteContainer = playerInstance;
        playerMovement.nonVoxelWorld = nonVoxelWorld;
        playerMovement.spriteMovement = spriteMovement;
        playerMovement.dialogue = dialogue;
        playerMovement.inputManager = inputManager;
        vpController.GetComponent<PlayerMovement>().enabled = true;

        vpController.GetComponent<VoxelPlayPlayer>().enabled = true;

        vpController.GetComponent<VoxelPlayFirstPersonController>().enabled = true;

        interactableVoxels = gameObject.GetComponent<InteractableVoxels>();
        voxelWorld = new VoxelWorld(voxelPlayEnvironment, interactableVoxels);
        playerInputContextHandler = new PlayerInputContextHandler(playerMovement, nonVoxelWorld, dialogue, voxelWorld,
            inputManager, dialogueJSON);
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
