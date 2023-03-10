using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using VoxelPlay;

public class Pathfinder
{
    private VoxelPlayEnvironment environment;
    private SpriteMovement spriteMovement;


    PriorityQueue<Node, int> priorityQueue = new PriorityQueue<Node, int>();
    List<Node> frontier = new List<Node>();
    Dictionary<Vector3Int, Node> positionToNode = new Dictionary<Vector3Int, Node>();
    List<Vector3Int> path = new List<Vector3Int>();

    public Pathfinder(VoxelPlayEnvironment environment, SpriteMovement spriteMovement) {
        this.environment = environment;
        this.spriteMovement = spriteMovement;
    }

    public List<Vector3Int> FindPath(Node start, Node end) {
        Debug.Log($"Finding path to {end.position}");

        if (!spriteMovement.IsReachablePosition(end.position)) {
            Debug.Log("End position isn't reachable.");
            return path;
        }

        start.score = 0;
        start.heuristicScore = 0;

        frontier.Add(start);
        frontier.Add(end);
        positionToNode[end.position] = end;

        Node currNode;
        while (true) {
            if (frontier.Count > 10000) {
                return path;
            }

            currNode = GetLowestHeuristicScoreUnvisited();
            if (currNode == null || currNode.score >= 1000) {
                Debug.Log("Path would be too long, stopping search.");
                return path;
            }
            currNode.visited = true;

            // get neighbors of current node
            foreach (Vector3Int coordinate in Coordinates.GetAdjacentCoordinates(currNode.position)) {
                Node neighborNode;
                if (!positionToNode.ContainsKey(coordinate)) {
                    positionToNode[coordinate] = new Node(coordinate);
                    frontier.Add(positionToNode[coordinate]);
                }
                neighborNode = positionToNode[coordinate];
                currNode.neighbors.Add(neighborNode);
            }

            // update neighbor costs
            foreach (Node neighbor in currNode.neighbors) {
                if (!neighbor.visited) {
                    float distanceToNeighbor = CalculateScore(currNode, neighbor);
                    if (distanceToNeighbor < neighbor.score) {
                        neighbor.score = distanceToNeighbor;
                        neighbor.heuristicScore = neighbor.score 
                            + CalculateHeuristicScore(neighbor, end);
                        neighbor.prevNode = currNode;
                    }
                }
            }
            if (currNode == end) {
                Debug.Log("Finished path");
                while (true) {
                    path.Add(currNode.position);
                    if (currNode.prevNode == null) {
                        return path;
                    }
                    currNode = currNode.prevNode;
                }
            }
        }
    }

    private float CalculateScore(Node node1, Node node2) {
        if (spriteMovement.IsATraversibleFromB(node2.position, node1.position)) {
            return Mathf.Abs((node2.position - node1.position).magnitude);
        }
        return float.MaxValue;
    }

    private float CalculateHeuristicScore(Node curr, Node end) {
        return Mathf.Abs((end.position - curr.position).magnitude);
    }

    private Node GetLowestHeuristicScoreUnvisited() {
        float lowestScore = float.MaxValue;
        Node lowestNode = null;
        foreach (Node node in frontier) {
            if (node.heuristicScore < lowestScore && !node.visited) {
                lowestScore = node.heuristicScore;
                lowestNode = node;
            }
        }
        return lowestNode;
    }
}
