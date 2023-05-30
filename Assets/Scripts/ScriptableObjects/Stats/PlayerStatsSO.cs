using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStats", menuName = "ScriptableObjects/Stats/PlayerStats")]
public class PlayerStatsSO : StatsSO
{
    public int level = 1;
    // todo - after implementing inventory/equipment, no longer need to set this
    public int armorClass;

    public override int CalculateArmorClass(CurrentStatus currentStatus) {
        if (currentStatus.ongoingEffects.ContainsKey(StatusEffect.MageArmor)) {
            int mageArmorValue = 13 + StatModifiers.GetModifierForStat(dexterity);
            return Mathf.Max(mageArmorValue, armorClass);
        }
        return armorClass;
    }
}
