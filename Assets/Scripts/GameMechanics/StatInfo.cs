using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMechanics {
    public class StatInfo
    {
        public static Action GetRangedAction(Stats stats) {
            foreach (Action action in stats.actions) {
                if (action.GetType() == typeof(Multiattack)) {
                    Multiattack multiattack = (Multiattack)action;
                    foreach (Attack attack in multiattack.attacks) {
                        if (attack.isRanged) {
                            return multiattack;
                        }
                    }
                }
                else if (action.GetType() == typeof(Attack)) {
                    Attack attack = (Attack)action;
                    if (attack.isRanged) {
                        return attack;
                    }
                }
            }
            return null;
        }

        // Falls back on ranged option if available
        public static Action GetMeleeActionThenRanged(Stats stats) {
            foreach (Action action in stats.actions) {
                if (action.GetType() == typeof(Multiattack)) {
                    Multiattack multiattack = (Multiattack)action;
                    foreach (Attack attack in multiattack.attacks) {
                        if (!attack.isRanged) {
                            return multiattack;
                        }
                    }
                }
                else if (action.GetType() == typeof(Attack)) {
                    Attack attack = (Attack)action;
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
