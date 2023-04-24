using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EntityDefinition;
using GameMechanics;
using GameMechanics.Wolf;
using UnityEngine.U2D.Animation;
using Orders;
using NonVoxelEntity;
using MovementDirection;

public class NonVoxelInitialization {
    public Dictionary<int, List<Spawnable>> environmentObjects;

    public List<ObjectIdentity> objectIdentities = new List<ObjectIdentity>();
    public List<TravellerIdentity> travellerIdentities = new List<TravellerIdentity>();

    public NonVoxelInitialization(GameObject playerPrefab, GameObject npcPrefab,
            GameObject sceneExitPrefab, GameObject bedPrefab, GameObject lampPrefab, 
            GameObject storyEventCubePrefab, TextAsset getAttention, TextAsset friendDialogue) {
        // SPRITE LIBRARIES
        SpriteLibraryAsset playerSpriteLibrary =
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/PlayerSpriteLibrary");
        SpriteLibraryAsset loreleiSpriteLibrary = 
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/LoreleiSpriteLibrary");
        SpriteLibraryAsset yellowSpriteLibrary =
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/YellowSpriteLibrary");
        SpriteLibraryAsset opossumSpriteLibrary =
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/OpossumSpriteLibrary");

        // STATS
        List<Action> playerActions = new List<Action> {
            new Attack("Shortsword", new Dice(1, 20, 5), new Dice(1, 6, 0))
        };
        PlayerStats playerStats = new PlayerStats("Player1", 30, 10, EntitySize.MEDIUM, 10, 10, 10, 10, 10,
            10, playerActions, 1);

        Attack scoutShortswordAttack = new Attack("Shortsword", new Dice(1, 20, 4), new Dice(1, 6, 2));
        Multiattack scoutMeleeMultiattack = new Multiattack(
            new List<Attack> { scoutShortswordAttack, scoutShortswordAttack });
        Attack scoutLongbowAttack = new Attack("Longbow", new Dice(1, 20, 4), new Dice(1, 8, 2), 150, 600);
        Multiattack scoutRangedMultiattack = new Multiattack(
            new List<Attack> { scoutLongbowAttack, scoutLongbowAttack });
        List<GameMechanics.Action> scoutActions = new List<GameMechanics.Action> {
            scoutMeleeMultiattack, scoutRangedMultiattack
        };
        NPCStats scoutStats = new NPCStats("Scout", 30, 16, EntitySize.MEDIUM,
            11, 14, 12, 11, 13, 11, scoutActions, "1/2", 16);

        List<Action> commonerActions = new List<Action> { 
            new Attack("Club", new Dice(1, 20, 2), new Dice(1, 4)) };
        NPCStats commonerStats = new NPCStats("Commoner", 30, 4, EntitySize.MEDIUM, 10, 10, 10, 10, 10, 10,
            commonerActions, "0", 10);

        List<Action> wolfActions = new List<Action> {
            new Bite("Bite", new Dice(1, 20, 4), new Dice(2, 4, 2))
        };
        NPCStats wolfStats = new NPCStats("Wolf", 40, 11, EntitySize.LARGE, 12, 15, 12, 3, 12, 6, 
            wolfActions, "1/4", 13);

        // TRAVELLER IDENTITIES
        TravellerIdentity mainCharacterID = new TravellerIdentity(playerPrefab, playerSpriteLibrary,
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.8f, 0.8f, 0.8f), playerStats);
        travellerIdentities.Add(mainCharacterID);

        TravellerIdentity scoutID = new TravellerIdentity(playerPrefab, loreleiSpriteLibrary,
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.8f, 0.8f, 0.8f), scoutStats);
        travellerIdentities.Add(scoutID);

        TravellerIdentity commonerID = new TravellerIdentity(npcPrefab, yellowSpriteLibrary,
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.8f, 0.8f, 0.8f), commonerStats);
        travellerIdentities.Add(commonerID);

        TravellerIdentity wolfID = new TravellerIdentity(npcPrefab, opossumSpriteLibrary,
            new Vector3(1f, 0.6f, 1f), new Vector3(6.5f, 6.5f, 6.5f), wolfStats);
        travellerIdentities.Add(wolfID);

        // TRAVELLER INSTANTIATIONS
        PlayerCharacter mainCharacter = new PlayerCharacter(new Vector3Int(859, 37, 347), mainCharacterID);
        Party party = new Party(
            mainCharacter,
            new List<PlayerCharacter> {
                mainCharacter,
                new PlayerCharacter(new Vector3Int(864, 29, 347), scoutID),
            }
        );

        NPC commoner = new NPC(new Vector3Int(862, 29, 346), Faction.PLAYER, IdleBehavior.STAND, commonerID);

        BattleGroup battleGroup1 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(835, 29, 350), Faction.ENEMY, IdleBehavior.WANDER, wolfID),
            new NPC(new Vector3Int(835, 29, 347), Faction.ENEMY, IdleBehavior.WANDER, wolfID)
        });
        BattleGroup battleGroup2 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(825, 31, 348), Faction.ENEMY, IdleBehavior.WANDER, wolfID),
            new NPC(new Vector3Int(825, 31, 350), Faction.ENEMY, IdleBehavior.WANDER, wolfID)
        });
        BattleGroup battleGroup3 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(468, 26, -46), Faction.ENEMY, IdleBehavior.WANDER, wolfID)
        });

        // OBJECT IDS
        ObjectIdentity bedID = new ObjectIdentity(bedPrefab, new List<Vector3Int> { Vector3Int.zero, 
            new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 0), new Vector3Int(1, 0, 1) });
        objectIdentities.Add(bedID);
        ObjectIdentity lampID = new ObjectIdentity(lampPrefab, new List<Vector3Int> { Vector3Int.zero });
        objectIdentities.Add(lampID);

        // OBJECT INSTANTIATIONS
        TangibleObject bed = new TangibleObject(new Vector3Int(857, 29, 350), Direction.NORTH, bedID);
        TangibleObject lamp = new TangibleObject(new Vector3Int(858, 33, 351), Direction.NORTH, lampID);

        environmentObjects = new Dictionary<int, List<Spawnable>> {
            {
                3, new List<Spawnable> {
                    party,
                    commoner,
                    battleGroup1.combatants[0],
                    battleGroup1.combatants[1],
                    battleGroup2.combatants[0],
                    battleGroup2.combatants[1],
                    new SceneExitCube(
                        new Vector3Int(864, 29, 351),
                        new EnvChangeDestination(1, new Vector3Int(466, 29, -46)),
                        sceneExitPrefab),
                    new StoryEventCube(
                        new Vector3Int(856, 36, 350), 1, storyEventCubePrefab,
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
            },
            {
                1, new List<Spawnable> {
                    party,
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(463, 29, -46),
                        new EnvChangeDestination(2, new Vector3Int(864, 29, 348)),
                        sceneExitPrefab)
                }
            },
            {
                2, new List<Spawnable> {
                    party,
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(-3, 44, 83),
                        new EnvChangeDestination(3, new Vector3Int(864, 29, 348)),
                        sceneExitPrefab)
                }
            }
        };
    }

    public Vector3Int GetPlayerStartPosition(int environmentIndex) {
        List<Spawnable> nonVoxelEntities = GetEnvEntities(environmentIndex);
        foreach (Spawnable nonVoxelEntity in nonVoxelEntities) {
            if (nonVoxelEntity.GetType() == typeof(Party)) {
                Party party = (Party)nonVoxelEntity;
                return party.mainCharacter.startPosition;
            }
        }
        throw new KeyNotFoundException($"No player position for env {environmentIndex} found.");
    }

    public List<Spawnable> GetEnvEntities(int environmentIndex) {
        return environmentObjects.GetValueOrDefault(environmentIndex, new List<Spawnable>());
    }
}
