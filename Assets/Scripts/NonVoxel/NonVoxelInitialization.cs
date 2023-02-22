using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonVoxelInitialization {
    private GameObject playerPrefab;
    private GameObject opossumPrefab;
    private GameObject sceneExitPrefab;

    public Dictionary<SceneIndex, List<NonVoxelEntity>> sceneObjects;

    public NonVoxelInitialization(GameObject playerPrefab, GameObject opossumPrefab, GameObject sceneExitPrefab) {
        this.playerPrefab = playerPrefab;
        this.opossumPrefab = opossumPrefab;
        this.sceneExitPrefab = sceneExitPrefab;

        List<NPC> battleGroup1 = new List<NPC> {
            new NPC(opossumPrefab, new Vector3Int(835, 29, 349)),
            new NPC(opossumPrefab, new Vector3Int(835, 29, 347))
        };
        foreach (NPC npc in battleGroup1) {
            npc.battleGroup = new BattleGroup(battleGroup1);
        }
        List<NPC> battleGroup2 = new List<NPC> {
            new NPC(opossumPrefab, new Vector3Int(834, 29, 349)),
            new NPC(opossumPrefab, new Vector3Int(833, 29, 349))
        };
        foreach (NPC npc in battleGroup2) {
            npc.battleGroup = new BattleGroup(battleGroup2);
        }
        List<NPC> battleGroup3 = new List<NPC> {
            new NPC(opossumPrefab, new Vector3Int(468, 26, -46))
        };
        foreach (NPC npc in battleGroup3) {
            npc.battleGroup = new BattleGroup(battleGroup3);
        }

        sceneObjects = new Dictionary<SceneIndex, List<NonVoxelEntity>> {
            {
                SceneIndex.SECOND_SCENE, new List<NonVoxelEntity> {
                    new PlayerCharacter(playerPrefab, new Vector3Int(864, 29, 348)),
                    battleGroup1[0],
                    battleGroup1[1],
                    battleGroup2[0],
                    battleGroup2[1],
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(523, 50, 249),
                        new Destination(SceneIndex.FOURTH_SCENE, new Vector3Int(466, 26, -46)))
                }
            },
            {
                SceneIndex.FOURTH_SCENE, new List<NonVoxelEntity> {
                    new PlayerCharacter(playerPrefab, new Vector3Int(466, 26, -66)),
                    battleGroup3[0],
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(466, 26, -44),
                        new Destination(SceneIndex.SECOND_SCENE, new Vector3Int(523, 50, 246)))
                }
            }
        };
    }

    public Vector3Int GetPlayerStartPosition(SceneIndex sceneIndex) {
        List<NonVoxelEntity> nonVoxelEntities = GetSceneObjects(sceneIndex);
        foreach (NonVoxelEntity nonVoxelEntity in nonVoxelEntities) {
            if (nonVoxelEntity.prefab == playerPrefab) {
                return nonVoxelEntity.startPosition;
            }
        }
        throw new KeyNotFoundException($"No player position for scene {sceneIndex} found.");
    }

    public List<NonVoxelEntity> GetSceneObjects(SceneIndex sceneIndex) {
        return sceneObjects.GetValueOrDefault(sceneIndex, new List<NonVoxelEntity>());
    }
}
