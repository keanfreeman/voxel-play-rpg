using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// not currently used for anything, but can create meshes programmatically
[RequireComponent(typeof(MeshFilter))]
public class SlopeMeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape() {
        vertices = new Vector3[] {
            new Vector3(0,0,0),
            new Vector3(0,1,0),
            new Vector3(1,0,0),

            new Vector3(0,0,1),
            new Vector3(0,1,1),
            new Vector3(1,0,1)
        };

        triangles = new int[] {
            //sides
            0,1,2,
            5,4,3,
            //bottom
            0,2,3,
            2,5,3,
            //back
            4,0,3,
            1,0,4,
            //top
            1,5,2,
            1,4,5
        };
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
