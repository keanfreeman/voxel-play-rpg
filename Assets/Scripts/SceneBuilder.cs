using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
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
    private Camera dummyCamera;
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
        VoxelPlayEnvironment.RegisterEnvironment(gameObject.scene.buildIndex, vpEnvironment);

        dummyCamera = gameObject.AddComponent<Camera>();
        dummyCamera.enabled = false;
        if (gameObject.scene.buildIndex == 1) {
            playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
            OnMadeActive(playerInstance);
        }
        else {
            vpEnvironment.cameraMain = dummyCamera;
        }
    }

    public void Update() {
        if (Input.GetKeyUp(KeyCode.G)) {
            Debug.Log("Debug key pressed.");
        }
        if (Input.GetKeyUp(KeyCode.H)) {
            Debug.Log("Second debug key pressed.");
            if (gameObject.scene.buildIndex == 2) {
                vpEnvironment.enabled = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.J) && gameObject.scene.buildIndex == 1) {
            Debug.Log(vpEnvironment.world);
            vpEnvironment.world = Resources.Load<WorldDefinition>("WorldDefinition2");
            Debug.Log(vpEnvironment.world);
            vpEnvironment.loadSavedGame = false;
            vpEnvironment.ReloadWorld();
            //Debug.Log($"script scene: {gameObject.scene.buildIndex}");
            //vpEnvironment.enabled = false;
            //vpEnvironment.cameraMain = dummyCamera;
            //for (int i = 0; i < transform.childCount; i++) {
            //    transform.GetChild(i).gameObject.SetActive(false);
            //}
            //parentSceneChanger.SetActiveScene(playerInstance);
        }

        if (Input.GetKeyUp(KeyCode.K) && gameObject.scene.buildIndex == 2) {
            Debug.Log($"script scene: {gameObject.scene.buildIndex}");
            vpEnvironment.enabled = false;
            vpEnvironment.cameraMain = dummyCamera;
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            parentSceneChanger.SetActiveScene(playerInstance);
        }

        if (vpEnvironment == null || !vpEnvironment.initialized || !vpEnvironment.enabled
            || SceneManager.GetActiveScene() != gameObject.scene) {
            return;
        }

        playerInputContextHandler.HandlePlayerInput();
    }

    // can create resources now that it's the active scene
    public void OnMadeActive(GameObject playerObject) {
        if (dummyCamera != null) {
            Destroy(dummyCamera);
        }

        playerInstance = playerObject;
        vpEnvironment.cameraMain = playerInstance.GetComponentInChildren<Camera>();
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

        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(true);
        }
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
        playerInstance.transform.position = playerStartPosition;
        nonVoxelWorld.SetPosition(playerInstance, playerStartPosition);

        if (gameObject.scene.buildIndex == 1) {
            //createNPC(new Vector3Int(527, 53, 247));
            //createNPC(new Vector3Int(521, 50, 246));
        }
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
