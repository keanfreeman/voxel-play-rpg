using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Multiattack", menuName = "ScriptableObjects/Actions/Multiattack")]
public class MultiattackSO : ActionSO
{
    public List<AttackSO> attacks;
}
