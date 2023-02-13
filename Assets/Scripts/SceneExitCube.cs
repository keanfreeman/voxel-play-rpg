using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExitCube : MonoBehaviour
{
    private SceneChanger sceneChanger;

    public void Init(SceneChanger sceneChanger) {
        this.sceneChanger = sceneChanger;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            /*
             * playermanager
                * stop all player input
             * map of Scene Exit to next Scene, starting coordinates.
                * figure out where to send this player (scene, coordinates in scene)
             * scenemanager
                * load the scene if it isn't already loaded
                * switch to that scene
             */
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            sceneChanger.LoadScene(currentScene, 3);
        }
    }
}
