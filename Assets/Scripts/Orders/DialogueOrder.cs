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

        [JsonConstructor]
        public DialogueOrder(string storyText, string speakerName) {
            this.storyText = storyText;
            this.speakerName = speakerName;
        }

        public DialogueOrder(string storyRawInkText) {
            Compiler inkCompiler = new Compiler(storyRawInkText);
            this.storyText = inkCompiler.Compile().ToJson();
        }

        public DialogueOrder(TextAsset story, string speakerName) {
            this.storyText = story.text;
            this.speakerName = speakerName;
        }
    }
}
