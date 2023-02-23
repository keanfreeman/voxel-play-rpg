using NonVoxel;
using System;
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

    public VoxelPlayEnvironment vpEnvironment;
    public SceneChanger parentSceneChanger;
    public GameObject playerInstance;
    
    private Vector3Int playerStartPosition;
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

    private List<NonVoxelEntity> nonVoxelEntities;

    public void Init(GameObject uiDocument, Vector3Int playerStartPosition, GameObject playerInstance,
            List<NonVoxelEntity> nonVoxelEntities) {
        this.uiDocument = uiDocument;
        this.playerStartPosition = playerStartPosition;
        this.playerInstance = playerInstance;
        this.nonVoxelEntities = nonVoxelEntities;
        enabled = true;
    }

    public void Start() {
        vpEnvironment = gameObject.GetComponent<VoxelPlayEnvironment>();
        vpEnvironment.cameraMain = playerInstance.transform.GetChild(0).GetChild(0).GetComponent<Camera>();
        vpEnvironment.enabled = true;
        vpEnvironment.seeThroughTarget = playerInstance.transform.GetChild(1).gameObject;

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
        if (vpEnvironment == null || !vpEnvironment.initialized || !vpEnvironment.enabled
            || SceneManager.GetActiveScene() != gameObject.scene) {
            return;
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
        temp.spriteChildTransform = playerInstance.transform.GetChild(0);
        return temp;
    }

    private void InitCreaturesAndWorld() {
        nonVoxelWorld.SetPosition(playerInstance, playerStartPosition);

        Dictionary<Guid, HashSet<NPCBehavior>> battleGroups = 
            new Dictionary<Guid, HashSet<NPCBehavior>>();
        foreach (NonVoxelEntity nonVoxelEntity in nonVoxelEntities) {
            if (nonVoxelEntity.startPosition != playerStartPosition) {
                GameObject gameObject = Instantiate(nonVoxelEntity.prefab,
                    nonVoxelEntity.startPosition, Quaternion.identity);
                nonVoxelWorld.SetPosition(gameObject, nonVoxelEntity.startPosition);
                
                if (nonVoxelEntity.GetType() == typeof(SceneExitCube)) {
                    SceneExitCube sceneExitCube = (SceneExitCube)nonVoxelEntity;
                    SceneExit sceneExitComponent = gameObject.GetComponent<SceneExit>();
                    sceneExitComponent.Init(parentSceneChanger, sceneExitCube.destination);
                }

                if (nonVoxelEntity.GetType() == typeof(NPC)) {
                    NPC npcInfo = (NPC)nonVoxelEntity;
                    NPCBehavior npcBehavior = gameObject.GetComponent<NPCBehavior>();
                    npcBehavior.Init(nonVoxelWorld, spriteMovement, vpEnvironment, rng, npcInfo);
                    nonVoxelWorld.npcs.Add(npcBehavior);

                    Guid battleGroupID = npcInfo.battleGroup.groupID;
                    if (!battleGroups.ContainsKey(battleGroupID)) {
                        battleGroups[battleGroupID] = new HashSet<NPCBehavior>();
                    }
                    battleGroups[battleGroupID].Add(npcBehavior);
                    npcBehavior.teammates = battleGroups[battleGroupID];
                }
            }
        }
    }
}
