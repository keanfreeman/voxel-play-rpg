using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(fileName = "New Traveller Identity", menuName = "ScriptableObjects/Identity/Traveller")]
public class TravellerIdentitySO : IdentitySO
{
    public SpriteLibraryAsset spriteLibraryAsset;
    public Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 scale = new Vector3(1, 1, 1);
    public StatsSO stats;
}
