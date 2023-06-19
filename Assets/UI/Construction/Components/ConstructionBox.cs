using CustomComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelPlay;
using static ConstructionOptions;

public class ConstructionBox : VisualElement {
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<ConstructionBox> { }

    public OptionPicker topOption;
    public OptionPicker middleOption;
    public OptionPicker bottomOption;

    private Label topText;
    private Label middleText;
    private Label bottomText;
    private const string styleResource = "ConstructionStyle";

    public ConstructionBox() {
        StyleSheet styleSheet = Resources.Load<StyleSheet>(styleResource);
        styleSheets.Add(styleSheet);

        topOption = new OptionPicker();
        middleOption = new OptionPicker();
        bottomOption = new OptionPicker();
        SetThirdOptionDisplay(DisplayStyle.None);

        Add(topOption);
        Add(middleOption);
        Add(bottomOption);

        topText = topOption.label;
        middleText = middleOption.label;
        bottomText = bottomOption.label;

        RenderOptions(new ConstructionOptions());
    }

    public void RenderOptions(ConstructionOptions constructionOptions) {
        TopBuildOption topBuildOption = constructionOptions.GetCurrTopBuildOption();
        topText.text = topBuildOption.ToString();
        middleText.text = constructionOptions.GetCurrOptionName();

        if (topBuildOption == TopBuildOption.Voxels) {
            SetThirdOptionDisplay(DisplayStyle.Flex);
            bottomText.text = constructionOptions.GetCurrVoxelBuildModeOption().ToString();
        }
        else {
            SetThirdOptionDisplay(DisplayStyle.None);
        }
    }

    private void SetThirdOptionDisplay(DisplayStyle display) {
        bottomOption.style.display = display;
    }
}

public class ConstructionOptions {
    public enum TopBuildOption {
        Voxels = 0,
        Objects = 1,
        Destroy = 2
    }
    private int topBuildOptionIterator = 0;
    public enum VoxelBuildModeOption {
        Cuboid = 0,
        Tunnel = 1
    }
    private int voxelBuildModeIterator = 0;

    public List<VoxelDefinition> voxelOptions { get; private set; }
    private int voxelOptionsIterator = 0;
    public List<ObjectIdentitySO> objectOptions { get; private set; }
    private int objectOptionsIterator = 0;

    public ConstructionOptions() {
        voxelOptions = new List<VoxelDefinition>();
        objectOptions = new List<ObjectIdentitySO>();
    }

    public ConstructionOptions(List<VoxelDefinition> vds, List<ObjectIdentitySO> objects) {
        voxelOptions = vds;
        objectOptions = objects;
    }

    public TopBuildOption GetCurrTopBuildOption() {
        return (TopBuildOption)topBuildOptionIterator;
    }

    public VoxelDefinition GetCurrVoxelDefinition() {
        if (voxelOptions.Count == 0) {
            return null;
        }
        return voxelOptions[voxelOptionsIterator];
    }

    public VoxelBuildModeOption GetCurrVoxelBuildModeOption() {
        return (VoxelBuildModeOption)voxelBuildModeIterator;
    }

    public ObjectIdentitySO GetCurrObject() {
        if (objectOptions.Count == 0) {
            return null;
        }
        return objectOptions[objectOptionsIterator];
    }

    public string GetCurrOptionName() {
        TopBuildOption buildOption = GetCurrTopBuildOption();
        if (buildOption == TopBuildOption.Voxels) {
            VoxelDefinition vd = GetCurrVoxelDefinition();
            string displayName = vd == null ? "No definition here" 
                : vd.name == "Null" ? "Empty Square" 
                : vd.name;
            return displayName;
        }
        else if (buildOption == TopBuildOption.Objects) {
            ObjectIdentitySO objectID = GetCurrObject();
            return objectID == null ? "No Objects available" 
                : objectID.prefab.name;
        }
        else return "Destroy Object/Voxel";
    }

    public void IterateTop(bool isRight) {
        topBuildOptionIterator += isRight ? 1 : -1;
        int enumNumOptions = Enum.GetNames(typeof(TopBuildOption)).Length;
        if (topBuildOptionIterator >= enumNumOptions) {
            topBuildOptionIterator = 0;
        }
        else if (topBuildOptionIterator < 0) {
            topBuildOptionIterator = enumNumOptions - 1;
        }
    }

    public void IterateMiddle(bool isRight) {
        int iteration = isRight ? 1 : -1;

        if (GetCurrTopBuildOption() == TopBuildOption.Voxels) {
            voxelOptionsIterator += iteration;
            if (voxelOptionsIterator >= voxelOptions.Count) {
                voxelOptionsIterator = 0;
            }
            else if (voxelOptionsIterator < 0) {
                voxelOptionsIterator = voxelOptions.Count - 1;
            }
        }
        else {
            objectOptionsIterator += iteration;
            if (objectOptionsIterator >= objectOptions.Count) {
                objectOptionsIterator = 0;
            }
            else if (objectOptionsIterator < 0) {
                objectOptionsIterator = objectOptions.Count - 1;
            }
        }
    }

    public void IterateBottom(bool isRight) {
        if (GetCurrTopBuildOption() != TopBuildOption.Voxels) return;

        voxelBuildModeIterator += isRight ? 1 : -1;
        int enumNumOptions = Enum.GetNames(typeof(VoxelBuildModeOption)).Length;
        if (voxelBuildModeIterator >= enumNumOptions) {
            voxelBuildModeIterator = 0;
        }
        else if (voxelBuildModeIterator < 0) {
            voxelBuildModeIterator = enumNumOptions - 1;
        }
    }
}
