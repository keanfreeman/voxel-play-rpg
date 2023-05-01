using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "ScriptableObjects/Actions/Attack")]
public class AttackSO : ActionSO
{
    public Dice attackRoll = new Dice(1, 20, 0);
    public Dice damageRoll;
    public bool isRanged = false;
    public int shortRange = 0;
    public int longRange = 0;
}
