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
        // todo - get the voxel definitions before runtime somehow
        // to avoid use of strings
        if (voxel.type.name == "BedVoxel" || voxel.type.name == "EmptyBedVoxel") {
            Story story = new Story(NoResponseObjectInk.text);
            story.ChoosePathString("bed");
            return story;
        }

        return null;
    }
}
