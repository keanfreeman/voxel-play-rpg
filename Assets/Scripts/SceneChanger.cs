using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneIndex {
    NONE = -1,
    INIT_SCENE = 0,
    SECOND_SCENE = 1,
    THIRD_SCENE = 2,
    FOURTH_SCENE = 3
}

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject detachedCameraPrefab;
    [SerializeField] private GameObject opossumPrefab;
    [SerializeField] private GameObject sceneExitPrefab;
    [SerializeField] private GameObject uiDocument;

    GameObject playerInstance;
    Vector3Int playerStartPosition;
    GameObject detachedCameraInstance;

    NonVoxelInitialization nonVoxelInitialization;

    private class SceneInfo {
        public Scene scene { get; }
        public SceneBuilder sceneBuilder { get; set; }

        public SceneInfo(Scene scene, SceneBuilder sceneBuilder) {
            this.scene = scene;
            this.sceneBuilder = sceneBuilder;
        }
    }

    private Dictionary<SceneIndex, SceneInfo> loadedScenes = new Dictionary<SceneIndex, SceneInfo>();

    void Awake() {
        nonVoxelInitialization = new NonVoxelInitialization(playerPrefab, opossumPrefab, sceneExitPrefab);

        loadedScenes[SceneIndex.INIT_SCENE] = new SceneInfo(SceneManager.GetActiveScene(), null);

        int secondSceneIndex = (int)SceneIndex.SECOND_SCENE;
        SceneManager.LoadScene(secondSceneIndex);
        Scene secondScene = SceneManager.GetSceneByBuildIndex(secondSceneIndex);
        loadedScenes[SceneIndex.SECOND_SCENE] = new SceneInfo(secondScene, null);

        playerStartPosition = nonVoxelInitialization.GetPlayerStartPosition(SceneIndex.SECOND_SCENE);
        playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);

        detachedCameraInstance = Instantiate(detachedCameraPrefab, playerStartPosition, Quaternion.identity);

        DontDestroyOnLoad(playerInstance);
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(detachedCameraInstance);
    }

    private void Update() {
        // load all scenebuilders
        bool allPopulated = true;
        foreach (KeyValuePair<SceneIndex, SceneInfo> sceneInfo in loadedScenes) {
            if (sceneInfo.Key != SceneIndex.INIT_SCENE && sceneInfo.Value.sceneBuilder == null) {
                allPopulated = false;
                if (!sceneInfo.Value.scene.isLoaded) {
                    return;
                }
                playerInstance.transform.SetPositionAndRotation(playerStartPosition, Quaternion.identity);
                loadedScenes[sceneInfo.Key].sceneBuilder = GetSceneBuilderForScene(loadedScenes[sceneInfo.Key].scene);
                loadedScenes[sceneInfo.Key].sceneBuilder.Init(uiDocument, playerStartPosition, playerInstance,
                    nonVoxelInitialization.GetSceneObjects(sceneInfo.Key), detachedCameraInstance);
            }
        }
        if (!allPopulated) {
            return;
        }

        gameObject.SetActive(false);
    }

    private SceneBuilder GetSceneBuilderForScene(Scene scene) {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject item in rootObjects) {
            SceneBuilder sceneBuilder = item.GetComponent<SceneBuilder>();
            if (sceneBuilder != null) {
                sceneBuilder.parentSceneChanger = this;
                return sceneBuilder;
            }
        }
        throw new MissingReferenceException("Could not find a scenebuilder for scene " + scene.name);
    }

    public void LoadNextScene(SceneIndex currentScene, Destination destination) {
        SceneManager.LoadScene((int)destination.destinationScene);

        playerStartPosition = destination.destinationTile;

        Scene destinationScene = SceneManager.GetSceneByBuildIndex((int)destination.destinationScene);
        loadedScenes.Remove(currentScene);
        currentScene = (SceneIndex)destinationScene.buildIndex;
        loadedScenes[currentScene] = new SceneInfo(destinationScene, null);

        gameObject.SetActive(true);
    }
}
