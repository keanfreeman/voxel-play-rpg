using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomComponents {
    public class ActionChoice: Button
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory: UxmlFactory<ActionChoice> {}

        private const string styleResource = "CombatStyle";
        private const string classActionButton = "ActionButton";

        public event System.Action<ActionChoice> gainedFocus;
        public event System.Action<ActionChoice> lostFocus;
        public event System.Action<ActionChoice> selected;
        public ActionSO currAction { get; private set; }
        public int position { get; set; }

        public ActionChoice() {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));

            this.text = "Prestidigitation";
            RegisterCallback<FocusInEvent>(OnActionButtonFocusGained);
            RegisterCallback<FocusOutEvent>(OnActionButtonLostFocus);
            RegisterCallback<ClickEvent>(OnActionButtonClicked);
            AddToClassList(classActionButton);
        }

        public ActionChoice(string actionName) : this() {
            text = actionName;
        }

        public void UpdateAction(ActionSO action) {
            currAction = action;
            text = action.actionName;
        }

        public void ClearAction() {
            currAction = null;
            text = "";
        }

        private void OnActionButtonFocusGained(FocusInEvent evt) {
            gainedFocus?.Invoke(this);
        }

        private void OnActionButtonLostFocus(FocusOutEvent evt) {
            lostFocus?.Invoke(this);
        }

        private void OnActionButtonClicked(ClickEvent evt) {
            selected?.Invoke(this);
        }
    }
}
