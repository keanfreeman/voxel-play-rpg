using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomComponents {
    public class ActionChoiceSlotComponent : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory: UxmlFactory<ActionChoiceSlotComponent> {}

        private const string styleResource = "CombatStyle";
        private const string classActionButton = "action_button";

        public Action clicked;

        private Button actionButton;

        public ActionChoiceSlotComponent() {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));

            actionButton = new Button {text = "Action1"};
            actionButton.RegisterCallback<FocusInEvent>(OnActionButtonFocusGained);
            actionButton.RegisterCallback<FocusOutEvent>(OnActionButtonLostFocus);
            actionButton.clicked += ActionButton_clicked;
            actionButton.AddToClassList(classActionButton);
            hierarchy.Add(actionButton);
        }

        public ActionChoiceSlotComponent(string actionName) : this() {
            actionButton.text = actionName;
        }

        private void ActionButton_clicked() {
            clicked?.Invoke();
        }

        private void OnActionButtonFocusGained(FocusInEvent evt) {
            Debug.Log("Button gained focus");
        }

        private void OnActionButtonLostFocus(FocusOutEvent evt) {
            Debug.Log("Button lost focus");
        }
    }
}
