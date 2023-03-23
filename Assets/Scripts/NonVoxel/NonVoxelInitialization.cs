using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NonVoxelEntity;
using GameMechanics;
using GameMechanics.Wolf;

public class NonVoxelInitialization {
    private GameObject playerPrefab;
    private GameObject opossumPrefab;
    private GameObject sceneExitPrefab;

    public Dictionary<int, List<Entity>> environmentObjects;

    public NonVoxelInitialization(GameObject playerPrefab, GameObject opossumPrefab,
            GameObject sceneExitPrefab) {
        this.playerPrefab = playerPrefab;
        this.opossumPrefab = opossumPrefab;
        this.sceneExitPrefab = sceneExitPrefab;

        List<Action> wolfActions = new List<Action> {
            new Bite("Bite", new Dice(2, 4, 2), new Dice(1, 20, 4))
        };
        NPCStats wolfStats = new NPCStats("Wolf", "1/4", 13, 40, 11, 12, 15, 12, 3, 12, 6, wolfActions);
        List<Action> playerActions = new List<Action> {
            new Attack("Shortsword", new Dice(1, 6, 0), new Dice(1, 20, 5))
        };
        PlayerStats playerStats = new PlayerStats("Player1", 1, 30, 10, 10, 10, 10, 10, 10, 10, playerActions);

        BattleGroup battleGroup1 = new BattleGroup(new List<NPC> {
            new NPC(opossumPrefab, new Vector3Int(835, 29, 349), wolfStats),
            new NPC(opossumPrefab, new Vector3Int(835, 29, 347), wolfStats)
        });
        BattleGroup battleGroup2 = new BattleGroup(new List<NPC> {
            new NPC(opossumPrefab, new Vector3Int(825, 31, 349), wolfStats),
            new NPC(opossumPrefab, new Vector3Int(825, 31, 350), wolfStats)
        });
        BattleGroup battleGroup3 = new BattleGroup(new List<NPC> {
            new NPC(opossumPrefab, new Vector3Int(468, 26, -46), wolfStats)
        });

        environmentObjects = new Dictionary<int, List<Entity>> {
            {
                3, new List<Entity> {
                    new PlayerCharacter(playerPrefab, new Vector3Int(864, 29, 348), playerStats),
                    battleGroup1.combatants[0],
                    battleGroup1.combatants[1],
                    battleGroup2.combatants[0],
                    battleGroup2.combatants[1],
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(864, 29, 351),
                        new Destination(1, new Vector3Int(466, 29, -46)))
                }
            },
            {
                1, new List<Entity> {
                    new PlayerCharacter(playerPrefab, new Vector3Int(466, 29, -46), playerStats),
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(463, 29, -46),
                        new Destination(2, new Vector3Int(864, 29, 348)))
                }
            },
            {
                2, new List<Entity> {
                    new PlayerCharacter(playerPrefab, new Vector3Int(-3, 44, 85), playerStats),
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(-3, 44, 83),
                        new Destination(3, new Vector3Int(864, 29, 348)))
                }
            }
        };
    }

    public Vector3Int GetPlayerStartPosition(int environmentIndex) {
        List<Entity> nonVoxelEntities = GetEnvEntities(environmentIndex);
        foreach (Entity nonVoxelEntity in nonVoxelEntities) {
            if (nonVoxelEntity.GetType() == typeof(PlayerCharacter)) {
                return nonVoxelEntity.startPosition;
            }
        }
        throw new KeyNotFoundException($"No player position for env {environmentIndex} found.");
    }

    public List<Entity> GetEnvEntities(int environmentIndex) {
        return environmentObjects.GetValueOrDefault(environmentIndex, new List<Entity>());
    }
}
