using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : NonVoxelEntity {
    public BattleGroup battleGroup { get; set; }

    public NPC(GameObject prefab, Vector3Int startPosition) : base(prefab, startPosition) {
        this.prefab = prefab;
        this.startPosition = startPosition;
    }
}

public class BattleGroup {
    public Guid groupID = Guid.NewGuid();
    public List<NPC> battleGroup { get; set; }

    public BattleGroup(List<NPC> battleGroup) {
        this.battleGroup = battleGroup;
    }
}
