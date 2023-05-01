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
    [SerializeField] GameObject sceneExitPrefab;
    [SerializeField] GameObject storyEventCubePrefab;
    [SerializeField] TextAsset getAttention;
    [SerializeField] TextAsset friendDialogue;

    public EnvChangeDestination currDestination { get; private set; }
        = new EnvChangeDestination(3, new Vector3Int(859, 37, 347));
    // first index is the scene index. done for serialization purposes.
    public Dictionary<int, List<Entity>> sceneEntityState;

    void Awake() {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        sceneEntityState = SetUpDefaultWorldEntities();
    }

    public void PopulateSaveData(SaveData saveData) {
        sceneEntityState[currDestination.destinationEnv] = SaveEntities(false);
        saveData.currDestination = currDestination;
        saveData.sceneEntityState = sceneEntityState;
    }

    public IEnumerator LoadFromSaveData(SaveData saveData) {
        currDestination = saveData.currDestination;
        sceneEntityState = saveData.sceneEntityState;
        yield return SceneManager.LoadSceneAsync(currDestination.destinationEnv);
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

        nonVoxelManager.CreateEntities(sceneEntityState[currDestination.destinationEnv], currDestination);
        partyManager.SetCurrControlledCharacter(partyManager.mainCharacter);
        environment.cameraMain = cameraManager.GetMainCamera();
        
        StartCoroutine(gameStateManager.SetControlState(ControlState.SPRITE_NEUTRAL));
    }

    public IEnumerator LoadNextScene(EnvChangeDestination newDestination) {
        StartCoroutine(gameStateManager.SetControlState(ControlState.LOADING));

        sceneEntityState[currDestination.destinationEnv] = SaveEntities(true);
        currDestination = newDestination;
        // add them to the next scene, which is where they'll be
        foreach (Instantiated.PlayerCharacter playerCharacter in partyManager.partyMembers) {
            playerCharacter.GetEntity().spawnPosition = partyManager
                .GetPositionFromDestination(currDestination, playerCharacter);
            sceneEntityState[currDestination.destinationEnv].Add(playerCharacter.GetEntity());
        }

        voxelWorldManager.SetVoxelPlayEnvironment(null);
        yield return nonVoxelWorld.DestroyAllEntities(false);
        yield return SceneManager.LoadSceneAsync(currDestination.destinationEnv);
    }

    public List<Entity> SaveEntities(bool removePlayers) {
        List<Entity> spawnables = new List<Entity>();
        foreach (KeyValuePair<Entity, Instantiated.InstantiatedEntity> pair in nonVoxelWorld.instantiationMap) {
            Entity entity = pair.Key;
            Instantiated.InstantiatedEntity instantiation = pair.Value;

            // if NPC, save moved position
            if (entity.GetType() == typeof(NPC)) {
                NPC npcDef = (NPC)entity;
                Instantiated.NPC npcInstance = (Instantiated.NPC)instantiation;
                npcDef.currSpawnPosition = npcInstance.origin;
            }

            if (entity.GetType() == typeof(PlayerCharacter)) {
                if (removePlayers) {
                    // remove from results and save in temporary spot for move to new scene.
                    continue;
                }

                PlayerCharacter pcDef = (PlayerCharacter)entity;
                Instantiated.PlayerCharacter pcInstance = (Instantiated.PlayerCharacter)instantiation;
                pcDef.spawnPosition = pcInstance.origin;
            }

            spawnables.Add(entity);
        }

        return spawnables;
    }

    private Dictionary<int, List<Entity>> SetUpDefaultWorldEntities() {
        PlayerCharacter mainCharacter = new PlayerCharacter(new Vector3Int(859, 37, 347),
            ResourceIDs.MAIN_CHARACTER_STRING);
        PlayerCharacter sidekick = new PlayerCharacter(new Vector3Int(864, 29, 347),
            ResourceIDs.SIDEKICK_STRING);

        NPC commoner = new NPC(new Vector3Int(862, 29, 346), Faction.PLAYER, IdleBehavior.STAND,
            ResourceIDs.FRIEND_STRING);

        BattleGroup battleGroup1 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(835, 29, 350), Faction.ENEMY, IdleBehavior.WANDER,
                ResourceIDs.WOLF_STRING),
            new NPC(new Vector3Int(835, 29, 347), Faction.ENEMY, IdleBehavior.WANDER,
                ResourceIDs.WOLF_STRING)
        });
        BattleGroup battleGroup2 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(825, 31, 348), Faction.ENEMY, IdleBehavior.WANDER,
                ResourceIDs.WOLF_STRING),
            new NPC(new Vector3Int(825, 31, 350), Faction.ENEMY, IdleBehavior.WANDER,
                ResourceIDs.WOLF_STRING)
        });
        BattleGroup battleGroup3 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(468, 26, -46), Faction.ENEMY, IdleBehavior.WANDER,
                ResourceIDs.WOLF_STRING)
        });

        TangibleObject bed = new TangibleObject(new Vector3Int(857, 29, 350), Direction.NORTH,
            ResourceIDs.BED_STRING);
        TangibleObject lamp = new TangibleObject(new Vector3Int(858, 33, 351), Direction.NORTH,
            ResourceIDs.LAMP_STRING);

        Dictionary<int, List<Entity>> defaults = new Dictionary<int, List<Entity>> {
            {
                1, new List<Entity> {
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(463, 29, -46),
                        new EnvChangeDestination(2, new Vector3Int(884, 26, 348)),
                        ResourceIDs.SCENE_EXIT_STRING)
                }
            },
            {
                2, new List<Entity> {
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(884, 26, 346),
                        new EnvChangeDestination(3, new Vector3Int(858, 37, 347)),
                        ResourceIDs.SCENE_EXIT_STRING)
                }
            },
            {
                3, new List<Entity> {
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
                    new StoryEventCube(
                        new Vector3Int(856, 36, 350), 1, ResourceIDs.STORY_EVENT_CUBE_STRING,
                        new OrderGroup(true, new List<Order>{
                            new DialogueOrder(getAttention, "???"),
                            new ExclaimOrder(mainCharacter),
                            new MoveOrder(new Vector3Int(859, 37, 347), mainCharacter),
                            new CameraFocusOrder(commoner),
                            new DialogueOrder(friendDialogue, "Corey")
                        })
                    ),
                    bed,
                    lamp
                }
            }
        };

        return defaults;
    }
}
