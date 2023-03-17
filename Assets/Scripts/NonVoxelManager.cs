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
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerInstance;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private NonVoxelWorld nonVoxelWorld;
    [SerializeField] private SpriteMovement spriteMovement;
    [SerializeField] private RandomManager randomManager;
    [SerializeField] private VoxelWorldManager voxelWorldManager;
    [SerializeField] private CameraManager cameraManager;

    [SerializeField] private GameObject opossumPrefab;
    [SerializeField] private GameObject sceneExitPrefab;
    [SerializeField] private GameObject detachedCameraPrefab;

    private InstantiatedEntity.PlayerCharacter playerCharacter;
    private NonVoxelInitialization nonVoxelInitialization;

    private List<Entity> nonVoxelEntities;

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
        nonVoxelWorld.SetPosition(playerInstance, playerStartPosition);
    }

    private void InitCreaturesAndWorld() {
        Dictionary<Guid, HashSet<NPCBehavior>> battleGroups = 
            new Dictionary<Guid, HashSet<NPCBehavior>>();
        foreach (Entity nonVoxelEntity in nonVoxelEntities) {
            if (nonVoxelEntity.GetType() == typeof(NonVoxelEntity.PlayerCharacter)) {
                NonVoxelEntity.PlayerCharacter entity = 
                    (NonVoxelEntity.PlayerCharacter)nonVoxelEntity;
                playerCharacter = playerInstance.GetComponent<InstantiatedEntity.PlayerCharacter>();
                playerCharacter.Init(entity);
                continue;
            }

            GameObject gameObject = Instantiate(nonVoxelEntity.prefab,
                nonVoxelEntity.startPosition, Quaternion.identity);
            nonVoxelWorld.SetPosition(gameObject, nonVoxelEntity.startPosition);
                
            if (nonVoxelEntity.GetType() == typeof(SceneExitCube)) {
                SceneExitCube sceneExitCube = (SceneExitCube)nonVoxelEntity;
                SceneExit sceneExitComponent = gameObject.GetComponent<SceneExit>();
                sceneExitComponent.Init(environmentSceneManager, sceneExitCube.destination);
            }

            if (nonVoxelEntity.GetType() == typeof(NPC)) {
                NPC npcInfo = (NPC)nonVoxelEntity;
                NPCBehavior npcBehavior = gameObject.GetComponent<NPCBehavior>();
                npcBehavior.Init(nonVoxelWorld, spriteMovement, randomManager.rng, npcInfo,
                    cameraManager);
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
