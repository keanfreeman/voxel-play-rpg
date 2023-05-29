using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionSO : ScriptableObject
{
    public string actionName;
    public ActionType actionType;
}

public enum ActionType {
    Action,
    BonusAction,
    Reaction,
    FreeAction
}
