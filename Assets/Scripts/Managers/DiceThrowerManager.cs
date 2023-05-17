using DieNamespace;
using Ink.Parsed;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceThrowerManager : MonoBehaviour
{
    [SerializeField] RandomManager randomManager;

    [SerializeField] Table table;
    [SerializeField] MeshRenderer wall1;
    [SerializeField] MeshRenderer wall2;
    [SerializeField] MeshRenderer wall3;
    [SerializeField] MeshRenderer wall4;
    [SerializeField] MeshRenderer tableSurface;

    private const float FADE_TIME = 1f;

    private void Awake() {
        gameObject.SetActive(false);
    }

    // does not add modifiers
    public IEnumerator RollDice(List<Die> dice) {
        gameObject.SetActive(true);

        foreach (Die die in dice) {
            if (die.diceSize == 4) {
                table.AddDiceD4(die.numDice);
            }
            else if (die.diceSize == 6) {
                table.AddDiceD6(die.numDice);
            }
            else if (die.diceSize == 8) {
                table.AddDiceD8(die.numDice);
            }
            else if (die.diceSize == 10) {
                table.AddDiceD10(die.numDice);
            }
            else if (die.diceSize == 12) {
                table.AddDiceD12(die.numDice);
            }
            else if (die.diceSize == 20) {
                table.AddDiceD20(die.numDice);
            }
            else {
                throw new NotImplementedException($"Cannot physically roll a die " +
                    $"of size {die.diceSize}");
            }
        }

        table.ThrowDirectionX = GetRandFloat();
        table.ThrowDirectionY = GetRandFloat();

        table.OnThrowDice();

        while (!table.ThrowResult()) {
            yield return null;
        }

        List<Tuple<DiceTypes, int>> results = new();
        int sum = 0;
        foreach (Dice diceResult in table.DiceThrown) {
            results.Add(new(diceResult.MyDiceType, diceResult.result));
            sum += diceResult.result;
        }
        yield return new DiceResult(results, sum);

        gameObject.SetActive(false);
        table.OnClearDiceList();
    }

    private IEnumerator Fade(bool isFadeIn) {
        float startTransparency = isFadeIn ? 0 : 1;
        float endTransparency = isFadeIn ? 1 : 0;

        float timer = Time.time;
        while (Time.time - timer < FADE_TIME) {
            float fractionDone = (Time.time - timer) / FADE_TIME;
            float currTransparency = Mathf.Lerp(startTransparency, endTransparency, fractionDone);
            SetTransparency(currTransparency);
            yield return null;
        }
        SetTransparency(endTransparency);
    }

    private void SetTransparency(float alpha) {
        SetMaterialTransparency(wall1.material, alpha);
        SetMaterialTransparency(wall2.material, alpha);
        SetMaterialTransparency(wall3.material, alpha);
        SetMaterialTransparency(wall4.material, alpha);
        SetMaterialTransparency(tableSurface.material, alpha);

        //foreach (Dice dice in table.DiceThrown) {
        //    SetMaterialTransparency(dice.dieRenderer.material, alpha);
        //}
    }

    private void SetMaterialTransparency(Material material, float alpha) {
        material.color = new(material.color.r, material.color.g, material.color.b, alpha);
    }

    private float GetRandFloat() {
        // -1 to 1
        float result = ((float)randomManager.rng.NextDouble() * 2) - 1;
        return result;
    }
}

public struct DiceResult {
    public List<Tuple<DiceTypes, int>> rolls;
    public int sum;

    public DiceResult(List<Tuple<DiceTypes, int>> results, int sum) {
        this.rolls = results;
        this.sum = sum;
    }
}
