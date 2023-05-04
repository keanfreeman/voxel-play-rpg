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
    [SerializeField] GameObject genericNonVoxelObjectPrefab;
    [SerializeField] GameObject playerPrefab;

    public Dictionary<string, TravellerIdentitySO> travellerIdentities { get; private set; } = new();
    public Dictionary<string, ObjectIdentitySO> objectIdentities { get; private set; } = new();
    public Dictionary<string, GameObject> intangiblePrefabs { get; private set; } = new();

    private void Awake() {
        SetUpResources();
    }

    public void ConvertNPCToPlayer(Instantiated.NPC npc) {
        NPC npcEntity = npc.GetEntity();
        Vector3Int currOrigin = npc.origin;
        nonVoxelWorld.DestroyEntity(npc);

        PlayerCharacter newPC = new PlayerCharacter(currOrigin, npcEntity.identity);
        CreatePCInstance(newPC);
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
                    CreatePCInstance((PlayerCharacter)entity);
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
                    
                    GameObject baseObjectPrefab = Instantiate(genericNonVoxelObjectPrefab);
                    Instantiated.TangibleObject instantiatedNVObject 
                        = baseObjectPrefab.GetComponent<Instantiated.TangibleObject>();
                    Instantiate(identity.prefab, instantiatedNVObject.leafTransform);
                    baseObjectPrefab.transform.position = nonVoxelObject.spawnPosition;

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

    private void CreatePCInstance(PlayerCharacter playerInfo) {
        TravellerIdentitySO identity = travellerIdentities[playerInfo.identity];
        GameObject playerObject = Instantiate(playerPrefab,
            playerInfo.spawnPosition, Quaternion.identity);
        Instantiated.PlayerCharacter playerInstance
            = playerObject.GetComponent<Instantiated.PlayerCharacter>();
        playerInstance.Init(spriteMovement, playerInfo, identity, nonVoxelWorld, cameraManager,
            partyManager);
        playerInstance.SetCurrPositions(playerInfo.spawnPosition, identity);
        partyManager.partyMembers.Add(playerInstance);
        if (identity == mainCharacterID) {
            partyManager.SetMainCharacter(playerInstance);
        }

        nonVoxelWorld.AddTangibleEntity(playerInfo, playerInstance);
    }
}
