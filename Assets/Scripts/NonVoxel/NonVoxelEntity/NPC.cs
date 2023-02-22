using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : NonVoxelEntity {
    public List<NPC> battleGroup { get; set; }

    public NPC(GameObject prefab, Vector3Int startPosition) : base(prefab, startPosition) {
        this.prefab = prefab;
        this.startPosition = startPosition;
    }
}
