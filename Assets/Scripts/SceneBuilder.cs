using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VoxelPlay;

public class SceneBuilder : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject opossumPrefab;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Vector3Int playerStartPosition;

    public VoxelPlayEnvironment vpEnvironment;
    public SceneChanger parentSceneChanger;

    private GameObject playerInstance;
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

    public void Start() {
        Debug.Log(gameObject.scene.buildIndex);
        vpEnvironment = gameObject.GetComponent<VoxelPlayEnvironment>();
        VoxelPlayEnvironment.RegisterEnvironment(gameObject.scene.buildIndex, vpEnvironment);
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
        if (vpEnvironment == null || !vpEnvironment.initialized
            || SceneManager.GetActiveScene() != gameObject.scene) {
            return;
        }

        if (Input.GetKeyUp(KeyCode.J)) {
            parentSceneChanger.SetActiveScene();
        }

        playerInputContextHandler.HandlePlayerInput();
    }

    private PlayerMovement InitPlayerMovement() {
        PlayerMovement temp = GetComponent<PlayerMovement>();
        temp.spriteContainer = playerInstance;
        temp.nonVoxelWorld = nonVoxelWorld;
        temp.spriteMovement = spriteMovement;
        temp.inputManager = inputManager;
        temp.spriteRenderer = playerInstance.GetComponentInChildren<SpriteRenderer>();
        temp.animator = playerInstance.GetComponentInChildren<Animator>();
        temp.spriteChildTransform = playerInstance.GetComponentInChildren<Transform>();
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
