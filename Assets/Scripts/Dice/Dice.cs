using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Dice
{
    public int numDice;
    public int diceSize;
    public int modifier;

    public Dice(int numDice, int diceSize) {
        this.numDice = numDice;
        this.diceSize = diceSize;
        this.modifier = 0;
    }

    public Dice(int numDice, int diceSize, int modifier) {
        this.numDice = numDice;
        this.diceSize = diceSize;
        this.modifier = modifier;
    }
}
