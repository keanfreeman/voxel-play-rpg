using EntityDefinition;
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
        [SerializeField] OrderManager orderManager;
        [SerializeField] TimerUIController timerUIController;

        private void Update() {
            if (Input.GetKeyUp(KeyCode.F5)) {
                StartCoroutine(Load());
            }
            else if (Input.GetKeyUp(KeyCode.F9)) {
                Save();
            }
            else if (Input.GetKeyUp(KeyCode.F11)) {
                ResetEntities();
            }
        }

        public bool SaveExists() {
            return FileManager.SaveExists();
        }

        public void ResetEntities() {
            string json = FileManager.ReadSaveJson();
            if (json == null) {
                Debug.LogError("Tried to load a nonexistent file.");
                return;
            }
            SaveData saveData = SaveData.CreateFromJson(json);

            // destroy all npcs, players, etc, for resetting story stuff
            foreach (SceneInfo sceneInfo in saveData.sceneEntityState.Values) {
                HashSet<Entity> entitiesToRemove = new();
                for (int i = 0; i < sceneInfo.entities.Count; i++) {
                    Entity entity = sceneInfo.entities[i];
                    if (!TypeUtils.IsSameTypeOrIsSubclass(entity, typeof(TangibleObject))) {
                        entitiesToRemove.Add(entity);
                    }
                }
                sceneInfo.entities.RemoveAll(e => entitiesToRemove.Contains(e));
            }

            saveData.timeRemaining = null;
            saveData.currControlledCharacter = null;
            saveData.usedShortRest = false;

            FileManager.WriteSaveJson(saveData.ToJson());
            Debug.Log("Reset entities");
        }

        public SaveData ReadFileState() {
            string json = FileManager.ReadSaveJson();
            if (json == null) {
                Debug.LogError("Tried to load a nonexistent file.");
                return null;
            }

            return SaveData.CreateFromJson(json);
        }

        public IEnumerator Load() {
            string json = FileManager.ReadSaveJson();
            if (json == null) {
                Debug.LogError("Tried to load a nonexistent file.");
                yield break;
            }
            SaveData saveData = SaveData.CreateFromJson(json);

            yield return gameStateManager.SetControlState(ControlState.LOADING);

            // destroy existing information
            cameraManager.DeParentCamera();
            nonVoxelWorld.DestroyAllEntities();
            partyManager.ClearData();
            orderManager.ClearData();

            // load new information
            yield return environmentSceneManager.LoadFromSaveData(saveData);
            yield return partyManager.LoadFromSaveData(saveData);
            yield return timerUIController.LoadFromSaveData(saveData);

            Debug.Log("Loaded");
        }

        private void Save() {
            SaveData saveData = new();
            environmentSceneManager.PopulateSaveData(saveData);
            partyManager.PopulateSaveData(saveData);
            timerUIController.PopulateSaveData(saveData);

            FileManager.WriteSaveJson(saveData.ToJson());
            Debug.Log("Saved");
        }
    }
}
