using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using VoxelPlay;

public class Pathfinder
{
    private SpriteMovement spriteMovement;

    private const int MAX_PATH_LENGTH = 1000;

    PriorityQueue<Node, float> frontier = new PriorityQueue<Node, float>();
    Dictionary<Vector3Int, Node> positionToNode = new Dictionary<Vector3Int, Node>();
    // keeps track of changed values since we can't update them directly in the priority queue
    private Dictionary<Node, float> changedNodes = new Dictionary<Node, float>();
    List<Vector3Int> path = new List<Vector3Int>();

    public Pathfinder(SpriteMovement spriteMovement) {
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

        frontier.Enqueue(start, start.heuristicScore);
        positionToNode[start.position] = start;

        Node currNode;
        int numLoops = 0;
        while (true) {
            if (numLoops > 10000) {
                Debug.LogError("Ran into infinite loop");
                return path;
            }
            numLoops += 1;

            currNode = GetLowestHeuristicScoreUnvisited();
            if (currNode == null || currNode.score >= MAX_PATH_LENGTH) {
                Debug.Log("Path would be too long, stopping search.");
                return path;
            }
            currNode.visited = true;

            // get neighbors of current node
            foreach (Vector3Int coordinate in Coordinates.GetAdjacentCoordinates(currNode.position)) {
                Node neighbor;
                if (!positionToNode.ContainsKey(coordinate)) {
                    neighbor = new Node(coordinate);
                    positionToNode[coordinate] = neighbor;
                }
                neighbor = positionToNode[coordinate];
                currNode.neighbors.Add(neighbor);

                // update neighbor costs
                if (!neighbor.visited) {
                    float newScore = CalculateDistance(currNode, neighbor) + currNode.score;
                    if (newScore < neighbor.score) {
                        neighbor.score = newScore;
                        neighbor.heuristicScore = neighbor.score + CalculateDirectLineLength(neighbor, end);
                        neighbor.prevNode = currNode;

                        // priority has changed
                        if (!changedNodes.ContainsKey(neighbor)) {
                            changedNodes[neighbor] = float.MaxValue;
                        }
                        changedNodes[neighbor] = neighbor.heuristicScore;
                    }
                    frontier.Enqueue(neighbor, neighbor.heuristicScore);
                }
            }

            if (currNode.position == end.position) {
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

    private float CalculateDistance(Node node1, Node node2) {
        if (spriteMovement.IsATraversibleFromB(node2.position, node1.position)) {
            return CalculateDirectLineLength(node2, node1);
        }
        return float.MaxValue;
    }

    private float CalculateDirectLineLength(Node curr, Node end) {
        return Mathf.Abs((end.position - curr.position).magnitude);
    }

    private Node GetLowestHeuristicScoreUnvisited() {
        while (true) {
            if (frontier.Count == 0) {
                return null;
            }

            // get rid of visited or outdated nodes
            Node topNode = frontier.Peek();
            if (topNode.visited) {
                frontier.Dequeue();
                continue;
            }
            if (changedNodes.ContainsKey(topNode) && changedNodes[topNode] < topNode.heuristicScore) {
                frontier.Dequeue();
                frontier.Enqueue(topNode, changedNodes[topNode]);
                continue;
            }
            break;
        }

        return frontier.Dequeue();
    }
}
