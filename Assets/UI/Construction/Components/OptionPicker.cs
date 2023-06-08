using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomComponents {
    public class OptionPicker : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<OptionPicker> { }

        public Label label { get; private set; }

        private const string styleResource = "ConstructionStyle";

        public OptionPicker() : this("DefaultText") {}

        public OptionPicker(string labelText) {
            StyleSheet styleSheet = Resources.Load<StyleSheet>(styleResource);
            styleSheets.Add(styleSheet);

            name = "OptionPicker";
            focusable = true;

            hierarchy.Add(new VisualElement { name = "LeftImage" });

            label = new() { name = "TextHolder", text = labelText };
            hierarchy.Add(label);

            hierarchy.Add(new VisualElement { name = "RightImage" });
        }
    }
}
