using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomComponents {
    public class ActionChoiceBar : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory: UxmlFactory<ActionChoiceBar> {}

        private const string styleResource = "CombatStyle";
        
        private struct Selectors {
            public const string ACTION_CHOICE_BAR = "action_choice_bar";
            public const string ACTION_HOLDER = "action_holder";
        }
        private VisualElement actionHolder;
        private VisualElement bonusActionHolder;

        public ActionChoiceBar() {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
            AddToClassList(Selectors.ACTION_CHOICE_BAR);

            actionHolder = new VisualElement();
            actionHolder.AddToClassList(Selectors.ACTION_HOLDER);
            bonusActionHolder = new VisualElement();
            bonusActionHolder.AddToClassList(Selectors.ACTION_HOLDER);

            actionHolder.Add(new ActionChoiceSlotComponent("Action1"));

            Add(actionHolder);
        }

        public void PopulateBar(List<string> actions, List<string> bonusActions,
                Button actionButton, Button bonusActionButton) {
            actionHolder.Clear();
            bonusActionHolder.Clear();

            foreach (string action in actions) {
                ActionChoiceSlotComponent component = new ActionChoiceSlotComponent(action);
                actionHolder.Add(component);
            }
            foreach (string bonusAction in bonusActions) {
                ActionChoiceSlotComponent component = new ActionChoiceSlotComponent(bonusAction);
                bonusActionHolder.Add(component);
            }

            actionButton.clicked += ActionButton_clicked;
            bonusActionButton.clicked += BonusActionButton_clicked;

            Add(actionHolder);
        }

        private void ActionButton_clicked() {
            Clear();
            Add(actionHolder);
        }
        
        private void BonusActionButton_clicked() {
            Clear();
            Add(bonusActionHolder);
        }
    }
}
