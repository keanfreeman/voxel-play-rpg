using CustomComponents;
using EntityDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelPlay;

public class ConstructionBox : VisualElement {
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<ConstructionBox> { }

    public VisualElement topOption;
    public VisualElement bottomOption;

    private Label topText;
    private Label bottomText;
    private VisualTreeAsset optionPickerAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
        "Assets/UI/Construction/OptionPicker.uxml");

    public ConstructionBox() {
        topOption = optionPickerAsset.Instantiate().Q<VisualElement>("OptionPicker");
        bottomOption = optionPickerAsset.Instantiate().Q<VisualElement>("OptionPicker");
        Add(topOption);
        Add(bottomOption);

        topText = topOption.Q<Label>();
        bottomText = bottomOption.Q<Label>();

        RenderOptions(new ConstructionOptions());
    }

    public void RenderOptions(ConstructionOptions constructionOptions) {
        topText.text = constructionOptions.GetCurrBuildOption().ToString();
        bottomText.text = constructionOptions.GetCurrOptionName();
    }
}

public class ConstructionOptions {
    public enum BuildOption {
        Voxels = 0,
        Objects = 1
    }
    private int buildOptionIterator = 0;

    public List<VoxelDefinition> voxelOptions { get; private set; }
    private int voxelOptionsIterator = 0;
    public List<ObjectIdentity> objectOptions { get; private set; }
    private int objectOptionsIterator = 0;

    public ConstructionOptions() {
        voxelOptions = new List<VoxelDefinition>();
        objectOptions = new List<ObjectIdentity>();
    }

    public ConstructionOptions(List<VoxelDefinition> vds, List<ObjectIdentity> objects) {
        voxelOptions = vds;
        objectOptions = objects;
    }

    public BuildOption GetCurrBuildOption() {
        return (BuildOption)buildOptionIterator;
    }

    public VoxelDefinition GetCurrVoxelDefinition() {
        if (voxelOptions.Count == 0) {
            return null;
        }
        return voxelOptions[voxelOptionsIterator];
    }

    public ObjectIdentity GetCurrObject() {
        if (objectOptions.Count == 0) {
            return null;
        }
        return objectOptions[objectOptionsIterator];
    }

    public string GetCurrOptionName() {
        if (GetCurrBuildOption() == BuildOption.Voxels) {
            VoxelDefinition vd = GetCurrVoxelDefinition();
            return vd == null ? "No Voxel Definitions available" : vd.name;
        }
        else {
            ObjectIdentity objectID = GetCurrObject();
            return objectID == null ? "No Objects available" 
                : objectID.prefab.name;
        }
    }

    public void IterateTop(bool isRight) {
        buildOptionIterator += isRight ? 1 : -1;
        int enumNumOptions = Enum.GetNames(typeof(BuildOption)).Length;
        if (buildOptionIterator >= enumNumOptions) {
            buildOptionIterator = 0;
        }
        else if (buildOptionIterator < 0) {
            buildOptionIterator = enumNumOptions - 1;
        }
    }

    public void IterateBottom(bool isRight) {
        int iteration = isRight ? 1 : -1;

        if (GetCurrBuildOption() == BuildOption.Voxels) {
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
}
