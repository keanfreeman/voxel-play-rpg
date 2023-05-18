using DieNamespace;
using Instantiated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RandomManager;

public class VisualRollManager : MonoBehaviour {
    [SerializeField] DiceThrowerManager diceThrowerManager;
    [SerializeField] DiceRollerUIController diceUIController;

    // adjustable
    [SerializeField] float TEXT_DISPLAY_WAIT = 0.5f;

    public IEnumerator RollAttack(int modifier, int targetAC, Advantage advantage) {
        string displayText = "Rolling attack roll";

        List<Die> diceToBeRolled = new();
        if (advantage != Advantage.Advantage && advantage != Advantage.Disadvantage) {
            diceToBeRolled.Add(new(1, 20));
        }
        else {
            diceToBeRolled.Add(new(2, 20));
            displayText += advantage == Advantage.Advantage ? " with Advantage" : " with Disadvantage";
        }
        yield return diceUIController.DisplayText(displayText);

        CoroutineWithData coroutineWithData = new(this, diceThrowerManager.RollDice(diceToBeRolled));
        yield return coroutineWithData.coroutine;
        DiceResult rollResult = (DiceResult)coroutineWithData.result;

        int finalNum;
        if (advantage == Advantage.Advantage) {
            finalNum = Mathf.Max(rollResult.rolls[0].Item2, rollResult.rolls[1].Item2);
        }
        else if (advantage == Advantage.Disadvantage) {
            finalNum = Mathf.Min(rollResult.rolls[0].Item2, rollResult.rolls[1].Item2);
        }
        else {
            finalNum = rollResult.rolls[0].Item2;
        }
        bool isCritical = finalNum == 20;
        finalNum += modifier;

        if (finalNum >= targetAC) {
            if (isCritical) {
                displayText = "Critical Hit!";
            }
            else displayText = $"{finalNum} Hits!";
        }
        else displayText = $"{finalNum} Misses!";
        yield return diceUIController.DisplayText(displayText);
        yield return new WaitForSeconds(TEXT_DISPLAY_WAIT);
        yield return diceUIController.Hide();

        yield return new AttackResult(finalNum, isCritical);
    }

    public IEnumerator RollSavingThrow(string displayText, int modifier, int targetDC, 
            Advantage advantage = Advantage.None) {
        List<Die> diceToBeRolled = new();
        if (advantage != Advantage.Advantage && advantage != Advantage.Disadvantage) {
            diceToBeRolled.Add(new(1, 20));
        }
        else {
            diceToBeRolled.Add(new(2, 20));
            displayText += advantage == Advantage.Advantage ? " with Advantage" : " with Disadvantage";
        }
        yield return diceUIController.DisplayText(displayText);

        CoroutineWithData coroutineWithData = new(this, diceThrowerManager.RollDice(diceToBeRolled));
        yield return coroutineWithData.coroutine;
        DiceResult rollResult = (DiceResult)coroutineWithData.result;

        int finalNum;
        if (advantage == Advantage.Advantage) {
            finalNum = Mathf.Max(rollResult.rolls[0].Item2, rollResult.rolls[1].Item2);
        }
        else if (advantage == Advantage.Disadvantage) {
            finalNum = Mathf.Min(rollResult.rolls[0].Item2, rollResult.rolls[1].Item2);
        }
        else {
            finalNum = rollResult.rolls[0].Item2;
        }
        finalNum += modifier;

        if (finalNum >= targetDC) {
            displayText = $"{finalNum} Succeeds!";
        }
        else displayText = $"{finalNum} Fails! DC{targetDC}";
        yield return diceUIController.DisplayText(displayText);
        yield return new WaitForSeconds(TEXT_DISPLAY_WAIT);
        yield return diceUIController.Hide();

        yield return finalNum;
    }

    public IEnumerator RollDamage(List<Die> damageRolls, bool isCritical) {
        yield return diceUIController.DisplayText("Rolling damage");

        int modifiersSum = 0;
        List<Die> totalRolls = new();
        foreach (Die dieGroup in damageRolls) {
            modifiersSum += dieGroup.modifier;
            if (isCritical) {
                totalRolls.Add(new Die(dieGroup.numDice * 2, dieGroup.diceSize));
            }
            else totalRolls.Add(dieGroup);
        }

        CoroutineWithData coroutineWithData = new(this, diceThrowerManager.RollDice(totalRolls));
        yield return coroutineWithData.coroutine;
        DiceResult rollResult = (DiceResult)coroutineWithData.result;

        int totalDamage = rollResult.sum + modifiersSum;
        yield return diceUIController.DisplayText($"Rolled {totalDamage} damage!");
        yield return new WaitForSeconds(TEXT_DISPLAY_WAIT);
        yield return diceUIController.Hide();

        yield return totalDamage;
    }
}

public class AttackResult {
    public int rolled;
    public bool isCritical;

    public AttackResult(int result, bool isCritical) {
        this.rolled = result;
        this.isCritical = isCritical;
    }

    public override string ToString() {
        return $"{rolled}";
    }
}
