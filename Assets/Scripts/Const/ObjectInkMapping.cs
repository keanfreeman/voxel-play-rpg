using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Ink.Runtime;
using VoxelPlay;

public class ObjectInkMapping : MonoBehaviour
{
    [SerializeField] private TextAsset NoResponseObjectInk;
    [SerializeField] private TextAsset testInk;

    public Story GetStoryFromObject(GameObject objectIn) {
        if (objectIn.tag == "NPC") {
            return new Story(testInk.text);
        }

        return null;
    }

    public Story GetStoryFromVoxel(Voxel voxel) {
        string voxelName = voxel.type.name;
        if (voxelName == "BedVoxel" || voxelName == "EmptyBedVoxel") {
            Story story = new Story(NoResponseObjectInk.text);
            story.ChoosePathString("bed");
            return story;
        }

        return null;
    }
}
