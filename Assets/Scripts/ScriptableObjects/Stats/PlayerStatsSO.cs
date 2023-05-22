using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStats", menuName = "ScriptableObjects/Stats/PlayerStats")]
public class PlayerStatsSO : StatsSO
{
    public int level = 1;
    // todo - after implementing inventory/equipment, no longer need to set this
    public int armorClass;

    public override int CalculateArmorClass() {
        return armorClass;
    }
}
