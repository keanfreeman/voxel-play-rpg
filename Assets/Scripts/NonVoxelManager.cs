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

    [SerializeField] GameObject opossumPrefab;
    [SerializeField] GameObject sceneExitPrefab;
    [SerializeField] GameObject detachedCameraPrefab;

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
                        GameObject playerObject = Instantiate(playerCharacter.prefab,
                            playerCharacter.startPosition, Quaternion.identity);

                        playerMovement = playerObject.GetComponent<PlayerMovement>();
                        playerMovement.Init(spriteMovement, playerCharacter, nonVoxelWorld, cameraManager,
                            partyManager);
                        playerMovement.spriteLibrary.spriteLibraryAsset = playerCharacter.spriteLibraryAsset;

                        partyManager.SetMainCharacter(playerMovement);
                        partyManager.partyMembers.Add(playerMovement);
                    }
                    nonVoxelWorld.SetPosition(playerMovement, playerCharacter.startPosition);
                    playerMovement.SetCurrVoxel(playerCharacter.startPosition);
                }

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
                    cameraManager, partyManager);
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
