using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    public System.Random rng { get; private set; }
    public Dice dice { get; private set; }

    void Awake() {
        rng = new System.Random();
        dice = new Dice(rng);
    }
}
