using InstantiatedEntity;
using NonVoxel;
using NonVoxelEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VoxelPlay;

public class NonVoxelManager : MonoBehaviour
{
    [SerializeField] InputManager inputManager;
    [SerializeField] EnvironmentSceneManager environmentSceneManager;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] RandomManager randomManager;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] OrderManager orderManager;

    [SerializeField] GameObject opossumPrefab;
    [SerializeField] GameObject sceneExitPrefab;
    [SerializeField] GameObject bedPrefab;
    [SerializeField] GameObject detachedCameraPrefab;
    [SerializeField] GameObject storyEventCubePrefab;
    [SerializeField] TextAsset getAttention;
    [SerializeField] TextAsset friendDialogue;

    private NonVoxelInitialization nonVoxelInitialization;

    private List<Spawnable> nonVoxelEntities;

    private void Awake() {
        nonVoxelInitialization = new NonVoxelInitialization(playerPrefab,
            opossumPrefab, sceneExitPrefab, bedPrefab, storyEventCubePrefab,
            getAttention, friendDialogue);
    }

    public void DestroyEntities() {
        nonVoxelWorld.DestroyEntities();
    }

    public void SetUpEntities(int environmentIndex) {
        nonVoxelEntities = nonVoxelInitialization.GetEnvEntities(environmentIndex);
        InitCreaturesAndWorld();
    }

    private void InitCreaturesAndWorld() {
        Dictionary<Guid, HashSet<NPCBehavior>> battleGroups = 
            new Dictionary<Guid, HashSet<NPCBehavior>>();
        foreach (Spawnable nonVoxelSpawnable in nonVoxelEntities) {
            if (nonVoxelSpawnable.GetType() == typeof(Party)) {
                Party party = (Party)nonVoxelSpawnable;

                bool partyAlreadySpawned = false;
                if (partyManager.partyMembers.Count > 0) {
                    partyAlreadySpawned = true;
                }
                foreach (PlayerCharacter playerCharacter in party.members) {
                    PlayerMovement playerMovement;
                    if (partyAlreadySpawned) {
                        playerMovement = partyManager.GetPlayerMovement(playerCharacter);
                        playerMovement.transform.SetPositionAndRotation(playerCharacter.startPosition,
                            Quaternion.identity);
                    }
                    else {
                        GameObject playerObject = Instantiate(playerCharacter.entityDisplay.prefab,
                            playerCharacter.startPosition, Quaternion.identity);

                        playerMovement = playerObject.GetComponent<PlayerMovement>();
                        playerMovement.Init(spriteMovement, playerCharacter, nonVoxelWorld, cameraManager,
                            partyManager);
                        playerMovement.spriteLibrary.spriteLibraryAsset = 
                            playerCharacter.entityDisplay.spriteLibraryAsset;
                        partyManager.partyMembers.Add(playerMovement);

                        if (playerCharacter == party.mainCharacter) {
                            partyManager.SetMainCharacter(playerMovement);
                        }
                    }
                    playerMovement.SetCurrPositions(playerCharacter);
                    nonVoxelWorld.AddEntity(playerMovement);
                }

                continue;
            }

            Entity entity = (Entity)nonVoxelSpawnable;
            GameObject gameObject = Instantiate(entity.entityDisplay.prefab,
                entity.startPosition, Quaternion.identity);

            if (nonVoxelSpawnable.GetType() == typeof(NPC)) {
                NPC npcInfo = (NPC)nonVoxelSpawnable;
                NPCBehavior npcBehavior = gameObject.GetComponent<NPCBehavior>();
                npcBehavior.Init(nonVoxelWorld, spriteMovement, randomManager.rng, npcInfo,
                    cameraManager, partyManager, gameStateManager);
                nonVoxelWorld.npcs.Add(npcBehavior);

                if (npcInfo.battleGroup != null) {
                    Guid battleGroupID = npcInfo.battleGroup.groupID;
                    if (!battleGroups.ContainsKey(battleGroupID)) {
                        battleGroups[battleGroupID] = new HashSet<NPCBehavior>();
                    }
                    battleGroups[battleGroupID].Add(npcBehavior);
                    npcBehavior.teammates = battleGroups[battleGroupID];
                }
                nonVoxelWorld.AddEntity(npcBehavior);
            }

            if (nonVoxelSpawnable.GetType() == typeof(NonVoxelObject)) {
                NonVoxelObject nonVoxelObject = (NonVoxelObject)nonVoxelSpawnable;
                InstantiatedNVObject instantiatedNVObject = gameObject.GetComponent<InstantiatedNVObject>();
                instantiatedNVObject.Init(nonVoxelWorld, nonVoxelObject);
                nonVoxelWorld.AddEntity(instantiatedNVObject);
            }

            if (nonVoxelSpawnable.GetType() == typeof(NonVoxelEntity.StoryEventCube)) {
                NonVoxelEntity.StoryEventCube definition = (NonVoxelEntity.StoryEventCube)nonVoxelSpawnable;
                InstantiatedEntity.StoryEventCube storyEventCube 
                    = gameObject.GetComponent<InstantiatedEntity.StoryEventCube>();
                storyEventCube.Init(definition, orderManager);
            }

            if (nonVoxelSpawnable.GetType() == typeof(SceneExitCube)) {
                SceneExitCube sceneExitCube = (SceneExitCube)nonVoxelSpawnable;
                SceneExit sceneExitComponent = gameObject.GetComponent<SceneExit>();
                sceneExitComponent.Init(environmentSceneManager, sceneExitCube.destination, sceneExitCube);
            }
        }
    }
}
