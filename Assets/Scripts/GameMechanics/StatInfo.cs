using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public class StatInfo
    {
        public static ActionSO GetRangedAction(StatsSO stats) {
            foreach (ActionSO action in stats.actions) {
                if (action.GetType() == typeof(MultiattackSO)) {
                    MultiattackSO multiattack = (MultiattackSO)action;
                    foreach (AttackSO attack in multiattack.attacks) {
                        if (attack.isRanged) {
                            return multiattack;
                        }
                    }
                }
                else if (action.GetType() == typeof(ActionSO)) {
                    AttackSO attack = (AttackSO)action;
                    if (attack.isRanged) {
                        return attack;
                    }
                }
            }
            return null;
        }

        // Falls back on ranged option if available
        public static ActionSO GetMeleeActionThenRanged(StatsSO stats) {
            foreach (ActionSO action in stats.actions) {
                if (action.GetType() == typeof(MultiattackSO)) {
                    MultiattackSO multiattack = (MultiattackSO)action;
                    foreach (AttackSO attack in multiattack.attacks) {
                        if (!attack.isRanged) {
                            return multiattack;
                        }
                    }
                }
                else if (action.GetType() == typeof(AttackSO)) {
                    AttackSO attack = (AttackSO)action;
                    if (!attack.isRanged) {
                        return attack;
                    }
                }
            }

            // no melee option found
            return GetRangedAction(stats);
        }
    }
}
