using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class IdentitySO : ScriptableObject
{
    // done instead of direct gameobject for serialization
    public GameObject prefab;
}
