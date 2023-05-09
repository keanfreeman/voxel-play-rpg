using Ink;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public DialogueOrder(string storyRawInkText, string speakerName) {
            Compiler inkCompiler = new Compiler(storyRawInkText);
            this.storyText = inkCompiler.Compile().ToJson();
            this.speakerName = speakerName;
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
