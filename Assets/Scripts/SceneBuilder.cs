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
    [SerializeField] private GameObject sceneExitPrefab;
    [SerializeField] private Vector3Int playerStartPosition;

    public VoxelPlayEnvironment vpEnvironment;
    public SceneChanger parentSceneChanger;
    public GameObject playerInstance;

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

    private GameObject uiDocument;
    private bool initialized = false;

    public void Init(GameObject uiDocument) {
        this.uiDocument = uiDocument;
        initialized = true;
    }

    public void Start() {
        vpEnvironment = gameObject.GetComponent<VoxelPlayEnvironment>();
        VoxelPlayEnvironment.RegisterEnvironment(gameObject.scene.buildIndex, vpEnvironment);
        vpEnvironment.enabled = true;

        dummyCamera = gameObject.AddComponent<Camera>();
        dummyCamera.enabled = false;
        if (gameObject.scene.buildIndex == 1) {
            playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
            OnMadeActive(playerInstance);
        }
        else {
            vpEnvironment.cameraMain = dummyCamera;
        }

        GameObject sceneExitCube = Instantiate(sceneExitPrefab, this.transform);
        sceneExitCube.transform.SetPositionAndRotation(playerStartPosition + Vector3Int.forward * 3, Quaternion.identity);
        SceneExitCube script = sceneExitCube.GetComponent<SceneExitCube>();
        script.Init(parentSceneChanger);
    }

    public void Update() {
        if (Input.GetKeyUp(KeyCode.G) && SceneManager.GetActiveScene().buildIndex == gameObject.scene.buildIndex) {
            vpEnvironment.Redraw();
        }

        if (Input.GetKeyUp(KeyCode.H) && SceneManager.GetActiveScene().buildIndex == gameObject.scene.buildIndex) {
            vpEnvironment.cameraMain = transform.Find("PlayerSpriteContainer").GetComponentInChildren<Camera>();
        }

        if (vpEnvironment == null || !vpEnvironment.initialized || !vpEnvironment.enabled
            || SceneManager.GetActiveScene() != gameObject.scene) {
            return;
        }

        if (initialized) {
            playerInputContextHandler.HandlePlayerInput();
        }
    }

    public void PrepareForSceneInactive() {
        vpEnvironment.cameraMain = dummyCamera;
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    // can create resources now that it's the active scene
    public void OnMadeActive(GameObject playerObject) {
        if (dummyCamera != null) {
            Destroy(dummyCamera);
        }

        playerInstance = playerObject;
        vpEnvironment.cameraMain = playerInstance.GetComponentInChildren<Camera>();

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
        if (!nonVoxelWorld.IsInWorld(playerInstance)) {
            playerInstance.transform.position = playerStartPosition;
            nonVoxelWorld.SetPosition(playerInstance, playerStartPosition);
        }
        else {
            playerInstance.transform.position = new Vector3Int(527, 50, 242);
            nonVoxelWorld.SetPosition(playerInstance, new Vector3Int(527, 50, 242));
        }

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
