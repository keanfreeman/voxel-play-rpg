using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NonVoxelEntity;

public class NonVoxelInitialization {
    private GameObject playerPrefab;
    private GameObject opossumPrefab;
    private GameObject sceneExitPrefab;

    public Dictionary<SceneIndex, List<Entity>> sceneObjects;

    public NonVoxelInitialization(GameObject playerPrefab, GameObject opossumPrefab,
            GameObject sceneExitPrefab) {
        this.playerPrefab = playerPrefab;
        this.opossumPrefab = opossumPrefab;
        this.sceneExitPrefab = sceneExitPrefab;

        NPCStats wolfStats = new NPCStats("Wolf", "1/4", 13, 40, 11, 12, 15, 12, 3, 12, 6);
        PlayerStats playerStats = new PlayerStats("Player1", 1, 30, 10, 10, 10, 10, 10, 10, 10);

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

        sceneObjects = new Dictionary<SceneIndex, List<Entity>> {
            {
                SceneIndex.SECOND_SCENE, new List<Entity> {
                    new PlayerCharacter(playerPrefab, new Vector3Int(864, 29, 348), playerStats),
                    battleGroup1.combatants[0],
                    battleGroup1.combatants[1],
                    battleGroup2.combatants[0],
                    battleGroup2.combatants[1],
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(864, 29, 351),
                        new Destination(SceneIndex.FOURTH_SCENE, new Vector3Int(466, 26, -46)))
                }
            },
            {
                SceneIndex.FOURTH_SCENE, new List<Entity> {
                    new PlayerCharacter(playerPrefab, new Vector3Int(466, 26, -40), playerStats),
                    battleGroup3.combatants[0],
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(466, 26, -44),
                        new Destination(SceneIndex.SECOND_SCENE, new Vector3Int(864, 29, 348)))
                }
            }
        };
    }

    public Vector3Int GetPlayerStartPosition(SceneIndex sceneIndex) {
        List<Entity> nonVoxelEntities = GetSceneObjects(sceneIndex);
        foreach (Entity nonVoxelEntity in nonVoxelEntities) {
            if (nonVoxelEntity.GetType() == typeof(PlayerCharacter)) {
                return nonVoxelEntity.startPosition;
            }
        }
        throw new KeyNotFoundException($"No player position for scene {sceneIndex} found.");
    }

    public List<Entity> GetSceneObjects(SceneIndex sceneIndex) {
        return sceneObjects.GetValueOrDefault(sceneIndex, new List<Entity>());
    }
}
