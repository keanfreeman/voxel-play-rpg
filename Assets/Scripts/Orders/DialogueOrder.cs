using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orders {
    public class DialogueOrder : Order
    {
        public Story story { get; private set; }
        public string speakerName { get; private set; }

        public DialogueOrder(TextAsset story, string speakerName) {
            this.story = new Story(story.text);
            this.speakerName = speakerName;
        }
    }
}
