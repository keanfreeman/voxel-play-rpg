using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using VoxelPlay;
using NonVoxel;
using System;

public class Orchestrator : MonoBehaviour
{
    public GameObject vpEnvironment;
    public GameObject vpController;
    public GameObject playerPrefab;
    public GameObject opossumPrefab;
    [Header("UI Document")] public UIDocument uiDocument;

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
    private ObjectInkMapping objectInkMapping;

    void Start()
    {
        objectInkMapping = GetComponent<ObjectInkMapping>();

        dialogue = uiDocument.GetComponent<Dialogue>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        inputManager = new InputManager(playerInputActions);

        voxelPlayEnvironment = VoxelPlayEnvironment.instance;
        spriteMovement = new SpriteMovement(voxelPlayEnvironment);

        InitCreaturesAndWorld();
        PlayerMovement playerMovement = vpController.GetComponent<PlayerMovement>();
        playerMovement.spriteContainer = playerInstance;
        playerMovement.nonVoxelWorld = nonVoxelWorld;
        playerMovement.spriteMovement = spriteMovement;
        playerMovement.inputManager = inputManager;
        vpController.GetComponent<PlayerMovement>().enabled = true;

        vpController.GetComponent<VoxelPlayPlayer>().enabled = true;

        vpController.GetComponent<VoxelPlayFirstPersonController>().enabled = true;

        interactableVoxels = gameObject.GetComponent<InteractableVoxels>();
        voxelWorld = new VoxelWorld(voxelPlayEnvironment, interactableVoxels);
        playerInputContextHandler = new PlayerInputContextHandler(playerMovement, nonVoxelWorld, dialogue, voxelWorld,
            inputManager, objectInkMapping);
    }

    void Update() {
        playerInputContextHandler.HandlePlayerInput();
    }

    private void InitCreaturesAndWorld() {

        Vector3Int playerStartPosition = new Vector3Int(523, 50, 246);
        GameObject playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        playerInstance.tag = "player";
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
