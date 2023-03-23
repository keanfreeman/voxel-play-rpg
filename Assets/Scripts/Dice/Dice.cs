using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Dice
{
    public int numDice { get; private set; }
    public int diceSize { get; private set; }
    public int modifier { get; private set; }
    
    public Dice(int numDice, int diceSize, int modifier) {
        this.numDice = numDice;
        this.diceSize = diceSize;
        this.modifier = modifier;
    }
}
