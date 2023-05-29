using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomComponents {
    public class StatusComponent : Label {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<StatusComponent> { }

        private const string styleResource = "PlayerStatusStyle";

        public StatusComponent() {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
            AddToClassList("TextBackground");

            text = "Status Effect (Condition1, Condition2)";
        }

        public StatusComponent(string statusName) : this() {
            this.text = statusName;
        }
    }
}
