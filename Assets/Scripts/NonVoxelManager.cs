using Instantiated;
using NonVoxel;
using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VoxelPlay;
using static UnityEngine.EventSystems.EventTrigger;

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
        Dictionary<Guid, HashSet<Instantiated.NPC>> battleGroups = 
            new Dictionary<Guid, HashSet<Instantiated.NPC>>();
        foreach (Spawnable nonVoxelSpawnable in nonVoxelEntities) {
            if (nonVoxelSpawnable.GetType() == typeof(Party)) {
                Party party = (Party)nonVoxelSpawnable;

                bool partyAlreadySpawned = false;
                if (partyManager.partyMembers.Count > 0) {
                    partyAlreadySpawned = true;
                }
                foreach (EntityDefinition.PlayerCharacter playerCharacter in party.members) {
                    Instantiated.PlayerCharacter playerMovement;
                    if (partyAlreadySpawned) {
                        playerMovement = partyManager.GetPlayerMovement(playerCharacter);
                        playerMovement.transform.SetPositionAndRotation(playerCharacter.startPosition,
                            Quaternion.identity);
                    }
                    else {
                        GameObject playerObject = Instantiate(playerCharacter.entityDisplay.prefab,
                            playerCharacter.startPosition, Quaternion.identity);
                        playerMovement = playerObject.GetComponent<Instantiated.PlayerCharacter>();

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
                    nonVoxelWorld.instantiationMap[playerCharacter] = playerMovement;
                }

                continue;
            }
            
            if (TypeUtil.IsSameTypeOrSubclass(nonVoxelSpawnable, typeof(EntityDefinition.TangibleEntity))) {
                EntityDefinition.TangibleEntity tangibleEntity 
                    = (EntityDefinition.TangibleEntity)nonVoxelSpawnable;
                GameObject gameObject = Instantiate(tangibleEntity.entityDisplay.prefab,
                    tangibleEntity.startPosition, Quaternion.identity);
                InstantiatedEntity script = gameObject.GetComponent<InstantiatedEntity>();

                if (nonVoxelSpawnable.GetType() == typeof(EntityDefinition.NPC)) {
                    EntityDefinition.NPC npcInfo = (EntityDefinition.NPC)nonVoxelSpawnable;
                    Instantiated.NPC npcBehavior = (Instantiated.NPC)script;
                    npcBehavior.Init(nonVoxelWorld, spriteMovement, randomManager.rng, npcInfo,
                        cameraManager, partyManager, gameStateManager);
                    nonVoxelWorld.npcs.Add(npcBehavior);

                    if (npcInfo.battleGroup != null) {
                        Guid battleGroupID = npcInfo.battleGroup.groupID;
                        if (!battleGroups.ContainsKey(battleGroupID)) {
                            battleGroups[battleGroupID] = new HashSet<Instantiated.NPC>();
                        }
                        battleGroups[battleGroupID].Add(npcBehavior);
                        npcBehavior.teammates = battleGroups[battleGroupID];
                    }
                    nonVoxelWorld.AddEntity(npcBehavior);
                }

                if (nonVoxelSpawnable.GetType() == typeof(EntityDefinition.TangibleObject)) {
                    EntityDefinition.TangibleObject nonVoxelObject
                        = (EntityDefinition.TangibleObject)nonVoxelSpawnable;
                    Instantiated.TangibleObject instantiatedNVObject = (Instantiated.TangibleObject)script;
                    instantiatedNVObject.Init(nonVoxelWorld, nonVoxelObject);
                    nonVoxelWorld.AddEntity(instantiatedNVObject);
                }

                nonVoxelWorld.instantiationMap[tangibleEntity] = script;
            }
            else {
                EntityDefinition.IntangibleEntity intangibleEntity
                    = (EntityDefinition.IntangibleEntity)nonVoxelSpawnable;
                GameObject gameObject = Instantiate(intangibleEntity.prefab,
                    intangibleEntity.startPosition, Quaternion.identity);
                InstantiatedEntity script = gameObject.GetComponent<InstantiatedEntity>();

                if (nonVoxelSpawnable.GetType() == typeof(EntityDefinition.StoryEventCube)) {
                    EntityDefinition.StoryEventCube definition 
                        = (EntityDefinition.StoryEventCube)nonVoxelSpawnable;
                    Instantiated.StoryEventCube storyEventCube = (Instantiated.StoryEventCube)script;
                    storyEventCube.Init(definition, orderManager);
                }

                if (nonVoxelSpawnable.GetType() == typeof(EntityDefinition.SceneExitCube)) {
                    EntityDefinition.SceneExitCube sceneExitCube
                        = (EntityDefinition.SceneExitCube)nonVoxelSpawnable;
                    Instantiated.SceneExitCube sceneExitComponent = (Instantiated.SceneExitCube)script;
                    sceneExitComponent.Init(environmentSceneManager, sceneExitCube.destination, sceneExitCube);
                }
                
                nonVoxelWorld.instantiationMap[intangibleEntity] = script;
            }
        }
    }
}
