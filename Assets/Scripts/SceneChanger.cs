using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private GameObject uiDocument;

    enum SceneIndex {
        NONE = -1,
        INIT_SCENE = 0,
        SECOND_SCENE = 1,
        THIRD_SCENE = 2,
    }

    private class SceneInfo {
        public SceneIndex index { get;}
        public Scene scene { get; }
        public SceneBuilder sceneBuilder { get; set; }

        public SceneInfo(SceneIndex index, Scene scene, SceneBuilder sceneBuilder) {
            this.index = index;
            this.scene = scene;
            this.sceneBuilder = sceneBuilder;
        }
    }

    private Dictionary<SceneIndex, SceneInfo> loadedScenes = new Dictionary<SceneIndex, SceneInfo>();

    void Awake() {
        DontDestroyOnLoad(gameObject);

        loadedScenes[SceneIndex.INIT_SCENE] = new SceneInfo(SceneIndex.INIT_SCENE,
            SceneManager.GetActiveScene(), null);

        int secondSceneIndex = (int)SceneIndex.SECOND_SCENE;
        SceneManager.LoadScene(secondSceneIndex);
        Scene secondScene = SceneManager.GetSceneByBuildIndex(secondSceneIndex);
        loadedScenes[SceneIndex.SECOND_SCENE] = new SceneInfo(SceneIndex.SECOND_SCENE,
            secondScene, null);

        int thirdSceneIndex = (int)SceneIndex.THIRD_SCENE;
        Scene thirdScene = LoadSceneAsInactive(thirdSceneIndex);
        loadedScenes[SceneIndex.THIRD_SCENE] = new SceneInfo(SceneIndex.THIRD_SCENE,
            thirdScene, null);
    }

    private void Update() {
        if (!loadedScenes[SceneIndex.SECOND_SCENE].scene.isLoaded
            || !loadedScenes[SceneIndex.THIRD_SCENE].scene.isLoaded
            ) {
            return;
        }

        if (loadedScenes[SceneIndex.SECOND_SCENE].sceneBuilder == null) {
            loadedScenes[SceneIndex.SECOND_SCENE].sceneBuilder =
                GetSceneBuilderForScene(loadedScenes[SceneIndex.SECOND_SCENE].scene);
            loadedScenes[SceneIndex.SECOND_SCENE].sceneBuilder.Init(uiDocument);
            loadedScenes[SceneIndex.SECOND_SCENE].sceneBuilder.enabled = true;
        }
        if (loadedScenes[SceneIndex.THIRD_SCENE].sceneBuilder == null) {
            loadedScenes[SceneIndex.THIRD_SCENE].sceneBuilder =
                GetSceneBuilderForScene(loadedScenes[SceneIndex.THIRD_SCENE].scene);
            loadedScenes[SceneIndex.THIRD_SCENE].sceneBuilder.Init(uiDocument);
            loadedScenes[SceneIndex.THIRD_SCENE].sceneBuilder.enabled = true;
        }

        SceneManager.SetActiveScene(loadedScenes[SceneIndex.SECOND_SCENE].scene);
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

    public Scene LoadSceneAsInactive(int index) {
        SceneManager.LoadScene(index, LoadSceneMode.Additive);
        Scene scene = SceneManager.GetSceneByBuildIndex(index);
        return scene;
    }

    public void SetActiveScene(GameObject playerObject) {
        int newSceneIndex = SceneManager.GetActiveScene().buildIndex == 2 ? 1 : 2;
        Debug.Log("Making " + newSceneIndex + " active");

        Scene nextScene = SceneManager.GetSceneByBuildIndex(newSceneIndex);
        SceneManager.MoveGameObjectToScene(playerObject, nextScene);
        SceneManager.SetActiveScene(nextScene);
        loadedScenes[(SceneIndex)newSceneIndex].sceneBuilder.OnMadeActive(playerObject);
    }

    // not blocking
    public void DestroyScene(Scene scene) {
        SceneManager.UnloadSceneAsync(scene);
    }
}
