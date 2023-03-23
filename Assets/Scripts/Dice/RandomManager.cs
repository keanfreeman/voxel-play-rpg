using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    public System.Random rng { get; private set; }

    void Awake() {
        rng = new System.Random();
    }

    public int Roll(int numDice, int diceSize, int modifier) {
        int sum = 0;
        for (int i = 0; i < numDice; i++) {
            sum += rng.Next(1, diceSize + 1);
        }
        return sum + modifier;
    }

    public int Roll(Dice dice) {
        return Roll(dice.numDice, dice.diceSize, dice.modifier);
    }
}
