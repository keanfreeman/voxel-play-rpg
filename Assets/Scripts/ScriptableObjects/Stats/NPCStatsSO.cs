using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPCStats", menuName = "ScriptableObjects/Stats/NPCStats")]
public class NPCStatsSO : StatsSO
{
    public string statblockName;
    public string challengeRating;
    public int armorClass;

    public override int CalculateArmorClass(CurrentStatus _) {
        return armorClass;
    }
}
