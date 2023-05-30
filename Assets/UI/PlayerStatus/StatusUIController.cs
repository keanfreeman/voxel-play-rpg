using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CustomComponents;
using System.Linq;
using GameMechanics;
using UnityEditor;

public class StatusUIController : MonoBehaviour
{
    // part of this object
    [SerializeField] private UIDocument statusUIDocument;
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private ThemeStyleSheet defaultThemeStyleSheet;
    [SerializeField] private PanelSettings referencePanelSettings;

    VisualElement statusEffectStack;

    private void Awake() {
        Material newMaterial = new Material(Shader.Find("Unlit/Transparent Cutout"));
        RenderTexture newTexture = new(200, 200, 0) {
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D
        };
        newTexture.Create();
        newMaterial.mainTexture = newTexture;
        meshRenderer.material = newMaterial;

        PanelSettings newPanelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        newPanelSettings.targetTexture = newTexture;
        // bug with ui toolkit - must grab from a panelsettings from the editor:
        // https://forum.unity.com/threads/can-i-create-panelsettings-and-rendertexture-at-runtime-for-using-with-uitoolkit.995317/
        newPanelSettings.themeStyleSheet = defaultThemeStyleSheet;
        newPanelSettings.scaleMode = referencePanelSettings.scaleMode;
        newPanelSettings.referenceResolution = referencePanelSettings.referenceResolution;
        newPanelSettings.clearColor = referencePanelSettings.clearColor;
        newPanelSettings.colorClearValue = referencePanelSettings.colorClearValue;
        statusUIDocument.panelSettings = newPanelSettings;

        statusUIDocument.enabled = true;

        statusEffectStack = statusUIDocument.rootVisualElement.Q<VisualElement>("StatusEffectStack");
        statusEffectStack.Clear();
    }

    public void SetStatuses(CurrentStatus currentStatus) {
        statusEffectStack.Clear();

        List<string> statusStrings = new();
        foreach (OngoingEffect ongoingEffect in currentStatus.GetOngoingEffects()) {
            string statusString = $"{ongoingEffect.cause}";
            if (ongoingEffect.conditions.Count > 0) {
                string conditionsString = " (";
                int numConditions = ongoingEffect.conditions.Count;
                int iterator = 0;
                foreach (Condition condition in ongoingEffect.conditions) {
                    conditionsString += condition;
                    bool isLastElement = iterator >= numConditions - 1;
                    if (!isLastElement) {
                        conditionsString += ", ";
                    }
                    else {
                        conditionsString += ")";
                    }
                    iterator++;
                }
                statusString += conditionsString;
            }

            statusStrings.Add(statusString);
        }

        foreach (string statusString in statusStrings) {
            statusEffectStack.Add(new StatusComponent(statusString));
        }
    }
}
