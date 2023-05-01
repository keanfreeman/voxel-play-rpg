using EntityDefinition;
using Instantiated;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saving {
    [Serializable]
    public class SaveData {
        public EnvChangeDestination currDestination;
        public Dictionary<int, List<Entity>> sceneEntityState;

        public SaveData() { }

        public SaveData(string jsonSave) {
            LoadFromJson(jsonSave);
        }

        public string ToJson() {
            return JsonConvert.SerializeObject(this, 
                new JsonSerializerSettings { Formatting = Formatting.Indented, 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, 
                    TypeNameHandling = TypeNameHandling.All});
        }

        private void LoadFromJson(string json) {
            SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json, 
                new JsonSerializerSettings{ TypeNameHandling = TypeNameHandling.All });
            this.currDestination = saveData.currDestination;
            this.sceneEntityState = saveData.sceneEntityState;
        }
    }
}
