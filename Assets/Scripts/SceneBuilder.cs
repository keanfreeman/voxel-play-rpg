using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;
using VoxelPlay;

public class SceneBuilder : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject opossumPrefab;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Vector3Int playerStartPosition;

    private GameObject playerInstance;
    private VoxelPlayEnvironment vpEnvironment;
    private NonVoxelWorld nonVoxelWorld = new NonVoxelWorld();
    private SpriteMovement spriteMovement;
    private System.Random rng = new System.Random();
    private PlayerInputContextHandler playerInputContextHandler;
    private PlayerMovement playerMovement;
    private Dialogue dialogue;
    private VoxelWorld voxelWorld;
    private InteractableVoxels interactableVoxels;
    private PlayerInputActions playerInputActions;
    private InputManager inputManager;
    private ObjectInkMapping objectInkMapping;

    public void Awake() {
        vpEnvironment = gameObject.GetComponent<VoxelPlayEnvironment>();
        vpEnvironment.enabled = true;
        
        spriteMovement = new SpriteMovement(vpEnvironment);
        InitCreaturesAndWorld();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        inputManager = new InputManager(playerInputActions);
        playerMovement = InitPlayerMovement();
        dialogue = uiDocument.GetComponent<Dialogue>();
        interactableVoxels = GetComponent<InteractableVoxels>();
        voxelWorld = new VoxelWorld(vpEnvironment, interactableVoxels);
        objectInkMapping = GetComponent<ObjectInkMapping>();
        playerInputContextHandler = new PlayerInputContextHandler(playerMovement, nonVoxelWorld, dialogue, voxelWorld,
            inputManager, objectInkMapping);
    }

    public void Update() {
        playerInputContextHandler.HandlePlayerInput();
    }

    private PlayerMovement InitPlayerMovement() {
        PlayerMovement temp = GetComponent<PlayerMovement>();
        temp.spriteContainer = playerInstance;
        temp.nonVoxelWorld = nonVoxelWorld;
        temp.spriteMovement = spriteMovement;
        temp.inputManager = inputManager;
        return temp;
    }

    private void InitCreaturesAndWorld() {
        playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        nonVoxelWorld.SetPosition(playerInstance, playerStartPosition);

        createNPC(new Vector3Int(527, 53, 247));
        createNPC(new Vector3Int(521, 50, 246));
    }

    private void createNPC(Vector3Int startPosition) {
        GameObject opossumInstance = Instantiate(opossumPrefab, startPosition, Quaternion.identity);
        NPCBehavior npcBehavior = opossumInstance.GetComponent<NPCBehavior>();
        npcBehavior.nonVoxelWorld = nonVoxelWorld;
        npcBehavior.spriteMovement = spriteMovement;
        npcBehavior.environment = vpEnvironment;
        npcBehavior.rng = rng;
        nonVoxelWorld.SetPosition(opossumInstance, startPosition);
    }
}
