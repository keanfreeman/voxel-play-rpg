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
    [SerializeField] GameObject playerInstance;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] RandomManager randomManager;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PartyManager partyManager;

    [SerializeField] GameObject opossumPrefab;
    [SerializeField] GameObject sceneExitPrefab;
    [SerializeField] GameObject detachedCameraPrefab;

    private InstantiatedEntity.PlayerCharacter playerCharacter;
    private NonVoxelInitialization nonVoxelInitialization;

    private List<Spawnable> nonVoxelEntities;

    private void Awake() {
        nonVoxelInitialization = new NonVoxelInitialization(playerPrefab,
            opossumPrefab, sceneExitPrefab);
    }

    public void DestroyEntities() {
        nonVoxelWorld.DestroyEntities();
    }

    public void SetUpEntities(int environmentIndex) {
        nonVoxelEntities = nonVoxelInitialization.GetEnvEntities(environmentIndex);
        SetUpPlayer(environmentIndex);
        InitCreaturesAndWorld();
    }

    public void SetUpPlayer(int currEnvIndex) {
        Vector3Int playerStartPosition = nonVoxelInitialization.GetPlayerStartPosition(currEnvIndex);
        playerInstance.transform.position = playerStartPosition;
        playerMovement.SetCurrVoxel(playerStartPosition);
        nonVoxelWorld.SetPosition(playerMovement, playerStartPosition);
    }

    private void InitCreaturesAndWorld() {
        Dictionary<Guid, HashSet<NPCBehavior>> battleGroups = 
            new Dictionary<Guid, HashSet<NPCBehavior>>();
        foreach (Spawnable nonVoxelSpawnable in nonVoxelEntities) {
            if (nonVoxelSpawnable.GetType() == typeof(NonVoxelEntity.Party)) {
                Party party = (Party)nonVoxelSpawnable;
                NonVoxelEntity.PlayerCharacter entity = party.mainCharacter;
                playerCharacter = playerInstance.GetComponent<InstantiatedEntity.PlayerCharacter>();
                playerCharacter.Init(entity);

                foreach (NPC npcInfo in party.members) {
                    GameObject npcObject = Instantiate(npcInfo.prefab,
                        npcInfo.startPosition, Quaternion.identity);
                    NPCBehavior npcBehavior = npcObject.GetComponent<NPCBehavior>();
                    nonVoxelWorld.SetPosition(npcBehavior, npcInfo.startPosition);
                    npcBehavior.Init(nonVoxelWorld, spriteMovement, randomManager.rng, npcInfo,
                        cameraManager);
                }
                continue;
            }

            if (nonVoxelSpawnable.GetType() == typeof(NonVoxelEntity.PlayerCharacter)) {
                NonVoxelEntity.PlayerCharacter entity = 
                    (NonVoxelEntity.PlayerCharacter)nonVoxelSpawnable;
                playerCharacter = playerInstance.GetComponent<InstantiatedEntity.PlayerCharacter>();
                playerCharacter.Init(entity);
                continue;
            }

            Entity nonVoxelEntity = (Entity)nonVoxelSpawnable;
            GameObject gameObject = Instantiate(nonVoxelEntity.prefab,
                nonVoxelEntity.startPosition, Quaternion.identity);
            nonVoxelWorld.SetPosition(gameObject.GetComponent<InstantiatedNVE>(),
                nonVoxelEntity.startPosition);
                
            if (nonVoxelSpawnable.GetType() == typeof(SceneExitCube)) {
                SceneExitCube sceneExitCube = (SceneExitCube)nonVoxelSpawnable;
                SceneExit sceneExitComponent = gameObject.GetComponent<SceneExit>();
                sceneExitComponent.Init(environmentSceneManager, sceneExitCube.destination);
            }

            if (nonVoxelSpawnable.GetType() == typeof(NPC)) {
                NPC npcInfo = (NPC)nonVoxelSpawnable;
                NPCBehavior npcBehavior = gameObject.GetComponent<NPCBehavior>();
                npcBehavior.Init(nonVoxelWorld, spriteMovement, randomManager.rng, npcInfo,
                    cameraManager);
                nonVoxelWorld.enemyNPCs.Add(npcBehavior);

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
