using NonVoxelEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelPlay;

public class EnvironmentSceneManager : MonoBehaviour
{
    [SerializeField] private VoxelWorldManager voxelWorldManager;
    [SerializeField] private NonVoxelManager nonVoxelManager;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private GameObject playerInstance;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject playerSeeThroughTarget;
    [SerializeField] private CameraManager cameraManager;

    private Destination destination;

    void Awake() {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        destination = new Destination(3, Vector3Int.zero);
    }

    private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode) {
        VoxelPlayEnvironment environment = FindObjectOfType<VoxelPlayEnvironment>();
        if (environment == null) {
            // load scene after init scene
            SceneManager.LoadScene(3);
            return;
        }

        voxelWorldManager.SetVoxelPlayEnvironment(environment);
        environment.cameraMain = cameraManager.GetMainCamera();
        cameraManager.AttachCameraToPlayer();
        
        nonVoxelManager.SetUpEntities(destination.destinationEnv);
        gameStateManager.controlState = ControlState.SPRITE_NEUTRAL;
    }

    public void LoadNextScene(Destination destination) {
        gameStateManager.controlState = ControlState.LOADING;
        playerMovement.HaltMovement();
        this.destination = destination;
        voxelWorldManager.SetVoxelPlayEnvironment(null);
        nonVoxelManager.DestroyEntities();
        SceneManager.LoadScene(destination.destinationEnv);
    }
}
