using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiattack : GameMechanics.Action
{
    public List<Attack> attacks { get; private set; }

    public Multiattack(List<Attack> attacks) {
        this.name = "Multiattack";
        this.attacks = attacks;
    }
}
