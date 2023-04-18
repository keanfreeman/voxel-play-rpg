using CustomComponents;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ConstructionBox : VisualElement {
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<ConstructionBox> { }

    public VisualElement topOption { get; private set; }
    public VisualElement bottomOption { get; private set; }

    private VisualTreeAsset optionPickerAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
        "Assets/UI/Construction/OptionPicker.uxml");
    // using these instead of one dict because I need iteration and reversibility
    private List<string> topOptions = new List<string> {"Top Option 1"};
    private List<List<string>> bottomOptions = new List<List<string>> { 
        { new List<string> { "Bottom Option 1" } } };
    private int topIterator = 0;
    private int bottomIterator = 0;

    private Label topText;
    private Label bottomText;

    public ConstructionBox() {
        topOption = optionPickerAsset.Instantiate().Q<VisualElement>("OptionPicker");
        bottomOption = optionPickerAsset.Instantiate().Q<VisualElement>("OptionPicker");
        Add(topOption);
        Add(bottomOption);

        topText = topOption.Q<Label>();
        bottomText = bottomOption.Q<Label>();

        InitUI(topOptions, bottomOptions);
    }

    public void InitUI(List<string> topOptions, List<List<string>> bottomOptions) {
        this.topOptions = topOptions;
        this.bottomOptions = bottomOptions;
        topIterator = 0;
        bottomIterator = 0;

        topText.text = topOptions[topIterator];
        bottomText.text = bottomOptions[topIterator][bottomIterator];
    }

    public void IterateTop(bool isRight) {
        topIterator += isRight ? 1 : -1;
        if (topIterator >= topOptions.Count) {
            topIterator = 0;
        }
        else if (topIterator < 0) {
            topIterator = topOptions.Count - 1;
        }
        bottomIterator = 0;

        topText.text = topOptions[topIterator];
        bottomText.text = bottomOptions[topIterator][bottomIterator];
    }

    public void IterateBottom(bool isRight) {
        bottomIterator += isRight ? 1 : -1;
        if (bottomIterator >= bottomOptions.Count) {
            bottomIterator = 0;
        }
        else if (bottomIterator < 0) {
            bottomIterator = bottomOptions.Count - 1;
        }

        bottomText.text = bottomOptions[topIterator][bottomIterator];
    }
}
