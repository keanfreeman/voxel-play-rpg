using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice
{
    private System.Random rng;
    public Dice(System.Random rng) {
        this.rng = rng;
    }

    public int Roll(int dieSize) {
        return Roll(0, dieSize, 0);
    }

    public int Roll(int numDice, int dieSize) {
        return Roll(numDice, dieSize, 0);
    }

    public int Roll(int numDice, int dieSize, int modifier) {
        int sum = 0;
        for (int i = 0; i < numDice; i++) {
            sum += rng.Next(1, dieSize + 1);
        }
        return sum + modifier;
    }
}
