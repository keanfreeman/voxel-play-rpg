using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Image healthBar;
    [SerializeField] TextMeshProUGUI textUI;

    private void Awake() {
        canvas.enabled = false;
    }

    public void SetHealth(int curr, int max) {
        if (curr >= max) {
            canvas.enabled = false;
            return;
        }
        canvas.enabled = true;

        float fraction = Mathf.Max(0, (float)curr / max);
        healthBar.fillAmount = fraction;
        textUI.text = $"{Mathf.Max(0, curr)}/{max}";
    }
}
