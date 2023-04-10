using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    public Vector3Int origin { get; private set; }
    public float score { get; set; } = float.MaxValue;
    public float heuristicScore { get; set; } = float.MaxValue;
    public bool visited { get; set; } = false;
    public List<Node> neighbors { get; private set; }
    public Node prevNode { get; set; } = null;

    public Node(Vector3Int origin) {
        this.origin = origin;
        neighbors = new List<Node>();
        neighbors.Capacity = 26;
    }
}
