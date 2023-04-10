using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NonVoxelEntity;
using GameMechanics;
using GameMechanics.Wolf;
using UnityEngine.U2D.Animation;

public class NonVoxelInitialization {
    private GameObject playerPrefab;
    private GameObject npcPrefab;
    private GameObject sceneExitPrefab;
    private GameObject bedPrefab;

    public Dictionary<int, List<Spawnable>> environmentObjects;

    public NonVoxelInitialization(GameObject playerPrefab, GameObject npcPrefab,
            GameObject sceneExitPrefab, GameObject bedPrefab) {
        this.playerPrefab = playerPrefab;
        this.npcPrefab = npcPrefab;
        this.sceneExitPrefab = sceneExitPrefab;

        SpriteLibraryAsset playerSpriteLibrary =
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/PlayerSpriteLibrary");
        SpriteLibraryAsset loreleiSpriteLibrary = 
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/LoreleiSpriteLibrary");
        SpriteLibraryAsset yellowSpriteLibrary =
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/YellowSpriteLibrary");
        SpriteLibraryAsset opossumSpriteLibrary =
            Resources.Load<SpriteLibraryAsset>("Borrowed/Sprites/OpossumSpriteLibrary");

        List<Action> playerActions = new List<Action> {
            new Attack("Shortsword", new Dice(1, 20, 5), new Dice(1, 6, 0))
        };
        PlayerStats playerStats = new PlayerStats("Player1", 30, 10, EntitySize.MEDIUM, 10, 10, 10, 10, 10, 
            10, playerActions, 1);
        PlayerCharacter mainCharacter = new PlayerCharacter(new Vector3Int(864, 29, 346), playerStats, 
            new EntityDisplay(playerPrefab, playerSpriteLibrary, new Vector3(0.5f, 0.5f, 0.5f), 
                new Vector3(0.8f, 0.8f, 0.8f)));

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
        EntityDisplay scout1Display = new EntityDisplay(playerPrefab, loreleiSpriteLibrary, 
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.8f, 0.8f, 0.8f));
        EntityDisplay scout2Display = new EntityDisplay(playerPrefab, yellowSpriteLibrary,
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.8f, 0.8f, 0.8f));

        Party party = new Party(
            mainCharacter,
            new List<PlayerCharacter> {
                mainCharacter,
                new PlayerCharacter(new Vector3Int(864, 29, 347), scoutStats, scout1Display),
                new PlayerCharacter(new Vector3Int(864, 29, 348), scoutStats, scout2Display)
            }
        );

        List<Action> wolfActions = new List<Action> {
            new Bite("Bite", new Dice(1, 20, 4), new Dice(2, 4, 2))
        };
        NPCStats wolfStats = new NPCStats("Wolf", 40, 11, EntitySize.LARGE, 12, 15, 12, 3, 12, 6, 
            wolfActions, "1/4", 13);
        EntityDisplay wolfDisplay = new EntityDisplay(npcPrefab, opossumSpriteLibrary, 
            new Vector3(1f, 0.6f, 1f), new Vector3(6.5f, 6.5f, 6.5f));

        BattleGroup battleGroup1 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(835, 29, 350), wolfStats, wolfDisplay),
            new NPC(new Vector3Int(835, 29, 347), wolfStats, wolfDisplay)
        });
        BattleGroup battleGroup2 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(825, 31, 348), wolfStats, wolfDisplay),
            new NPC(new Vector3Int(825, 31, 350), wolfStats, wolfDisplay)
        });
        BattleGroup battleGroup3 = new BattleGroup(new List<NPC> {
            new NPC(new Vector3Int(468, 26, -46), wolfStats, wolfDisplay)
        });

        NonVoxelObject bed = new NonVoxelObject(new Vector3Int(857, 29, 350), 
            new EntityDisplay(bedPrefab), 
            new List<Vector3Int> { Vector3Int.zero, new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 0),
                new Vector3Int(1, 0, 1) },
            MovementDirection.Direction.NORTH);

        environmentObjects = new Dictionary<int, List<Spawnable>> {
            {
                3, new List<Spawnable> {
                    party,
                    battleGroup1.combatants[0],
                    battleGroup1.combatants[1],
                    battleGroup2.combatants[0],
                    battleGroup2.combatants[1],
                    new SceneExitCube(
                        new Vector3Int(864, 29, 351),
                        new Destination(1, new Vector3Int(466, 29, -46)),
                        new EntityDisplay(sceneExitPrefab)),
                    bed
                }
            },
            {
                1, new List<Spawnable> {
                    party,
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(463, 29, -46),
                        new Destination(2, new Vector3Int(864, 29, 348)),
                        new EntityDisplay(sceneExitPrefab))
                }
            },
            {
                2, new List<Spawnable> {
                    party,
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        new Vector3Int(-3, 44, 83),
                        new Destination(3, new Vector3Int(864, 29, 348)),
                        new EntityDisplay(sceneExitPrefab))
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
