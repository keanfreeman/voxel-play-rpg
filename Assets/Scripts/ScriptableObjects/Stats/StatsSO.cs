using GameMechanics;
using NonVoxelEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatsSO : ScriptableObject
{
    // Speed in feet per 6 seconds
    public int baseSpeed = 30;
    public int hitPoints;
    public EntitySize size = EntitySize.MEDIUM;

    // todo - add GetSavingThrowModifier since NPCs may have special one, and PCs have additives too
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    public List<ActionSO> actions;
    public List<Feature> features;

    public abstract int CalculateArmorClass();
}
