using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameMechanics {
    public class CurrentStatus {
        private Dictionary<StatusEffect, OngoingEffect> ongoingEffects = new();

        public OngoingEffect Get(StatusEffect statusEffect) {
            return ongoingEffects.GetValueOrDefault(statusEffect, null);
        }

        public void Add(StatusEffect statusEffect, OngoingEffect ongoingEffect) {
            ongoingEffects[statusEffect] = ongoingEffect;
        }

        public void Remove(StatusEffect statusEffect) {
            if (ongoingEffects.ContainsKey(statusEffect)) {
                ongoingEffects.Remove(statusEffect);
            }
        }

        public bool IsParalyzed() {
            return ongoingEffects.Values
                .Select(ongoingEffect => ongoingEffect.conditions)
                .Where(conditions => conditions.Contains(Condition.Paralyzed))
                .Count() > 0;
        }
    }

    public enum Condition {
        Blinded,
        Charmed,
        Deafened,
        Frightened,
        Grappled,
        Incapacitated,
        Invisible,
        // todo - implement autofailed dex/str saves
        Paralyzed,
        Petrified,
        Poisoned,
        Prone,
        Restrained,
        Stunned,
        Unconscious
    }

    // describes a specific effect, e.g. (a ghoul's claw and everything associated), rather
    // than the condition game mechanic (e.g. paralysis)
    public enum StatusEffect {
        GhoulClaw
    }

    public class OngoingEffect {
        public StatusEffect cause;
        public HashSet<Condition> conditions;
        public int turnsLeft;

        public OngoingEffect(StatusEffect cause, IEnumerable<Condition> conditions, int turnsLeft) {
            this.cause = cause;
            this.conditions = conditions.ToHashSet();
            this.turnsLeft = turnsLeft;
        }
    }

    public static class ConditionEnumExtension {
        private static readonly IReadOnlyCollection<string> incapacitatedDescription = new List<string> {
            "An incapacitated creature can�t take actions, bonus actions, or reactions."
        };
        private static readonly IReadOnlyCollection<string> paralyzedDescription = new List<string> {
            "A paralyzed creature is incapacitated (see the condition) and can�t move or speak.",
            "The creature automatically fails Strength and Dexterity saving throws. " +
                "Attack rolls against the creature have advantage.",
            "Any attack that hits the creature is a critical hit if the attacker is " +
                "within 5 feet of the creature."
        };

        public static IReadOnlyCollection<string> GetDescription(Condition condition) {
            switch (condition) {
                case Condition.Blinded:
                    return new List<string>{};
                case Condition.Charmed:
                    return new List<string> {};
                case Condition.Deafened:
                    return new List<string> {};
                case Condition.Frightened:
                    return new List<string> {};
                case Condition.Grappled:
                    return new List<string> {};
                case Condition.Incapacitated:
                    return incapacitatedDescription;
                case Condition.Invisible:
                    return new List<string> {};
                case Condition.Paralyzed:
                    return paralyzedDescription;
                case Condition.Petrified:
                    return new List<string> {};
                case Condition.Poisoned:
                    return new List<string> {};
                case Condition.Prone:
                    return new List<string> {};
                case Condition.Restrained:
                    return new List<string> {};
                case Condition.Stunned:
                    return new List<string> {};
                case Condition.Unconscious:
                    return new List<string> {};
                default:
                    throw new System.NotImplementedException($"No description for enum {condition}");
            }
        }
    }
}