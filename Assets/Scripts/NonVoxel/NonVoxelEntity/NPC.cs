using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : NonVoxelEntity {
    public BattleGroup battleGroup { get; set; }
    public NPCStats stats { get; private set; }

    public NPC(GameObject prefab, Vector3Int startPosition, NPCStats stats)
            : base(prefab, startPosition) {
        this.prefab = prefab;
        this.startPosition = startPosition;
        this.stats = stats;
    }
}

public struct NPCStats {
    public string name { get; private set; }
    public string challengeRating { get; private set; }

    public int armorClass { get; private set; }
    public int speed { get; private set; }
    public int hitPoints { get; private set; }

    public int strength { get; private set; }
    public int dexterity { get; private set; }
    public int constitution { get; private set; }
    public int intelligence { get; private set; }
    public int wisdom { get; private set; }
    public int charisma { get; private set; }

    public NPCStats(string name, string challengeRating, int armorClass, int speed, 
            int hitPoints, int strength, int dexterity, int constitution, int intelligence, 
            int wisdom, int charisma) {
        this.name = name;
        this.challengeRating = challengeRating;
        this.armorClass = armorClass;
        this.speed = speed;
        this.hitPoints = hitPoints;
        this.strength = strength;
        this.dexterity = dexterity;
        this.constitution = constitution;
        this.intelligence = intelligence;
        this.wisdom = wisdom;
        this.charisma = charisma;
    }
}

public class BattleGroup {
    public Guid groupID = Guid.NewGuid();
    public List<NPC> combatants { get; set; }

    public BattleGroup(List<NPC> combatants) {
        this.combatants = combatants;
    }
}
