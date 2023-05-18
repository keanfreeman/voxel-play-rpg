using DieNamespace;
using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    public System.Random rng { get; private set; } = new System.Random();

    public int RollAbilityCheck(int modifier, Advantage advantage = Advantage.None) {
        Die straightD20 = new(1, 20, 0);
        int roll = Roll(straightD20);
        if (advantage == Advantage.Advantage) {
            roll = Mathf.Max(roll, Roll(straightD20));
        }
        else if (advantage == Advantage.Disadvantage) {
            roll = Mathf.Min(roll, Roll(straightD20));
        }
        return roll + modifier;
    }
    
    // todo - convey if there was a critical hit (e.g. have damageroll method instead)
    private int Roll(int numDice, int diceSize, int modifier) {
        int sum = 0;
        for (int i = 0; i < numDice; i++) {
            int roll = rng.Next(1, diceSize + 1);
            Debug.Log($"Rolled a {roll} on a d{diceSize}.");
            sum += roll;
        }
    
        return sum + modifier;
    }
    // todo - convey if there was a critical hit
    private int Roll(Die dice) {
        return Roll(dice.numDice, dice.diceSize, dice.modifier);
    }

    public enum Advantage {
        None,
        Advantage,
        Disadvantage,
        Both
    }

    public static class AdvantageCalcs {
        public static Advantage GetNewAdvantageState(Advantage original, Advantage next) {
            if (original == Advantage.Both || next == Advantage.Both) {
                return Advantage.Both;
            }
            if (original == Advantage.None) {
                return next;
            }
            if (next == Advantage.None) {
                return original;
            }
            if (original == Advantage.Advantage && next == Advantage.Advantage
                    || original == Advantage.Disadvantage && next == Advantage.Disadvantage) {
                return original;
            }

            return Advantage.Both;
        }
    }
}
