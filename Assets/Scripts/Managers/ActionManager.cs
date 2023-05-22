using GameMechanics;
using Instantiated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public List<ActionSO> GetActions(Traveller traveller) {
        List<ActionSO> actions = new();
        StatsSO stats = traveller.GetStats();
        foreach (ActionSO action in stats.actions) {
            actions.Add(action);
        }

        foreach (Feature feature in stats.features) {
            foreach (ActionSO action in feature.providedActions) {
                actions.Add(action);
            }
        }

        return actions;
    }
}
