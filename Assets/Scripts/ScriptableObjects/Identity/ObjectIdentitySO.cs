using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Object Identity", menuName = "ScriptableObjects/Identity/Objects")]
public class ObjectIdentitySO : IdentitySO {
    public List<Vector3Int> occupiedPositions = new List<Vector3Int> { Vector3Int.zero };
}
