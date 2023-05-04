using Ink;
using Ink.Runtime;
using Ink.UnityIntegration;
using Newtonsoft.Json;
using Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Orders {
    [Serializable]
    public class DialogueOrder : Order
    {
        public string storyText;
        public string speakerName;
        public Dictionary<string, Guid> joinPartyTargets;

        [JsonConstructor]
        public DialogueOrder(string storyText, string speakerName, 
                Dictionary<string, Guid> joinPartyTargets) {
            this.storyText = storyText;
            this.speakerName = speakerName;
            this.joinPartyTargets = joinPartyTargets;
        }

        public DialogueOrder(string storyRawInkText) {
            Compiler inkCompiler = new Compiler(storyRawInkText);
            this.storyText = inkCompiler.Compile().ToJson();
        }

        public DialogueOrder(TextAsset story, Dictionary<string, Guid> joinPartyTargets) {
            this.storyText = story.text;
            this.speakerName = null;
            this.joinPartyTargets = joinPartyTargets;
        }

        public DialogueOrder(TextAsset story, string speakerName) {
            this.storyText = story.text;
            this.speakerName = speakerName;
        }
    }
}
