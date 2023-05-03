using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelPlay;
using Orders;
using MovementDirection;
using Saving;
using NonVoxel;
using ResourceID;

public class EnvironmentSceneManager : MonoBehaviour, ISaveable
{
    [SerializeField] VoxelWorldManager voxelWorldManager;
    [SerializeField] NonVoxelManager nonVoxelManager;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] CameraManager cameraManager;
    [SerializeField] PartyManager partyManager;
    [SerializeField] ConstructionUI constructionUI;
    [SerializeField] SaveManager saveManager;
    [SerializeField] NonVoxelWorld nonVoxelWorld;

    [SerializeField] TravellerIdentitySO mainCharacterID;
    [SerializeField] TravellerIdentitySO sidekickID;
    [SerializeField] TravellerIdentitySO friendID;
    [SerializeField] TravellerIdentitySO wolfID;
    [SerializeField] ObjectIdentitySO bedID;
    [SerializeField] ObjectIdentitySO lampID;
    [SerializeField] ObjectIdentitySO constructionToolsID;
    [SerializeField] GameObject sceneExitPrefab;
    [SerializeField] GameObject storyEventCubePrefab;
    [SerializeField] TextAsset getAttention;
    [SerializeField] TextAsset friendDialogue;

    public EnvChangeDestination currDestination { get; private set; }
        = new EnvChangeDestination(3, new Vector3Int(859, 37, 347));
    // first index is the scene index. done for serialization purposes.
    public Dictionary<int, SceneInfo> sceneEntityState;

    void Awake() {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        sceneEntityState = SetUpDefaultWorldEntities();
    }

    public void PopulateSaveData(SaveData saveData) {
        sceneEntityState[currDestination.sceneIndex].entities = RetrieveEntitiesInScene();
        VoxelPlayEnvironment vpEnv = voxelWorldManager.GetEnvironment();
        sceneEntityState[currDestination.sceneIndex].vpSaveBase64
            = vpEnv.SaveGameToBase64();

        saveData.currDestination = currDestination;
        saveData.sceneEntityState = sceneEntityState;
    }

    public IEnumerator LoadFromSaveData(SaveData saveData) {
        currDestination = saveData.currDestination;
        sceneEntityState = saveData.sceneEntityState;
        yield return SceneManager.LoadSceneAsync(currDestination.sceneIndex);
    }

    private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode) {
        VoxelPlayEnvironment environment = FindObjectOfType<VoxelPlayEnvironment>();
        if (loadedScene.buildIndex == 0) {
            if (saveManager.SaveExists()) {
                StartCoroutine(saveManager.Load());
            }
            else {
                SceneManager.LoadScene(3);
            }
            return;
        }

        voxelWorldManager.SetVoxelPlayEnvironment(environment);
        voxelWorldManager.AssignVPEnvironmentInitEvent(constructionUI.OnEnvInitialized);

        environment.Init();
        string vpBase64 = sceneEntityState[currDestination.sceneIndex].vpSaveBase64;
        if (vpBase64 != null) {
            environment.LoadGameFromBase64(vpBase64, false);
        }

        nonVoxelManager.CreateEntities(sceneEntityState[currDestination.sceneIndex].entities, currDestination);
        environment.cameraMain = cameraManager.GetMainCamera();
        partyManager.SetCurrControlledCharacter(partyManager.mainCharacter);

        StartCoroutine(gameStateManager.SetControlState(ControlState.SPRITE_NEUTRAL));
    }

    public IEnumerator LoadNextScene(EnvChangeDestination newDestination) {
        yield return gameStateManager.SetControlState(ControlState.LOADING);

        sceneEntityState[currDestination.sceneIndex].entities = RetrieveEntitiesInScene();
        MovePlayersToNextScene(currDestination, newDestination);
        currDestination = newDestination;

        voxelWorldManager.SetVoxelPlayEnvironment(null);
        cameraManager.DeParentCamera();
        nonVoxelWorld.DestroyAllEntities();
        partyManager.ClearData();
        yield return SceneManager.LoadSceneAsync(currDestination.sceneIndex);
    }

    // todo - address list inefficiency if necessary
    public void MovePlayersToNextScene(EnvChangeDestination origin, EnvChangeDestination destination) {
        List<Entity> originEntities = sceneEntityState[origin.sceneIndex].entities;
        List<Entity> destinationEntities = sceneEntityState[destination.sceneIndex].entities;

        List<Entity> pcs = new();
        foreach (Entity entity in originEntities) {
            if (entity.GetType() == typeof(PlayerCharacter)) {
                PlayerCharacter pcDef = (PlayerCharacter)entity;
                pcDef.spawnPosition = destination.destinationTile + Vector3Int.right * pcs.Count;
                destinationEntities.Add(entity);
                pcs.Add(entity);
            }
        }

        foreach (Entity pc in pcs) {
            originEntities.Remove(pc);
        }
    }

    public List<Entity> RetrieveEntitiesInScene() {
        List<Entity> spawnables = new List<Entity>();
        foreach (KeyValuePair<Guid, Instantiated.InstantiatedEntity> pair 
                in nonVoxelWorld.entityIDToInstantiation) {
            Instantiated.InstantiatedEntity instantiation = pair.Value;
            Entity entity = instantiation.GetEntity();

            // if NPC, save moved position
            if (entity.GetType() == typeof(NPC)) {
                NPC npcDef = (NPC)entity;
                Instantiated.NPC npcInstance = (Instantiated.NPC)instantiation;
                npcDef.currSpawnPosition = npcInstance.origin;
            }

            if (entity.GetType() == typeof(PlayerCharacter)) {
                PlayerCharacter pcDef = (PlayerCharacter)entity;
                Instantiated.PlayerCharacter pcInstance = (Instantiated.PlayerCharacter)instantiation;
                pcDef.spawnPosition = pcInstance.origin;
            }

            spawnables.Add(entity);
        }

        return spawnables;
    }

    private Dictionary<int, SceneInfo> SetUpDefaultWorldEntities() {
        PlayerCharacter mainCharacter = new PlayerCharacter(new Vector3Int(859, 37, 347),
            mainCharacterID.name);
        PlayerCharacter sidekick = new PlayerCharacter(new Vector3Int(864, 29, 347),
            sidekickID.name);

        NPC commoner = new NPC(new Vector3Int(862, 29, 346), Faction.PLAYER, IdleBehavior.STAND,
            friendID.name);

        BattleGroup battleGroup1 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(835, 29, 350), Faction.ENEMY, IdleBehavior.WANDER,
                wolfID.name),
            new NPC(new Vector3Int(835, 29, 347), Faction.ENEMY, IdleBehavior.WANDER,
                wolfID.name)
        });
        BattleGroup battleGroup2 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(825, 31, 348), Faction.ENEMY, IdleBehavior.WANDER,
                wolfID.name),
            new NPC(new Vector3Int(825, 31, 350), Faction.ENEMY, IdleBehavior.WANDER,
                wolfID.name)
        });
        BattleGroup battleGroup3 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(468, 26, -46), Faction.ENEMY, IdleBehavior.WANDER,
                wolfID.name)
        });

        TangibleObject bed = new TangibleObject(new Vector3Int(857, 29, 350), Direction.NORTH,
            bedID.name);
        TangibleObject lamp = new TangibleObject(new Vector3Int(858, 33, 351), Direction.NORTH,
            lampID.name);
        TangibleObject constructionTools = new TangibleObject(new Vector3Int(858, 37, 351), Direction.NORTH,
            constructionToolsID.name);

        OrderGroup coreyIntroOrders = new OrderGroup(new List<Order>{
            new DialogueOrder(getAttention, "???"),
            new ExclaimOrder(mainCharacter),
            new MoveOrder(new Vector3Int(859, 37, 347), mainCharacter),
            new CameraFocusOrder(commoner),
            new DialogueOrder(friendDialogue, "Corey"),
            new CameraFocusOrder(mainCharacter),
            new MoveOrder(new Vector3Int(862, 29, 350), commoner)
        }, true);
        StoryEventCube introEventCube = new StoryEventCube(
            new Vector3Int(856, 36, 350), 1, ResourceIDs.STORY_EVENT_CUBE_STRING,
            coreyIntroOrders
        );
        OrderGroup toolsIntroOrders = new OrderGroup(new List<Order> {
            new DialogueOrder("It's a set of construction tools. Worth every penny!"),
            new DestroyOrder(constructionTools),
            // todo - play pickup fanfare
        });
        constructionTools.interactOrders = toolsIntroOrders;

        Dictionary<int, SceneInfo> defaults = new() {
            {
                1, new SceneInfo(new List<Entity> {
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(463, 29, -46),
                        new EnvChangeDestination(2, new Vector3Int(884, 26, 348)),
                        ResourceIDs.SCENE_EXIT_STRING)
                }, null)
            },
            {
                2, new SceneInfo(new List<Entity> {
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(884, 26, 346),
                        new EnvChangeDestination(3, new Vector3Int(858, 37, 347)),
                        ResourceIDs.SCENE_EXIT_STRING)
                }, null)
            },
            {
                3, new SceneInfo(new List<Entity> {
                    mainCharacter,
                    sidekick,
                    commoner,
                    battleGroup1.combatants[0],
                    battleGroup1.combatants[1],
                    battleGroup2.combatants[0],
                    battleGroup2.combatants[1],
                    new SceneExitCube(
                        new Vector3Int(864, 29, 351),
                        new EnvChangeDestination(1, new Vector3Int(466, 29, -46)),
                        ResourceIDs.SCENE_EXIT_STRING),
                    introEventCube,
                    bed,
                    lamp,
                    constructionTools
                }, null)
            }
        };

        return defaults;
    }
}

[Serializable]
public class SceneInfo {
    public List<Entity> entities;
    public string vpSaveBase64;

    public SceneInfo(List<Entity> entities, string vpSaveBase64) {
        this.entities = entities;
        this.vpSaveBase64 = vpSaveBase64;
    }
}
