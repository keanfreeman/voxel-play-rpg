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

        sceneObjects = new Dictionary<SceneIndex, List<NonVoxelEntity>> {
            {
                SceneIndex.SECOND_SCENE, new List<NonVoxelEntity> {
                    new NonVoxelEntity(playerPrefab, new Vector3Int(523, 50, 246)),
                    new NonVoxelEntity(opossumPrefab, new Vector3Int(527, 53, 247)),
                    new NonVoxelEntity(opossumPrefab, new Vector3Int(521, 50, 246)),
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(523, 50, 249),
                        new Destination(SceneIndex.FOURTH_SCENE, new Vector3Int(466, 26, -46)))
                }
            },
            {
                SceneIndex.FOURTH_SCENE, new List<NonVoxelEntity> {
                    new NonVoxelEntity(playerPrefab, new Vector3Int(466, 26, -46)),
                    new NonVoxelEntity(opossumPrefab, new Vector3Int(468, 26, -46)),
                    new SceneExitCube(
                        sceneExitPrefab,
                        new Vector3Int(466, 26, -43),
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
