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
using System.Linq;

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
    [SerializeField] AudioController audioController;
    [SerializeField] MusicManager musicManager;

    [SerializeField] TravellerIdentitySO mainCharacterID;
    [SerializeField] TravellerIdentitySO sidekickID;
    [SerializeField] TravellerIdentitySO friendID;
    [SerializeField] TravellerIdentitySO rogueGrimesID;
    [SerializeField] TravellerIdentitySO bardDrillID;
    [SerializeField] TravellerIdentitySO commonerHaulID;
    [SerializeField] TravellerIdentitySO wolfID;
    [SerializeField] TravellerIdentitySO catID;
    [SerializeField] TravellerIdentitySO zombieID;
    [SerializeField] TravellerIdentitySO bloodyEyeID;
    [SerializeField] TravellerIdentitySO ghoulID;
    [SerializeField] ObjectIdentitySO bedID;
    [SerializeField] ObjectIdentitySO lampID;
    [SerializeField] ObjectIdentitySO constructionToolsID;
    [SerializeField] GameObject sceneExitPrefab;
    [SerializeField] GameObject storyEventCubePrefab;

    [SerializeField] TextAsset getAttention;
    [SerializeField] TextAsset friendDialogue;
    [SerializeField] TextAsset catIntroDialogue;
    [SerializeField] TextAsset drillFirstConversation;
    [SerializeField] TextAsset grimesIntroDialogue;
    [SerializeField] TextAsset coreySpeechDialogue;
    [SerializeField] TextAsset haulIntroDialogue;
    [SerializeField] TextAsset haulFinalDialogue;

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
        Dictionary<int, SceneInfo> sceneEntityStateTemp = saveData.sceneEntityState;
        // make it possible to have saved vpenvironment info but need to load other info
        SceneInfo currSceneInfo = sceneEntityStateTemp[currDestination.sceneIndex];

        bool hasOnlyObjects = false;
        if (currSceneInfo.entities != null) {
            int numNonObjects = currSceneInfo.entities
                .Where((entity) => {
                    return !TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(TangibleObject));
                })
                .Count();
            hasOnlyObjects = numNonObjects == 0;
        }
        if (currSceneInfo.vpSaveBase64 != null && hasOnlyObjects) {
            Debug.Log("Using default values with loaded vpenvironment and non-objects.");
            sceneEntityState = SetUpDefaultWorldEntities();
            foreach (KeyValuePair<int, SceneInfo> item in sceneEntityState) {
                int sceneIndex = item.Key;
                sceneEntityState[sceneIndex].vpSaveBase64 = sceneEntityStateTemp[sceneIndex].vpSaveBase64;

                // remove objects from default
                HashSet<Entity> entitiesToRemove = new();
                for (int i = 0; i < sceneEntityState[sceneIndex].entities.Count; i++) {
                    Entity entity = sceneEntityState[sceneIndex].entities[i];
                    if (TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(TangibleObject))) {
                        entitiesToRemove.Add(entity);
                    }
                }
                sceneEntityState[sceneIndex].entities.RemoveAll(e => entitiesToRemove.Contains(e));

                // add them from save state
                foreach (Entity entity in sceneEntityStateTemp[sceneIndex].entities) {
                    if (TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(TangibleObject))) {
                        sceneEntityState[sceneIndex].entities.Add(entity);
                    }
                }
            }
        }
        else {
            sceneEntityState = saveData.sceneEntityState;
        }

        yield return SceneManager.LoadSceneAsync(currDestination.sceneIndex);
    }

    private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode) {
        VoxelPlayEnvironment environment = FindObjectOfType<VoxelPlayEnvironment>();
        if (loadedScene.buildIndex == 0) {
            saveManager.Load();
            return;
        }

        voxelWorldManager.SetVoxelPlayEnvironment(environment);
        voxelWorldManager.AssignVPEnvironmentInitEvent(constructionUI.OnEnvInitialized);
        environment.OnInitialized += Environment_OnInitialized;

        environment.cameraMain = cameraManager.GetMainCamera();
        cameraManager.SetSeeThroughTarget(environment);

        environment.Init();
        string vpBase64 = sceneEntityState[currDestination.sceneIndex].vpSaveBase64;
        if (vpBase64 != null) {
            environment.LoadGameFromBase64(vpBase64, false);
        }

        nonVoxelManager.CreateEntities(sceneEntityState[currDestination.sceneIndex].entities, currDestination);
        partyManager.SetCurrControlledCharacter(partyManager.mainCharacter);
        musicManager.StartMusic();
    }

    private void Environment_OnInitialized() {
        Debug.Log("Environment initialized.");
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
        PlayerCharacter testFighter = new(new Vector3Int(862, 29, 345), friendID.name);
        PlayerCharacter testRogue = new(new Vector3Int(850, -8, 345), rogueGrimesID.name);
        PlayerCharacter testBard = new(new Vector3Int(805, -29, 355), bardDrillID.name);

        NPC fighterCorey = new(new Vector3Int(869, 22, 350), Faction.PLAYER, IdleBehavior.STAND,
            friendID.name);
        NPC cat = new(new Vector3Int(858, 33, 350), Faction.PLAYER, IdleBehavior.WANDER, catID.name);
        cat.interactOrders = new OrderGroup(new List<Order> {
            new DialogueOrder(catIntroDialogue, new Dictionary<string, Guid>{{catID.name, cat.guid}}),
        });
        NPC rogueGrimes = new(new Vector3Int(845, -4, 327), Faction.PLAYER, IdleBehavior.STAND,
            rogueGrimesID.name);
        rogueGrimes.interactOrders = new(new List<Order> {
            new CameraFocusOrder(rogueGrimes),
            new DialogueOrder(grimesIntroDialogue, "Grimes")
        }, true);
        NPC bardDrill = new(new Vector3Int(862, -6, 329), Faction.PLAYER, IdleBehavior.STAND,
            bardDrillID.name);
        bardDrill.interactOrders = new(new List<Order> {
            new CameraFocusOrder(bardDrill),
            new DialogueOrder(drillFirstConversation, "Drill")
        }, true);
        NPC commonerHaul = new(new Vector3Int(799, -24, 351), Faction.PLAYER, IdleBehavior.STAND,
            commonerHaulID.name);

        BattleGroup convenienceBG = new(new List<NPC> {
            new(new Vector3Int(835, 29, 351), Faction.ENEMY, IdleBehavior.WANDER, bloodyEyeID.name),
            new(new Vector3Int(835, 29, 350), Faction.ENEMY, IdleBehavior.WANDER, bloodyEyeID.name),
        });
        BattleGroup zombieBG1 = new(new List<NPC> {
            new(new Vector3Int(849, -12, 365), Faction.ENEMY, IdleBehavior.WANDER, zombieID.name),
            new(new Vector3Int(849, -12, 362), Faction.ENEMY, IdleBehavior.WANDER, zombieID.name),
            new(new Vector3Int(840, -12, 365), Faction.ENEMY, IdleBehavior.WANDER, zombieID.name),
        });
        BattleGroup bloodyEyeBG = new(new List<NPC> {
            new(new Vector3Int(822, -27, 347), Faction.ENEMY, IdleBehavior.WANDER, bloodyEyeID.name),
            new(new Vector3Int(822, -27, 346), Faction.ENEMY, IdleBehavior.WANDER, bloodyEyeID.name),
            new(new Vector3Int(822, -27, 345), Faction.ENEMY, IdleBehavior.WANDER, bloodyEyeID.name),
            new(new Vector3Int(823, -27, 345), Faction.ENEMY, IdleBehavior.WANDER, bloodyEyeID.name),
        });
        NPC ghoulBoss = new(new Vector3Int(798, -29, 353), Faction.ENEMY, IdleBehavior.WANDER, ghoulID.name);

        TangibleObject lamp = new TangibleObject(new Vector3Int(858, 33, 351), lampID.name, Direction.NORTH);

        OrderGroup coreyUndergroundMeetingOrders = new OrderGroup(new List<Order> {
            // todo place them in the arb
            new ChangeOrdersOrder(rogueGrimes, null),
            new ChangeOrdersOrder(bardDrill, null),
            new MoveOrder(new Vector3Int(846, -8, 337), mainCharacter, true),
            new MoveOrder(new Vector3Int(844, -8, 339), bardDrill, false),
            new MoveOrder(new Vector3Int(843, -8, 337), rogueGrimes, true),
            new CameraFocusOrder(fighterCorey),
            // todo - music change (e.g. tense, exciting, silly)
            // todo - use input name of player
            // todo - reaction bubbles above non-combat NPCs in crowd (e.g. I'm scared!)
            new DialogueOrder(coreySpeechDialogue, "Corey"),
            new JoinPartyOrder(fighterCorey.guid),
            new JoinPartyOrder(bardDrill.guid),
            new JoinPartyOrder(rogueGrimes.guid)
        }, true);
        OrderGroup coreyIntroOrders = new OrderGroup(new List<Order>{
            new MoveOrder(new Vector3Int(862, 29, 346), fighterCorey.guid, waitForCompletion: false),
            new DialogueOrder(getAttention, "???"),
            new ExclaimOrder(mainCharacter),
            new MoveOrder(new Vector3Int(859, 37, 347), mainCharacter),
            new CameraFocusOrder(fighterCorey),
            new DialogueOrder(friendDialogue, "Corey"),
            new MoveOrder(new Vector3Int(846, -8, 339), fighterCorey, false),
            new CameraFocusOrder(mainCharacter),
            // todo place covering arb
            new CreateEntityOrder(new StoryEventCube(new Vector3Int(842, -8, 334), 10,
                ResourceIDs.STORY_EVENT_CUBE_STRING, coreyUndergroundMeetingOrders))
        }, true);
        StoryEventCube introEventCube = new StoryEventCube(
            new Vector3Int(855, 36, 350), 1, ResourceIDs.STORY_EVENT_CUBE_STRING,
            coreyIntroOrders
        );

        Vector3Int sceneExitTile = new Vector3Int(862, -8, 341);
        SceneExitCube level1Exit = new(sceneExitTile,
            new EnvChangeDestination(1, new Vector3Int(466, 29, -46)),
            ResourceIDs.SCENE_EXIT_STRING);
        StoryEventCube level1ExitPrevention = new(new Vector3Int(860, -10, 339), 6,
            ResourceIDs.STORY_EVENT_CUBE_STRING, new OrderGroup(new List<Order> {
                new DialogueOrder("I don't have time to leave right now!"),
                // todo - make target the currcontrolled character, or the entire party.
                new MoveImmediateOrder(new Vector3Int(861, -8, 334), MoveOrderType.Party)
            }, false));
        commonerHaul.interactOrders = new OrderGroup(new List<Order> {
            new CameraFocusOrder(commonerHaul),
            new DialogueOrder(haulIntroDialogue, "Haul"),
            new CameraFocusOrder(mainCharacter),
            new MoveImmediateOrder(new Vector3Int(844, -8, 337), MoveOrderType.Party),
            new MoveImmediateOrder(new Vector3Int(846, -8, 339), MoveOrderType.Entity, commonerHaul.guid),
            new CameraFocusOrder(commonerHaul),
            new DialogueOrder(haulFinalDialogue, "Haul"),
            new DestroyOrder(level1ExitPrevention)
        });

        Dictionary<int, SceneInfo> defaults = new() {
            {
                1, new SceneInfo(new List<Entity> {
                    new SceneExitCube(
                        new Vector3Int(463, 29, -46),
                        new EnvChangeDestination(2, new Vector3Int(884, 26, 348)),
                        ResourceIDs.SCENE_EXIT_STRING)
                }, null)
            },
            {
                2, new SceneInfo(new List<Entity> {
                    new SceneExitCube(
                        new Vector3Int(884, 26, 345),
                        new EnvChangeDestination(3, new Vector3Int(858, 37, 347)),
                        ResourceIDs.SCENE_EXIT_STRING)
                }, null)
            },
            {
                3, new SceneInfo(new List<Entity> {
                    mainCharacter,
                    //testFighter,
                    //testRogue,
                    //testBard,
                    fighterCorey,
                    cat,
                    rogueGrimes,
                    bardDrill,
                    commonerHaul,

                    zombieBG1.combatants[0],
                    zombieBG1.combatants[1],
                    zombieBG1.combatants[2],
                    bloodyEyeBG.combatants[0],
                    bloodyEyeBG.combatants[1],
                    bloodyEyeBG.combatants[2],
                    bloodyEyeBG.combatants[3],
                    ghoulBoss,
                    //convenienceBG.combatants[0],
                    //convenienceBG.combatants[1],

                    lamp,

                    introEventCube,
                    level1Exit,
                    level1ExitPrevention,

                    new MusicCube(audioController.snowpointTheme.name, 1, new(836, -9, 319),
                        new(882, 2, 348)),
                    new MusicCube(audioController.eternaForestTheme.name, 2, new(689, -40, 300),
                        new(860, -6, 500)),
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
