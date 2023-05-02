using Ink.Runtime;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Saving {
    public class SaveManager : MonoBehaviour
    {
        [SerializeField] NonVoxelWorld nonVoxelWorld;
        [SerializeField] PartyManager partyManager;
        [SerializeField] EnvironmentSceneManager environmentSceneManager;
        [SerializeField] CameraManager cameraManager;
        [SerializeField] GameStateManager gameStateManager;

        private void Update() {
            if (Input.GetKeyUp(KeyCode.F5)) {
                StartCoroutine(Load());
            }
            else if (Input.GetKeyUp(KeyCode.F9)) {
                Save();
            }
        }

        public bool SaveExists() {
            return FileManager.SaveExists();
        }

        public IEnumerator Load() {
            string json = FileManager.ReadSaveJson();
            if (json == null) {
                Debug.LogError("Tried to load a nonexistent file.");
                yield break;
            }
            SaveData saveData = new SaveData(json);

            yield return gameStateManager.SetControlState(ControlState.LOADING);

            // destroy existing information
            cameraManager.DeParentCamera();

            nonVoxelWorld.DestroyAllEntities();
            partyManager.ClearData();

            // load new information
            yield return environmentSceneManager.LoadFromSaveData(saveData);
            yield return partyManager.LoadFromSaveData(saveData);

            yield return gameStateManager.SetControlState(ControlState.SPRITE_NEUTRAL);
            Debug.Log("Loaded");
        }

        private void Save() {
            SaveData saveData = new SaveData();
            environmentSceneManager.PopulateSaveData(saveData);
            partyManager.PopulateSaveData(saveData);
            FileManager.WriteSaveJson(saveData.ToJson());
            Debug.Log("Saved");
        }
    }
}
