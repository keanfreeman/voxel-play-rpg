using Ink.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class DialogueOrder : Order
    {
        public string storyText;
        public string speakerName;

        [JsonConstructor]
        public DialogueOrder(string storyText, string speakerName) {
            this.storyText = storyText;
            this.speakerName = speakerName;
        }

        public DialogueOrder(TextAsset story, string speakerName) {
            this.storyText = story.text;
            this.speakerName = speakerName;
        }
    }
}
