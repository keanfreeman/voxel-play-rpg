using NonVoxel;
using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Saving;

public class NonVoxelManager : MonoBehaviour
{
    [SerializeField] InputManager inputManager;
    [SerializeField] EnvironmentSceneManager environmentSceneManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] RandomManager randomManager;
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] OrderManager orderManager;
    [SerializeField] SaveManager saveManager;

    [SerializeField] TravellerIdentitySO mainCharacterID;

    [SerializeField] GameObject detachedCameraPrefab;

    public Dictionary<string, TravellerIdentitySO> travellerIdentities { get; private set; } = new();
    public Dictionary<string, ObjectIdentitySO> objectIdentities { get; private set; } = new();
    public Dictionary<string, GameObject> intangiblePrefabs { get; private set; } = new();

    private void Awake() {
        SetUpResources();
    }

    private void SetUpResources() {
        TravellerIdentitySO[] travellerIDs = Resources.LoadAll<TravellerIdentitySO>(
            "ScriptableObjects/Identities/Travellers");
        foreach (TravellerIdentitySO travellerIdentitySO in travellerIDs) {
            travellerIdentities.Add(travellerIdentitySO.name, travellerIdentitySO);
        }

        ObjectIdentitySO[] objectIDs = Resources.LoadAll<ObjectIdentitySO>(
            "ScriptableObjects/Identities/Objects");
        foreach (ObjectIdentitySO objectIdentitySO in objectIDs) {
            objectIdentities.Add(objectIdentitySO.name, objectIdentitySO);
        }

        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/IntangibleEntity");
        foreach (GameObject gameObject in prefabs) {
            intangiblePrefabs.Add(gameObject.name, gameObject);
        }
    }

    public void CreateEntities(List<Entity> entities, EnvChangeDestination destination) {
        Dictionary<Guid, HashSet<Instantiated.NPC>> battleGroups = 
            new Dictionary<Guid, HashSet<Instantiated.NPC>>();
        foreach (Entity entity in entities) {
            if (TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(TangibleEntity))) {
                if (entity.GetType() == typeof(PlayerCharacter)) {
                    PlayerCharacter playerInfo = (PlayerCharacter)entity;
                    TravellerIdentitySO identity = travellerIdentities[playerInfo.identity];
                    Instantiated.PlayerCharacter playerInstance;

                    // check if player instance already exists
                    if (partyManager.currControlledCharacter != null) {
                        playerInstance = partyManager.GetPlayerInstance(playerInfo);
                        playerInstance.SetCurrPositions(playerInfo.spawnPosition, identity);
                    }
                    else {
                        GameObject playerObject = Instantiate(identity.prefab,
                            destination.destinationTile, Quaternion.identity);
                        playerInstance = playerObject.GetComponent<Instantiated.PlayerCharacter>();
                        playerInstance.Init(spriteMovement, playerInfo, identity, nonVoxelWorld, cameraManager,
                            partyManager);
                        playerInstance.SetCurrPositions(playerInfo.spawnPosition, identity);
                        partyManager.partyMembers.Add(playerInstance);
                        if (identity == mainCharacterID) {
                            partyManager.SetMainCharacter(playerInstance);
                        }

                        nonVoxelWorld.AddTangibleEntity(playerInfo, playerInstance);
                    }
                    continue;
                }
                else if (entity.GetType() == typeof(NPC)) {
                    NPC npcInfo = (NPC)entity;
                    TravellerIdentitySO identity = travellerIdentities[npcInfo.identity];
                    GameObject npcObject = Instantiate(identity.prefab, npcInfo.spawnPosition, 
                        Quaternion.identity);
                    Instantiated.NPC npcInstance = npcObject.GetComponent<Instantiated.NPC>();
                    npcInstance.Init(nonVoxelWorld, spriteMovement, randomManager.rng, npcInfo, identity,
                        cameraManager, partyManager, gameStateManager);

                    if (npcInfo.battleGroup != null) {
                        Guid battleGroupID = npcInfo.battleGroup.groupID;
                        if (!battleGroups.ContainsKey(battleGroupID)) {
                            battleGroups[battleGroupID] = new HashSet<Instantiated.NPC>();
                        }
                        battleGroups[battleGroupID].Add(npcInstance);
                        npcInstance.teammates = battleGroups[battleGroupID];
                    }
                    nonVoxelWorld.AddTangibleEntity(npcInfo, npcInstance);
                }
                else {
                    TangibleObject nonVoxelObject = (TangibleObject)entity;
                    ObjectIdentitySO identity = objectIdentities[nonVoxelObject.identity];
                    GameObject npcObject = Instantiate(identity.prefab, nonVoxelObject.spawnPosition,
                        Quaternion.identity);
                    Instantiated.TangibleObject instantiatedNVObject 
                        = npcObject.GetComponent<Instantiated.TangibleObject>();
                    instantiatedNVObject.Init(nonVoxelWorld, nonVoxelObject, identity);
                    nonVoxelWorld.AddTangibleEntity(nonVoxelObject, instantiatedNVObject);
                }
            }
            else {
                IntangibleEntity intangibleEntity = (IntangibleEntity)entity;
                GameObject gameObject = Instantiate(intangiblePrefabs[intangibleEntity.prefabName],
                    intangibleEntity.spawnPosition, Quaternion.identity);
                Instantiated.IntangibleEntity script = gameObject
                    .GetComponent<Instantiated.IntangibleEntity>();

                if (intangibleEntity.GetType() == typeof(StoryEventCube)) {
                    StoryEventCube definition = (StoryEventCube)intangibleEntity;
                    Instantiated.StoryEventCube storyEventCube = (Instantiated.StoryEventCube)script;
                    storyEventCube.Init(definition, orderManager, nonVoxelWorld);
                }

                if (intangibleEntity.GetType() == typeof(SceneExitCube)) {
                    SceneExitCube sceneExitCube = (SceneExitCube)intangibleEntity;
                    Instantiated.SceneExitCube sceneExitComponent = (Instantiated.SceneExitCube)script;
                    sceneExitComponent.Init(environmentSceneManager, inputManager, partyManager,
                        sceneExitCube.destination, sceneExitCube);
                }

                nonVoxelWorld.AddIntangibleEntity(intangibleEntity, script);
            }
        }
    }
}
