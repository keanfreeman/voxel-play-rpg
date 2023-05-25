using DieNamespace;
using GameMechanics;
using Instantiated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "ScriptableObjects/Actions/Attack")]
public class AttackSO : ActionSO
{
    public Die attackRoll = new(1, 20, 0);
    public Die damageRoll;
    public DamageType damageType = DamageType.Bludgeoning;
    public bool isRanged = false;
    public int shortRange = 0;
    public int longRange = 0;

    public AttackFeature attackFeature = AttackFeature.None;

    public string GetDescription() {
        string rangeString = isRanged ? $"{shortRange}/{longRange}" : "None";
        return $"Attack Roll: {attackRoll}\n" +
                $"Damage: {damageRoll} {damageType}\n" +
                $"Range: {rangeString}";
    }
}
