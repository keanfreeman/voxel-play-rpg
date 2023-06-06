using EntityDefinition;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saving {
    [Serializable]
    public class SaveData {
        public EnvChangeDestination currDestination;
        public Dictionary<int, SceneInfo> sceneEntityState;

        public PlayerCharacter currControlledCharacter;
        public bool usedShortRest;

        public TimeRemaining timeRemaining;

        public SaveData() {}

        public string ToJson() {
            return JsonConvert.SerializeObject(this, 
                new JsonSerializerSettings { Formatting = Formatting.Indented, 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, 
                    TypeNameHandling = TypeNameHandling.All});
        }

        public static SaveData CreateFromJson(string json) {
            return JsonConvert.DeserializeObject<SaveData>(json, 
                new JsonSerializerSettings{ TypeNameHandling = TypeNameHandling.All });
        }
    }
}
