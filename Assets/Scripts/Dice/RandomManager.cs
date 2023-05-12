using GameMechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    public System.Random rng { get; private set; }

    void Awake() {
        rng = new System.Random();
    }

    public AttackRoll RollAttack(Dice dice, Advantage advantage = Advantage.Normal) {
        return RollAttack(dice.modifier, advantage);
    }

    public AttackRoll RollAttack(int modifier, Advantage advantage = Advantage.Normal) {
        Dice straightD20 = new(1, 20, 0);
        int roll = rng.Next(1, 21);
        if (advantage == Advantage.Advantage) {
            roll = Mathf.Max(roll, Roll(straightD20));
        }
        else if (advantage == Advantage.Disadvantage) {
            roll = Mathf.Min(roll, Roll(straightD20));
        }

        bool isCritical = roll == 20;
        if (isCritical) {
            Debug.Log("Critical hit!");
        }
        return new AttackRoll(roll + modifier, isCritical);
    }

    public int RollDamage(Dice damageRoll, bool isCritical) {
        if (isCritical) {
            return Roll(damageRoll.numDice * 2, damageRoll.diceSize, damageRoll.modifier);
        }
        return Roll(damageRoll);
    }

    public int RollAbilityCheck(int modifier, Advantage advantage = Advantage.Normal) {
        Dice straightD20 = new(1, 20, 0);
        int roll = Roll(straightD20);
        if (advantage == Advantage.Advantage) {
            roll = Mathf.Max(roll, Roll(straightD20));
        }
        else if (advantage == Advantage.Disadvantage) {
            roll = Mathf.Min(roll, Roll(straightD20));
        }
        return roll + modifier;
    }

    public int RollSavingThrow(int modifier, Advantage advantage = Advantage.Normal) {
        return RollAbilityCheck(modifier, advantage);
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
    private int Roll(Dice dice) {
        return Roll(dice.numDice, dice.diceSize, dice.modifier);
    }

    public class AttackRoll {
        public int result;
        public bool isCritical;

        public AttackRoll(int result, bool isCritical) {
            this.result = result;
            this.isCritical = isCritical;
        }

        public override string ToString() {
            return $"{result}";
        }
    }

    public enum Advantage {
        Normal,
        Advantage,
        Disadvantage
    }
}
