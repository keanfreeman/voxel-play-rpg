using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NonVoxelEntity {
    public GameObject prefab { get; protected set; }
    public Vector3Int startPosition { get; protected set; }

    public NonVoxelEntity(GameObject prefab, Vector3Int startPosition) {
        this.prefab = prefab;
        this.startPosition = startPosition;
    }
}
