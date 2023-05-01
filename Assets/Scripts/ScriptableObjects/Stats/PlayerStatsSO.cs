using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStats", menuName = "ScriptableObjects/Stats/PlayerStats")]
public class PlayerStatsSO : StatsSO
{
    public int level = 1;

    public override int CalculateArmorClass() {
        return 10 + StatModifiers.GetModifierForStat(dexterity);
    }
}
